
using UnityEngine;
using System.Collections.Generic;

public class ShipInteractions : MonoBehaviour
{
    public static ShipInteractions Instance;
    public bool isNight = false;

    void Awake()
    {
        Instance = this; // Singleton for easy access
    }

    // Checks interactions after all ships move
    public void CheckForInteractions(List<GameObject> allShips)
    {
        foreach (GameObject ship in allShips)
        {
            ShipMovement shipMovement = ship.GetComponent<ShipMovement>();
            if (shipMovement == null) continue;

            Vector2Int shipGridPos = shipMovement.currentGridPosition;

            foreach (GameObject otherShip in allShips)
            {
                if (ship == otherShip) continue;

                ShipMovement otherShipMovement = otherShip.GetComponent<ShipMovement>();
                if (otherShipMovement == null) continue;

                Vector2Int otherGridPos = otherShipMovement.currentGridPosition;

                // Handle each interaction based on ship types
                if (ship.CompareTag("Pirate") && otherShip.CompareTag("Patrol") && IsWithinRange(shipGridPos, otherGridPos, 3))
                {
                    HandleDefeat(ship);
                }
                else if (ship.CompareTag("Pirate") && otherShip.CompareTag("Cargo") && isNight == false && IsWithinRange(shipGridPos, otherGridPos, 3))
                {
                    HandleCapture(ship, otherShip);
                }
                else if (ship.CompareTag("Pirate") && otherShip.CompareTag("Cargo") && isNight == true && IsWithinRange(shipGridPos, otherGridPos, 2))
                {
                    HandleCapture(ship, otherShip);
                }
                else if (ship.CompareTag("Patrol") && otherShip.CompareTag("Captured") && IsWithinRange(shipGridPos, otherGridPos, 3))
                {
                    HandleRescue(otherShip);
                }
                else if (ship.CompareTag("Cargo") && otherShip.CompareTag("Pirate") && IsWithinRange(shipGridPos, otherGridPos, 4))
                {
                    HandleEvasion(ship);
                }
            }
        }
    }

    // Check if two grid positions are within a given range
    private bool IsWithinRange(Vector2Int pos1, Vector2Int pos2, int range)
    {
        return Mathf.Abs(pos1.x - pos2.x) <= range && Mathf.Abs(pos1.y - pos2.y) <= range;
    }

    // Pirate is defeated by Patrol
    private void HandleDefeat(GameObject pirate)
    {
        Debug.Log($"{pirate.name} was defeated by a Patrol and removed.");
        Destroy(pirate);
    }

    // Pirate captures Cargo
    private void HandleCapture(GameObject pirate, GameObject cargo)
    {
        Debug.Log($"{pirate.name} captured {cargo.name}");

        // Mark Cargo as Captured
        cargo.tag = "Captured";  // Change Cargo's tag to 'Captured'

        // Move the Pirate to Cargo's Position
        ShipMovement pirateMovement = pirate.GetComponent<ShipMovement>();
        ShipMovement cargoMovement = cargo.GetComponent<ShipMovement>();

        if (pirateMovement != null && cargoMovement != null)
        {
            pirateMovement.currentGridPosition = cargoMovement.currentGridPosition;
            pirate.transform.position = cargo.transform.position; // Move Pirate in Unity world

            // Link Cargo and Pirate (So They Move Together Next Turn)
            pirateMovement.SetCapturedCargo(cargo);
        }

    }
    // Patrol rescues Captured Cargo
    private void HandleRescue(GameObject capturedCargo)
    {
        Debug.Log($"[{capturedCargo.name}] was rescued by a Patrol.");

        // Revert Captured Cargo to Normal Cargo
        capturedCargo.tag = "Cargo"; // Change the tag back

        // Find the Pirate that captured this Cargo
        GameObject pirate = FindPirateNear(capturedCargo);

        if (pirate != null)
        {
            ShipMovement pirateMovement = pirate.GetComponent<ShipMovement>();

            if (pirateMovement != null)
            {
                // Step 3: Unlink the Cargo from the Pirate
                pirateMovement.SetCapturedCargo(null);
            }

            // Remove the Pirate from the simulation
            Debug.Log($"[{pirate.name}] was removed after {capturedCargo.name} was rescued!");
            Destroy(pirate);
        }
    }

    // Cargo evades Pirate
    private void HandleEvasion(GameObject cargo)
    {
        ShipMovement movement = cargo.GetComponent<ShipMovement>();
        if (movement != null)
        {
            movement.currentGridPosition += new Vector2Int(1, 1); // Move diagonally north-east
            cargo.transform.position = movement.GridToWorld(movement.currentGridPosition);
            Debug.Log($"{cargo.name} evaded a Pirate!");
        }
    }

    // Find the Pirate near a captured Cargo
    private GameObject FindPirateNear(GameObject capturedCargo)
    {
        ShipMovement cargoMovement = capturedCargo.GetComponent<ShipMovement>();
        Vector2Int cargoGridPos = cargoMovement.currentGridPosition;

        GameObject[] allShips = GameObject.FindGameObjectsWithTag("Pirate");
        foreach (GameObject pirate in allShips)
        {
            ShipMovement pirateMovement = pirate.GetComponent<ShipMovement>();
            if (pirateMovement != null && pirateMovement.currentGridPosition == cargoGridPos)
            {
                return pirate;
            }
        }
        return null;
    }
}
