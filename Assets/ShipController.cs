using UnityEngine;

public class ShipController : MonoBehaviour
{
    public GameObject Sphere;

    public void TriggerShipMovement()
    {
        // Access the ShipMovement script on the ship GameObject
        ShipMovement movementScript = Sphere.GetComponent<ShipMovement>();
        if (movementScript != null)
        {
            movementScript.MoveShipTowardsDestination(); // Trigger movement
            Debug.Log("Ship movement triggered");
        }
        else
        {
            Debug.LogWarning("ShipMovement script not found on the assigned ship GameObject.");
        }
    }
}