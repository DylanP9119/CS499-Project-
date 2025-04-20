using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;

public class ShipInteractions : MonoBehaviour
{
    public static ShipInteractions Instance;
    public bool isNight = false;
    public TextController textController;

    // Maps a pirate to its captured cargo
    private Dictionary<GameObject, GameObject> pirateToCapturedCargo = new();
    // Tracks which pirates a cargo has already evaded
    private Dictionary<GameObject, HashSet<GameObject>> cargoEvadedPirates = new();
    //private Dictionary<(GameObject cargo, GameObject pirate), int> evadeTimestamps = new();
    // Stores whether evasion outcome (success/failure) has been logged already
    private Dictionary<(GameObject, GameObject), bool> evasionOutcomeLogged = new();
    // Tracks evasion attempts that are still pending evaluation
    private Dictionary<(GameObject cargo, GameObject pirate), int> pendingEvasions = new();
    // Timestamps for when evasion attempts occurred

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
        List<GameObject> piratesToRemove = new(); // For pirates that need to be removed (like when reaching south edge)
        List<GameObject> shipsToRemove = new(); // For ships that leave the map

        // Remove any null references first.
        allShips.RemoveAll(ship => ship == null);

        // Outer loop: iterate through each ship
        foreach (GameObject ship in allShips.ToList())
        {
            if (ship == null || !ship.activeInHierarchy) // null protection / not in hierarchy
                continue;
            Vector3 shipPos = ship.transform.position;

            // Comapre with outerloop
            foreach (GameObject otherShip in allShips.ToList())
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

        int currentTick = ShipController.TimeStepCounter;
        // Move captured pairs when the simulation tick has advanced.
        if (currentTick != lastDownMovementTick)
        {
            foreach (KeyValuePair<GameObject, GameObject> pair in pirateToCapturedCargo)
            {
                //Loop through each pirate-cargo captured pair
                GameObject pirateShip = pair.Key; //Key = pirate
                GameObject capturedCargo = pair.Value; //Value = captured

                if (pirateShip == null || capturedCargo == null) //Skip and remove if either ship missing
                {
                    piratesToRemove.Add(pirateShip);
                    continue;
                }

                // Grab movement scripts
                PirateBehavior pirateBehavior = pirateShip.GetComponent<PirateBehavior>();
                CargoBehavior cargoBehavior = capturedCargo.GetComponent<CargoBehavior>();

                if (pirateBehavior != null && cargoBehavior != null)
                {
                    int direction = 1;
                    // Move captured pair one grid cell vertically.
                    pirateBehavior.currentGridPosition += Vector2Int.down * direction;
                    // Update grid position for pirate
                    pirateShip.transform.position = pirateBehavior.GridToWorld(pirateBehavior.currentGridPosition);

                    // Brings cargo to pirate cell
                    cargoBehavior.currentGridPosition = pirateBehavior.currentGridPosition;
                    // Update cargo to match pirate cell
                    capturedCargo.transform.position = pirateShip.transform.position;
                }
                // If south, destroy both
                if (pirateShip.transform.position.z <= 0)
                {
                    Destroy(pirateShip);
                    Destroy(capturedCargo);
                    piratesToRemove.Add(pirateShip);
                }
            }
            lastDownMovementTick = currentTick;
        }

        // Remove the pirate-cargo mappings for destroyed pirates
        foreach (GameObject pirateToRemove in piratesToRemove)
        {
            pirateToCapturedCargo.Remove(pirateToRemove);
        }

        // Remove ships at boundaries.
        RemoveShipsAtEdge(allShips, shipsToRemove);
        foreach (GameObject ship in shipsToRemove)
        {
            allShips.Remove(ship);
            Destroy(ship);
        }

    // Finalize evasion outcome if not captured on next tick
        List<(GameObject, GameObject)> evasionSuccesses = new();

        foreach (var entry in evadeTimestamps)
        {
            var pair = entry.Key;
            int evadeTick = entry.Value;

            if (ShipController.TimeStepCounter == evadeTick + 1)
            {
            // If cargo not captured and not already logged
                if (!pirateToCapturedCargo.ContainsKey(pair.Item2) &&
                (!evasionOutcomeLogged.ContainsKey(pair) || evasionOutcomeLogged[pair] == false))
                    {
                        textController.UpdateEvasion(true, true); // Mark successful evasion
                        evasionOutcomeLogged[pair] = true;
                        //Debug.Log($"[EVADE SUCCESS] {pair.Item1?.name} evaded {pair.Item2?.name} on tick {evadeTick}");
                    }
                evasionSuccesses.Add(pair); // Schedule cleanup
            }
        }

        // Remove finalized entries
        foreach (var pair in evasionSuccesses)
        {
            evadeTimestamps.Remove(pair);
            pendingEvasions.Remove(pair);
        }    
    }

