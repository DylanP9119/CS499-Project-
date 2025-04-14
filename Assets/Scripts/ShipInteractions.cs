using UnityEngine;
using System.Collections.Generic;

public class ShipInteractions : MonoBehaviour
{
    public static ShipInteractions Instance;
    public bool isNight = false;

    public TextController textController;

    private Dictionary<GameObject, GameObject> pirateToCapturedCargo = new();
    private Dictionary<GameObject, HashSet<GameObject>> cargoEvadedPirates = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            //Debug.LogWarning("[ShipInteractions] Duplicate detected, destroying extra instance.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        //Debug.Log($"[ShipInteractions] Awake called on frame {Time.frameCount}");
    }

    public void CheckForInteractions(List<GameObject> allShips)
    {

        List<GameObject> piratesToRemove = new();
        List<GameObject> shipsToRemove = new();

        allShips.RemoveAll(ship => ship == null);

        foreach (GameObject ship in allShips)
        {
            if (ship == null || !ship.activeInHierarchy) continue;
            Vector3 shipPos = ship.transform.position;

            foreach (GameObject otherShip in allShips)
            {
                if (otherShip == null || !otherShip.activeInHierarchy) continue;
                if (ship == otherShip) continue;

                Vector3 otherPos = otherShip.transform.position;

                if (ship.CompareTag("Pirate") && otherShip.CompareTag("Patrol") && IsWithinRange(shipPos, otherPos, 3))
                {
                   HandleDefeat(ship, otherShip); //bug occurs 
                }
                else if (ship.CompareTag("Pirate") && otherShip.CompareTag("Cargo") && !isNight && IsWithinRange(shipPos, otherPos, 3))
                {
                   HandleCapture(ship, otherShip); //bug occurs
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
                        //Debug.Log($"[TRIGGER] {ship.name} triggered rescue of {otherShip.name}");
                        //Debug.LogWarning($"[DEBUG] Checking interaction: {ship.name} ({ship.tag}) -> {otherShip.name} ({otherShip.tag})");

                        HandleRescue(ship); // captured cargo rescued by patrol NO BUG
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

        //foreach (GameObject ship in allShips)
        //{
            //if (ship != null && ship.CompareTag("Evading"))
            //{
             //   ship.tag = "Cargo";
           // }
      //  }

        // Move captured pirates and cargos south and remove if at the coast
        foreach (KeyValuePair<GameObject, GameObject> pirateCargoPair in pirateToCapturedCargo)
        {
            GameObject pirateShip = pirateCargoPair.Key;
            GameObject capturedCargo = pirateCargoPair.Value;

            if (pirateShip == null || capturedCargo == null)
            {
                //Debug.LogWarning($"[CLEANUP] Removing null pair {pirateShip?.name}, {capturedCargo?.name}");
                piratesToRemove.Add(pirateShip);
                continue;
            }

            PirateBehavior pirateBehavior = pirateShip.GetComponent<PirateBehavior>();
            CargoBehavior cargoBehavior = capturedCargo.GetComponent<CargoBehavior>();

            if (pirateBehavior != null && cargoBehavior != null)
            {
                // Move both by grid
                pirateBehavior.currentGridPosition += Vector2Int.down;
                pirateShip.transform.position = pirateBehavior.GridToWorld(pirateBehavior.currentGridPosition);

                cargoBehavior.currentGridPosition = pirateBehavior.currentGridPosition;
                capturedCargo.transform.position = pirateShip.transform.position;
            }

            //Debug.Log($"[Captured Move] {capturedCargo.name} & {pirateShip.name} moved to {pirateShip.transform.position}");

            if (pirateShip.transform.position.z <= 0)
            {
                //Debug.Log($"[EXIT] {capturedCargo.name} and {pirateShip.name} exited at the coast and were removed.");
                Destroy(pirateShip);
                Destroy(capturedCargo);
                piratesToRemove.Add(pirateShip);
            }
        }

        foreach (GameObject pirateToRemove in piratesToRemove)
        {
            pirateToCapturedCargo.Remove(pirateToRemove);
        }

        // Clean up any ships that reach the map boundaries and are no longer active
        RemoveShipsAtEdge(allShips, shipsToRemove);

        foreach (GameObject ship in shipsToRemove)
        {
            allShips.Remove(ship);
            Destroy(ship);
        }
    }

    private void RemoveShipsAtEdge(List<GameObject> allShips, List<GameObject> shipsToRemove)
    {
        foreach (GameObject ship in allShips)
        {
            if (ship == null) continue;

            Vector3 pos = ship.transform.position;

            if (ship.CompareTag("Cargo") && pos.x >= 399)
            {
                //Debug.Log($"[Exit] {ship.name} reached the right edge and was removed.");
                shipsToRemove.Add(ship);
                textController.UpdateShipExit("cargo");
            }
            else if (ship.CompareTag("Patrol") && pos.x <= -1f)
            {
                Debug.Log($"[Exit] {ship.name} reached the left edge and was removed.");
                shipsToRemove.Add(ship);
                textController.UpdateShipExit("patrol");
            }
            else if (ship.CompareTag("Pirate") && pos.z >= 99 && !pirateToCapturedCargo.ContainsKey(ship))
            {
                //Debug.Log($"[Exit] {ship.name} reached the top edge and was removed.");
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

    // Pirate is defeated by Patrol
    private void HandleDefeat(GameObject pirate, GameObject patrol)
    {
        if (pirate == null || patrol == null) return;
    
        //Debug.Log($"[DEFEAT] {pirate.name} was defeated by {patrol.name}");
    
        if (pirateToCapturedCargo.TryGetValue(pirate, out GameObject cargo))
        {
            if (cargo != null)
            {
                CargoBehavior cargoBehavior = cargo.GetComponent<CargoBehavior>();
                if (cargoBehavior != null) cargoBehavior.isCaptured = false;
            }
     
            pirateToCapturedCargo.Remove(pirate);
        }
        textController.PirateDestroyed();
        Destroy(pirate);
        //Debug.LogWarning($"[CLEANUP] {pirate.name} destroyed. Removing from pirateToCapturedCargo.");

    }

    // Pirate captures Cargo
    private void HandleCapture(GameObject pirate, GameObject cargo)
    {
        // Make sure the cargo isn't already captured
        if (pirateToCapturedCargo.ContainsValue(cargo))
        {
            //Debug.LogWarning($"[CAPTURE BLOCKED] {cargo.name} is already captured. Skipping.");
            return;
        }

        // Make sure the pirate isn't already escorting someone
        if (pirateToCapturedCargo.ContainsKey(pirate))
        {
            //Debug.LogWarning($"[CAPTURE BLOCKED] {pirate.name} is already escorting a cargo. Skipping.");
            return;
        }

        // Double-check the cargo's internal state too (optional safety net)
        CargoBehavior cargoBehavior = cargo.GetComponent<CargoBehavior>();
        if (cargoBehavior != null && cargoBehavior.isCaptured)
        {
            //Debug.LogWarning($"[CAPTURE BLOCKED] {cargo.name} already has isCaptured = true. Skipping.");
            return;
        }

        // All checks passed � capture is valid. Now we mark everything.
        if (cargoBehavior != null)
        {
            cargoBehavior.isCaptured = true;
        }

        pirateToCapturedCargo[pirate] = cargo;

        PirateBehavior pirateBehavior = pirate.GetComponent<PirateBehavior>();
        if (pirateBehavior != null)
        {
            pirateBehavior.hasCargo = true;
        }

        pirate.transform.position = cargo.transform.position;

        //Debug.LogWarning($"[CAPTURE] {pirate.name} captured {cargo.name} (isCaptured: {cargoBehavior?.isCaptured})");

        textController.UpdateCaptures(true);
    }

    private void HandleRescue(GameObject patrol)
    {
        if (patrol == null) return;

        // Look through all captured cargo
        List<GameObject> toRescue = new();

        foreach (var pair in pirateToCapturedCargo)
        {
            GameObject pirate = pair.Key;
            GameObject cargoCandidate = pair.Value; // renamed here

            if (cargoCandidate == null || pirate == null) continue;

            Vector3 patrolPos = patrol.transform.position;
            Vector3 cargoPos = cargoCandidate.transform.position;

            if (IsWithinRange(patrolPos, cargoPos, 3)) // 3x3 rescue rule
            {
                toRescue.Add(cargoCandidate); // this is where it's added
            }
        }

        // Actually rescue the captured cargos
        foreach (GameObject cargoToRescue in toRescue)
        {
            GameObject capturingPirate = null;

            foreach (var pair in pirateToCapturedCargo)
            {
                if (pair.Value == cargoToRescue) // this is where the cargo match is checked
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
                //Debug.LogWarning($"[CLEANUP] {capturingPirate.name} destroyed. Removing from pirateToCapturedCargo.");

            }

            var cargoBehavior = cargoToRescue.GetComponent<CargoBehavior>();
            if (cargoBehavior != null)
            {
                cargoBehavior.isCaptured = false;
                //Debug.LogWarning($"[POST-RESCUE STATE] {cargoToRescue.name} � isCaptured: {cargoBehavior.isCaptured}");
                //capturedCargo.tag = "Cargo";
                cargoToRescue.tag = "Cargo";
            }

            //Debug.LogWarning($"[RESCUE TRIGGERED] {cargoToRescue.name} rescued by {patrol.name}. isCaptured was true and is now reset.");
        }

        textController.UpdateCaptures(false);
    }

    private void HandleEvasion(GameObject cargo, GameObject pirate)
    {
        //Debug.Log($"[Evasion Dictionary Count] {cargoEvadedPirates.Count} cargos tracked so far");

        CargoBehavior cargoBehavior = cargo.GetComponent<CargoBehavior>();
        if (cargoBehavior == null) return;

        //Debug.Log($"[CHECK] HandleEvasion called for Cargo: {cargo.name} (ID: {cargo.GetInstanceID()}) | Pirate: {pirate.name} (ID: {pirate.GetInstanceID()})");
        //Debug.Log($"[CHECK] isEvadingThisStep: {cargoBehavior.isEvadingThisStep}");


        // Prevent duplicate evasion triggers within the same step
        if (cargoBehavior.isEvadingThisStep) return;

        // Set up tracking if needed
        if (!cargoEvadedPirates.ContainsKey(cargo))
        {
            //Debug.Log($"[DEBUG] No evasion history for {cargo.name}, creating new entry.");
            cargoEvadedPirates[cargo] = new HashSet<GameObject>();
        }

        // Already evaded this pirate
        if (cargoEvadedPirates[cargo].Contains(pirate))
        {
            //Debug.LogWarning($"[BLOCK] {cargo.name} already evaded {pirate.name}. Skipping.");
            return;
        }

        // Prevent duplicate evasion triggers within the same step
        if (cargoBehavior.isEvadingThisStep)
        {
            //Debug.LogWarning($"[BLOCK] {cargo.name} is already evading this step. Skipping.");
            return;
        }

        // Debug
        //Debug.LogWarning($"[TRIGGER] {cargo.name} (ID: {cargo.GetInstanceID()}) evading {pirate.name} (ID: {pirate.GetInstanceID()})");

        // Record the evasion
        cargoEvadedPirates[cargo].Add(pirate);

        // Perform the evasion
        cargoBehavior.currentGridPosition += new Vector2Int(1, 1);
        cargo.transform.position = cargoBehavior.GridToWorld(cargoBehavior.currentGridPosition);
        cargoBehavior.isEvadingThisStep = true;

        //Debug.Log($"{cargo.name} evaded {pirate.name}!");
    }

    private GameObject FindPirateNear(GameObject capturedCargo)
    {
        Vector3 cargoGridPos = capturedCargo.transform.position;
        GameObject[] allShips = GameObject.FindGameObjectsWithTag("Pirate");

        foreach (GameObject pirate in allShips)
        {
            Vector3 piratePos = pirate.transform.position;

            if (Mathf.Approximately(piratePos.x, cargoGridPos.x) && Mathf.Approximately(piratePos.z, cargoGridPos.z))
            {
                return pirate;
            }
        }
        return null;
    }
}
