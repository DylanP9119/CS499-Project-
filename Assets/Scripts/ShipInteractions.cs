using UnityEngine;
using System.Collections.Generic;

public class ShipInteractions : MonoBehaviour
{
    public static ShipInteractions Instance;
    public bool isNight = false;

    private Dictionary<GameObject, GameObject> pirateToCapturedCargo = new();
    private Dictionary<GameObject, HashSet<GameObject>> cargoEvadedPirates = new();

    void Awake()
    {
        Instance = this;
    }

    public void CheckForInteractions(List<GameObject> allShips)
    {
        List<GameObject> piratesToRemove = new();
        List<GameObject> shipsToRemove = new();

        allShips.RemoveAll(ship => ship == null);

        foreach (GameObject ship in allShips)
        {
            Vector3 shipPos = ship.transform.position;

            foreach (GameObject otherShip in allShips)
            {
                if (ship == otherShip) continue;

                Vector3 otherPos = otherShip.transform.position;

                if (ship.CompareTag("Pirate") && otherShip.CompareTag("Patrol") && IsWithinRange(shipPos, otherPos, 3))
                {
                   //HandleDefeat(ship, otherShip); //bug occurs 
                }
                else if (ship.CompareTag("Pirate") && otherShip.CompareTag("Cargo") && !isNight && IsWithinRange(shipPos, otherPos, 3))
                {
                   //HandleCapture(ship, otherShip); //bug occurs
                }
                else if (ship.CompareTag("Pirate") && otherShip.CompareTag("Cargo") && isNight && IsWithinRange(shipPos, otherPos, 2))
                {
                   //HandleCapture(ship, otherShip);
                }
                else if (ship.CompareTag("Patrol"))
                {
                    CargoBehavior cargoBehavior = otherShip.GetComponent<CargoBehavior>();
                    if (cargoBehavior != null && cargoBehavior.isCaptured && IsWithinRange(shipPos, otherPos, 3))
                    {
                       //HandleRescue(ship, otherShip); // captured cargo rescued by patrol NO BUG
                    }
                }
                else if (ship.CompareTag("Cargo") && otherShip.CompareTag("Pirate") && IsWithinRange(shipPos, otherShip.transform.position, 4))
                {
                    //HandleEvasion(ship, otherShip); NO BUG 
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

            Vector3 southward = new Vector3(0, 0, -1);
            pirateShip.transform.position += southward;
            capturedCargo.transform.position = pirateShip.transform.position;

            CargoBehavior cargoBehavior = capturedCargo.GetComponent<CargoBehavior>();
            if (cargoBehavior != null)
            {
                cargoBehavior.currentGridPosition = new Vector2Int(
                    Mathf.RoundToInt(capturedCargo.transform.position.x),
                    Mathf.RoundToInt(capturedCargo.transform.position.z)
                );
            }

            //Debug.Log($"[Captured Move] {capturedCargo.name} & {pirateShip.name} moved to {pirateShip.transform.position}");

            if (pirateShip.transform.position.z <= 0)
            {
                Debug.Log($"[EXIT] {capturedCargo.name} and {pirateShip.name} exited at the coast and were removed.");
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
                Debug.Log($"[Exit] {ship.name} reached the right edge and was removed.");
                shipsToRemove.Add(ship);
            }
            else if (ship.CompareTag("Patrol") && Mathf.RoundToInt(pos.x) <= 0)
            {
                Debug.Log($"[Exit] {ship.name} reached the left edge and was removed.");
                shipsToRemove.Add(ship);
            }
            else if (ship.CompareTag("Pirate") && pos.z >= 99 && !pirateToCapturedCargo.ContainsKey(ship))
            {
                Debug.Log($"[Exit] {ship.name} reached the top edge and was removed.");
                shipsToRemove.Add(ship);
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
    
        Debug.Log($"[DEFEAT] {pirate.name} was defeated by {patrol.name}");
    
        if (pirateToCapturedCargo.TryGetValue(pirate, out GameObject cargo))
        {
            if (cargo != null)
            {
                CargoBehavior cargoBehavior = cargo.GetComponent<CargoBehavior>();
                if (cargoBehavior != null) cargoBehavior.isCaptured = false;
            }
     
            pirateToCapturedCargo.Remove(pirate);
        }
     
        Destroy(pirate);
    }

    // Pirate captures Cargo
    private void HandleCapture(GameObject pirate, GameObject cargo)
    {
        Debug.Log($"{pirate.name} captured {cargo.name}");

        if (pirateToCapturedCargo.ContainsValue(cargo))
        {
            Debug.LogWarning($"{cargo.name} is already captured by another pirate.");
            return;
        }

        if (pirateToCapturedCargo.ContainsKey(pirate))
        {
            Debug.LogWarning($"{pirate.name} is already escorting a cargo!");
            return;
        }

        //cargo.tag = "Captured";

        CargoBehavior cargoBehavior = cargo.GetComponent<CargoBehavior>();
        if (cargoBehavior != null)
        {
            cargoBehavior.isCaptured = true;
        }

        pirate.transform.position = cargo.transform.position;

        PirateBehavior pirateBehavior = pirate.GetComponent<PirateBehavior>();
        if (pirateBehavior != null)
        {
            pirateBehavior.hasCargo = true;
        }

        pirateToCapturedCargo[pirate] = cargo;
    }

    private void HandleRescue(GameObject capturedCargo, GameObject patrol)
    {
        //Debug.Log($"[{capturedCargo.name}] was rescued by a Patrol.");

        //GameObject capturingPirate = FindPirateNear(capturedCargo);

        //if (capturingPirate != null)
        //{
        //    pirateToCapturedCargo.Remove(capturingPirate);
        //    Debug.Log($"[{capturingPirate.name}] was removed after {capturedCargo.name} was rescued!");
        //    Destroy(capturingPirate);
        //}

        //CargoBehavior cargoBehavior = capturedCargo.GetComponent<CargoBehavior>();
        //if (cargoBehavior != null) // to prevent it from being counted as alive in another place
        //{
        //    cargoBehavior.isCaptured = false;
        // }

        //Destroy(capturedCargo); // Remove the rescued cargo too

        if (capturedCargo == null || patrol == null) return;

        Debug.Log($"[RESCUE] {capturedCargo.name} was rescued by {patrol.name}");

        GameObject capturingPirate = FindPirateNear(capturedCargo);
        if (capturingPirate != null)
        {
            pirateToCapturedCargo.Remove(capturingPirate);
            Destroy(capturingPirate);
        }

       CargoBehavior cargoBehavior = capturedCargo.GetComponent<CargoBehavior>();
        if (cargoBehavior != null)
        {
            cargoBehavior.isCaptured = false;
        }

        Destroy(capturedCargo); // or reset instead of destroying
    }

    private void HandleEvasion(GameObject cargo, GameObject pirate)
    {
        //if (cargo.CompareTag("Captured")) return;

        if (!cargoEvadedPirates.ContainsKey(cargo))
        {
            cargoEvadedPirates[cargo] = new HashSet<GameObject>();
        }

        if (cargoEvadedPirates[cargo].Contains(pirate)) return;

        cargoEvadedPirates[cargo].Add(pirate);

        CargoBehavior cargoBehavior = cargo.GetComponent<CargoBehavior>();
        if (cargoBehavior != null)
        {
            cargoBehavior.currentGridPosition += new Vector2Int(1, 1);
            cargo.transform.position = cargoBehavior.GridToWorld(cargoBehavior.currentGridPosition);
            Debug.Log($"{cargo.name} evaded {pirate.name}!");
        }
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