    private void RemoveShipsAtEdge(List<GameObject> allShips, List<GameObject> shipsToRemove)
    {
        foreach (GameObject ship in allShips)
        {
            if (ship == null)
                continue;
            Vector3 pos = ship.transform.position;
            if (ship.CompareTag("Cargo") && pos.x > 399) // Cargo exits east
            {
                shipsToRemove.Add(ship);
                textController.UpdateShipExit("cargo");
            }
            else if (ship.CompareTag("Patrol") && Mathf.RoundToInt(pos.x) <= 0) // Patrol exit west
            {
                //Debug.Log($"[Exit] {ship.name} reached the left edge and was removed.");
                shipsToRemove.Add(ship);
                textController.UpdateShipExit("patrol");
            }
            else if (ship.CompareTag("Pirate") && pos.z > 99 && !pirateToCapturedCargo.ContainsKey(ship)) //Pirates exit north
            {
                shipsToRemove.Add(ship);
                textController.UpdateShipExit("pirate");
            }
        }
    }

    private bool IsWithinRange(Vector3 pos1, Vector3 pos2, float range) // range of each other
    {
        float dx = Mathf.Abs(pos1.x - pos2.x); // Horizontal distance between two pos
        float dz = Mathf.Abs(pos1.z - pos2.z); //Vertical between two pos
        return dx <= range && dz <= range; // Return true if x and z distances are within range
    }

    private void HandleDefeat(GameObject pirate, GameObject patrol)
    {
        if (pirate == null || patrol == null)
            return;
        if (pirateToCapturedCargo.TryGetValue(pirate, out GameObject cargo)) // Was for testing, should not ever run
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

        //pirate.SetActive(false); // disable object
        ShipController shipCtrl = FindObjectOfType<ShipController>();
        if (shipCtrl != null)
        {
            shipCtrl.allShips.Remove(pirate); //remove from global list
        }
        Destroy(pirate); // defeat pirate
        //Debug.Log($"[PIRATE DESTROYED]");
    }

    private void HandleCapture(GameObject pirate, GameObject cargo)
    {
        if (pirateToCapturedCargo.ContainsValue(cargo))  // avoid dups
            return;
        if (pirateToCapturedCargo.ContainsKey(pirate))
            return;

        CargoBehavior cargoBehavior = cargo.GetComponent<CargoBehavior>();
        if (cargoBehavior != null && cargoBehavior.isCaptured)
            return;

        if (pendingEvasions.ContainsKey((cargo, pirate))) // handle failed evasion log
        {
            //Debug.Log($"[CAPTURE] {cargo.name} was previously evaded from {pirate.name}");
            if (!evasionOutcomeLogged.ContainsKey((cargo, pirate)) || evasionOutcomeLogged[(cargo, pirate)] == true)
            {
                //Debug.Log($"[FAILURE LOGGED] {cargo.name} failed to evade {pirate.name}");
                textController.UpdateEvasion(false, false);
                evasionOutcomeLogged[(cargo, pirate)] = false; // mark that we’ve handled this pair
            }
            else
            {
                //Debug.Log($"[SKIPPED LOGGING] {cargo.name} already marked failed for {pirate.name}");
            }
            pendingEvasions.Remove((cargo, pirate));
            if (evadeTimestamps.ContainsKey((cargo, pirate)))
            {
                textController.UpdateEvasion(false, false);
                evadeTimestamps.Remove((cargo, pirate));
            }

            if (cargoBehavior != null)  // marks cargo as captured
                cargoBehavior.isCaptured = true;

            pirateToCapturedCargo[pirate] = cargo; //record as a pair

            PirateBehavior pirateBehavior = pirate.GetComponent<PirateBehavior>(); //mark pirate as has cargo
            if (pirateBehavior != null)
                pirateBehavior.hasCargo = true;

            // Align pirate with cargo, same cell
            if (pirateBehavior != null && cargoBehavior != null)
            {
                pirateBehavior.currentGridPosition = cargoBehavior.currentGridPosition;
                pirate.transform.position = cargo.transform.position;

                cargo.transform.rotation = Quaternion.Euler(0, 180, 0);
                pirate.transform.rotation = Quaternion.Euler(0, 180, 0);
            }
            textController.UpdateCaptures(true);
        }
    }

