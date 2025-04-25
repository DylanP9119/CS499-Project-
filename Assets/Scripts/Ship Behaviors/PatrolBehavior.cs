using UnityEngine;

public class PatrolBehavior : MonoBehaviour
{
    public Vector2Int currentGridPosition;
    public Vector2Int destinationGridPosition;
    public Vector2Int gridSize = new Vector2Int(400, 100);
    public float gridCellSize = 1f;
    public float movementDelay = 0.1f;
    private float movementTimer = 0f;

    void Start()
    {
    if (ReplayManager.Instance != null && ReplayManager.Instance.ReplayModeActive)
    {
        currentGridPosition = WorldToGrid(transform.position);
        destinationGridPosition = new Vector2Int(0, currentGridPosition.y);
    }
    else
    {
        currentGridPosition = WorldToGrid(transform.position);
        destinationGridPosition = new Vector2Int(0, currentGridPosition.y);
    }
    }

    public void Step(bool forceMove)
    {
        if (forceMove)
        {
            MoveShipTowardsDestination();
            return;
        }
        movementTimer += Time.deltaTime;
        if (movementTimer < movementDelay)
            return;
        movementTimer = 0f;
        MoveShipTowardsDestination();
    }

public void MoveShipTowardsDestination()
    {
        // Move one step closer to the destination
        if (currentGridPosition != destinationGridPosition)
        {
            Vector2Int direction = GetStepDirection();
            currentGridPosition += direction;

            // Update the ship's position in Unity world space
            transform.position = GridToWorld(currentGridPosition);

            // Log the current position and add it to the travel path
            //travelPath.Add(currentGridPosition);
            //Debug.Log($"Ship moved to: {currentGridPosition}");
        }
        else
        {
            // Stop movement and save the path when the destination is reached
            //isMoving = false;
            //SaveTravelPathToFile();
            //Debug.Log($"Destination reached at: {destinationGridPosition}");
        }
    }

    private Vector2Int GetStepDirection()
    {
        // Calculate the direction to move closer to the destination
        int stepX = -2;
        int stepY = 0; // Y remains constant, only moving left to right, vice versa for simplicity

        return new Vector2Int(stepX, stepY);
    }
public Vector2Int WorldToGrid(Vector3 worldPosition)
{
    int x = Mathf.FloorToInt(worldPosition.x / gridCellSize);
    int y = Mathf.FloorToInt(worldPosition.z / gridCellSize);
    return new Vector2Int(x, y);
}

    public Vector3 GridToWorld(Vector2Int gridPosition)
    {
        return new Vector3(gridPosition.x * gridCellSize, 0, gridPosition.y * gridCellSize);
    }
}
