using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ShipInteractions : MonoBehaviour
{
    public static ShipInteractions Instance;
    public bool isNight = false;
    public TextController textController;

    private Dictionary<GameObject, GameObject> pirateToCapturedCargo = new();
    private Dictionary<GameObject, HashSet<GameObject>> cargoEvadedPirates = new();
    private Dictionary<(GameObject cargo, GameObject pirate), int> evadeTimestamps = new();

    // For controlling captured pair movement per tick.
    private int lastDownMovementTick = -1;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Called each tick (or replay update) to process interactions.
    public void CheckForInteractions(List<GameObject> allShips)
    {
        List<GameObject> piratesToRemove = new();
        List<GameObject> shipsToRemove = new();

        allShips.RemoveAll(ship => ship == null);

        foreach (GameObject ship in allShips)
        {
            if (ship == null || !ship.activeInHierarchy)
                continue;
            Vector3 shipPos = ship.transform.position;

            foreach (GameObject otherShip in allShips)
            {
                if (otherShip == null || !otherShip.activeInHierarchy)
                    continue;
                if (ship == otherShip)
                    continue;
                Vector3 otherPos = otherShip.transform.position;

                if (ship.CompareTag("Pirate") && otherShip.CompareTag("Patrol") && IsWithinRange(shipPos, otherPos, 3))
                {
                    HandleDefeat(ship, otherShip);
                }
                else if (ship.CompareTag("Pirate") && otherShip.CompareTag("Cargo") && !isNight && IsWithinRange(shipPos, otherPos, 3))
                {
                    HandleCapture(ship, otherShip);
                }
                else if (ship.CompareTag("Pirate") && otherShip.CompareTag("Cargo") && isNight && IsWithinRange(shipPos, otherPos, 2))
                {
                    HandleCapture(ship, otherShip);
                }
                else if (ship.CompareTag("Patrol"))
                {
                    CargoBehavior cargoBehavior = otherShip.GetComponent<CargoBehavior>();
                    if (cargoBehavior != null && cargoBehavior.isCaptured && IsWithinRange(shipPos, otherPos, 3))
                    {
                        HandleRescue(ship);
                    }
                }
                else if (ship.CompareTag("Cargo") && otherShip.CompareTag("Pirate"))
                {
                    bool inOuterRange = IsWithinRange(shipPos, otherShip.transform.position, 4);
                    bool inInnerRange = IsWithinRange(shipPos, otherShip.transform.position, 3);
                    if (inOuterRange && !inInnerRange)
                    {
                        HandleEvasion(ship, otherShip);
                    }
                }
            }
        }

        // Move captured pairs when the simulation tick has advanced.
        if (ShipController.TimeStepCounter != lastDownMovementTick)
        {
            foreach (KeyValuePair<GameObject, GameObject> pair in pirateToCapturedCargo)
            {
                GameObject pirateShip = pair.Key;
                GameObject capturedCargo = pair.Value;

                if (pirateShip == null || capturedCargo == null)
                {
                    piratesToRemove.Add(pirateShip);
                    continue;
                }

                PirateBehavior pirateBehavior = pirateShip.GetComponent<PirateBehavior>();
                CargoBehavior cargoBehavior = capturedCargo.GetComponent<CargoBehavior>();

                if (pirateBehavior != null && cargoBehavior != null)
                {
                    // Determine movement direction: normally move downward; if reversing in replay, move upward.
                    int direction = 1;
                    if (ReplayManager.Instance != null && ReplayManager.Instance.ReplayModeActive && ReplayManager.Instance.replaySpeed < 0)
                        direction = -1;
                    // Move captured pair one grid cell vertically.
                    pirateBehavior.currentGridPosition += Vector2Int.down * direction;
                    pirateShip.transform.position = pirateBehavior.GridToWorld(pirateBehavior.currentGridPosition);

                    cargoBehavior.currentGridPosition = pirateBehavior.currentGridPosition;
                    capturedCargo.transform.position = pirateShip.transform.position;
                }

                if (pirateShip.transform.position.z <= 0)
                {
                    Destroy(pirateShip);
                    Destroy(capturedCargo);
                    piratesToRemove.Add(pirateShip);
                }
            }
            lastDownMovementTick = ShipController.TimeStepCounter;
        }

        // Remove ships at boundaries.
        RemoveShipsAtEdge(allShips, shipsToRemove);
        foreach (GameObject ship in shipsToRemove)
        {
            allShips.Remove(ship);
            Destroy(ship);
        }

        // Cleanup evasion records older than 6 ticks.
        List<(GameObject, GameObject)> evasionCleanup = new();
        foreach (var entry in evadeTimestamps)
        {
            (GameObject cargo, GameObject pirate) = entry.Key;
            int evadeFrame = entry.Value;
            if (ShipController.TimeStepCounter - evadeFrame >= 6)
                evasionCleanup.Add((cargo, pirate));
        }
        foreach (var pair in evasionCleanup)
        {
            textController.UpdateEvasion(true, true);
            evadeTimestamps.Remove(pair);
        }
    }

    private void RemoveShipsAtEdge(List<GameObject> allShips, List<GameObject> shipsToRemove)
    {
        foreach (GameObject ship in allShips)
        {
            if (ship == null)
                continue;
            Vector3 pos = ship.transform.position;
            if (ship.CompareTag("Cargo") && pos.x > 399)
            {
                shipsToRemove.Add(ship);
                textController.UpdateShipExit("cargo");
            }
            else if (ship.CompareTag("Patrol") && Mathf.RoundToInt(pos.x) <= 0)
            {
                Debug.Log($"[Exit] {ship.name} reached the left edge and was removed.");
                shipsToRemove.Add(ship);
                textController.UpdateShipExit("patrol");
            }
            else if (ship.CompareTag("Pirate") && pos.z > 99 && !pirateToCapturedCargo.ContainsKey(ship))
            {
                shipsToRemove.Add(ship);
                textController.UpdateShipExit("pirate");
            }
        }
    }

    private bool IsWithinRange(Vector3 pos1, Vector3 pos2, float range)
    {
        float dx = Mathf.Abs(pos1.x - pos2.x);
        float dz = Mathf.Abs(pos1.z - pos2.z);
        return dx <= range && dz <= range;
    }

    private void HandleDefeat(GameObject pirate, GameObject patrol)
    {
        if (pirate == null || patrol == null)
            return;
        if (pirateToCapturedCargo.TryGetValue(pirate, out GameObject cargo))
        {
            if (cargo != null)
            {
                CargoBehavior cargoBehavior = cargo.GetComponent<CargoBehavior>();
                if (cargoBehavior != null)
                    cargoBehavior.isCaptured = false;
            }
            pirateToCapturedCargo.Remove(pirate);
        }
        textController.PirateDestroyed();
        Destroy(pirate);
    }

    private void HandleCapture(GameObject pirate, GameObject cargo)
    {
        if (pirateToCapturedCargo.ContainsValue(cargo))
            return;
        if (pirateToCapturedCargo.ContainsKey(pirate))
            return;

        CargoBehavior cargoBehavior = cargo.GetComponent<CargoBehavior>();
        if (cargoBehavior != null && cargoBehavior.isCaptured)
            return;

        if (evadeTimestamps.ContainsKey((cargo, pirate)))
        {
            textController.UpdateEvasion(false, false);
            evadeTimestamps.Remove((cargo, pirate));
        }

        if (cargoBehavior != null)
            cargoBehavior.isCaptured = true;

        pirateToCapturedCargo[pirate] = cargo;

        PirateBehavior pirateBehavior = pirate.GetComponent<PirateBehavior>();
        if (pirateBehavior != null)
            pirateBehavior.hasCargo = true;

        // Align pirate with cargo.
        if (pirateBehavior != null && cargoBehavior != null)
        {
            pirateBehavior.currentGridPosition = cargoBehavior.currentGridPosition;
            pirate.transform.position = cargo.transform.position;
        }

        textController.UpdateCaptures(true);
    }

    private void HandleRescue(GameObject patrol)
    {
        if (patrol == null)
            return;
        List<GameObject> toRescue = new();
        foreach (var pair in pirateToCapturedCargo)
        {
            GameObject pirate = pair.Key;
            GameObject cargoCandidate = pair.Value;
            if (cargoCandidate == null || pirate == null)
                continue;
            Vector3 patrolPos = patrol.transform.position;
            Vector3 cargoPos = cargoCandidate.transform.position;
            if (IsWithinRange(patrolPos, cargoPos, 3))
                toRescue.Add(cargoCandidate);
        }

        foreach (GameObject cargoToRescue in toRescue)
        {
            GameObject capturingPirate = null;
            foreach (var pair in pirateToCapturedCargo)
            {
                if (pair.Value == cargoToRescue)
                {
                    capturingPirate = pair.Key;
                    break;
                }
            }
            if (capturingPirate != null)
            {
                pirateToCapturedCargo.Remove(capturingPirate);
                capturingPirate.SetActive(false);
                Destroy(capturingPirate);
            }
            CargoBehavior cargoBehavior = cargoToRescue.GetComponent<CargoBehavior>();
            if (cargoBehavior != null)
            {
                cargoBehavior.isCaptured = false;
                cargoToRescue.tag = "Cargo";
            }
        }
        textController.UpdateCaptures(false);
    }

    private void HandleEvasion(GameObject cargo, GameObject pirate)
    {
        if (pirateToCapturedCargo.ContainsKey(pirate))
            return;
        CargoBehavior cargoBehavior = cargo.GetComponent<CargoBehavior>();
        if (cargoBehavior == null)
            return;
        if (cargoBehavior.isEvadingThisStep)
            return;
        if (!cargoEvadedPirates.ContainsKey(cargo))
            cargoEvadedPirates[cargo] = new HashSet<GameObject>();
        if (cargoEvadedPirates[cargo].Contains(pirate))
            return;

        cargoEvadedPirates[cargo].Add(pirate);
        evadeTimestamps[(cargo, pirate)] = ShipController.TimeStepCounter;

        cargoBehavior.currentGridPosition += new Vector2Int(1, 1);
        cargo.transform.position = cargoBehavior.GridToWorld(cargoBehavior.currentGridPosition);
    }
}