    private void HandleRescue(GameObject patrol)
    {
        if (patrol == null)
            return;
        List<GameObject> toRescue = new(); // list for holding ships ready for rescue
        foreach (var pair in pirateToCapturedCargo) // loops through captured pairs
        {
            GameObject pirate = pair.Key;
            GameObject cargoCandidate = pair.Value;
            if (cargoCandidate == null || pirate == null)
                continue;
            Vector3 patrolPos = patrol.transform.position;
            Vector3 cargoPos = cargoCandidate.transform.position;
            if (IsWithinRange(patrolPos, cargoPos, 3)) // If patrol is within 3x3 range of captured, mark for rescue
                toRescue.Add(cargoCandidate);
        }

        foreach (GameObject cargoToRescue in toRescue) // Go through each cargo ship marked for rescue
        {
            GameObject capturingPirate = null;
            foreach (var pair in pirateToCapturedCargo) // Find pirate that captured the cargo
            {
                if (pair.Value == cargoToRescue)
                {
                    capturingPirate = pair.Key;
                    break;
                }
            }
            if (capturingPirate != null) // if found, remove pirate, free cargo
            {
                pirateToCapturedCargo.Remove(capturingPirate);
                capturingPirate.SetActive(false);
                Destroy(capturingPirate);
            }
            CargoBehavior cargoBehavior = cargoToRescue.GetComponent<CargoBehavior>();
            if (cargoBehavior != null) // cargo goes back to normal
            {
                cargoBehavior.isCaptured = false;
                cargoToRescue.tag = "Cargo";
                cargoToRescue.transform.rotation = Quaternion.Euler(0, 90, 0);
            }

            // Record the rescue event so that replay includes it.
            if (ReplayManager.Instance != null)
            {
                int shipId = ExtractShipId(cargoToRescue);
                float simTime = ShipController.TimeStepCounter * 1f;
            }
        }
        textController.UpdateCaptures(false);
    }

    private void HandleEvasion(GameObject cargo, GameObject pirate)
    {
        if (pirateToCapturedCargo.ContainsKey(pirate)) // No evasions if pirate already has a cargo
            return;
        CargoBehavior cargoBehavior = cargo.GetComponent<CargoBehavior>();
        if (cargoBehavior == null)
            return;
        if (cargoBehavior.isEvadingThisStep)
            return;
        if (!cargoEvadedPirates.ContainsKey(cargo)) // Make evasion tracking set for cargo if first time evading pirate
            cargoEvadedPirates[cargo] = new HashSet<GameObject>();
        if (cargoEvadedPirates[cargo].Contains(pirate)) // Skip if cargo already evaded this pirate
            return;

        cargoEvadedPirates[cargo].Add(pirate); // Log that the cargo has evaded the pirate. 
        pendingEvasions[(cargo, pirate)] = ShipController.TimeStepCounter;
        //Debug.Log($"[EVADE] {cargo.name} evaded {pirate.name} at tick {ShipController.TimeStepCounter}");
        evadeTimestamps[(cargo, pirate)] = ShipController.TimeStepCounter;

        cargoBehavior.currentGridPosition += new Vector2Int(1, 1); //actually evade northeast. 
        cargo.transform.position = cargoBehavior.GridToWorld(cargoBehavior.currentGridPosition);
    }

    private void FinalizeEvadeOutcomes() // Any evades pending, mark as success
    {
        List<(GameObject, GameObject)> toFinalize = new(pendingEvasions.Keys); // List for pending evasions
        foreach (var pair in toFinalize)
        {
            if (!evasionOutcomeLogged.ContainsKey(pair)) // If not yet logged 
            {
                textController.UpdateEvasion(true, true);  // mark it successful evasion and log
                evasionOutcomeLogged[pair] = true;
                //Debug.Log($"[FINALIZE] Marked evade as SUCCESS: {pair.Item1?.name} vs {pair.Item2?.name}");
            }
        }
        pendingEvasions.Clear(); // clear the list
    }

    public void FinalizePendingEvasions()
    {
        FinalizeEvadeOutcomes();
    }
    private int ExtractShipId(GameObject ship)
    {
        if (ship == null || string.IsNullOrEmpty(ship.name))
            return 0;
        int start = ship.name.IndexOf('(');
        int end = ship.name.IndexOf(')');
        if (start >= 0 && end > start)
        {
            string numStr = ship.name.Substring(start + 1, end - start - 1);
            if (int.TryParse(numStr, out int id))
                return id;
        }
        return 0;
    }
}
