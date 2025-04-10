using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PirateBehavior : MonoBehaviour
{
    public int ShipId; // Assigned by ShipController when spawned.
    public Vector2Int currentGridPosition;
    public Vector2Int destinationGridPosition;
    public Vector2Int gridSize = new Vector2Int(400, 100);
    public float gridCellSize = 1f; // Size of each grid cell
    public float movementDelay = 0.1f; // Time delay between movements

    private float movementTimer;
    private List<Vector2Int> travelPath = new List<Vector2Int>(); // For replay (if needed)
    private string filePath;
    private bool isMoving = false; // Flag to determine if the ship should move
    private bool hasCargo = false; // Determines whether a cargo ship has been captured for reverse direction

    void Start()
    {
        // Spawn the ship at a random (X, Y) position
        int startX = Random.Range(0, gridSize.x); // Random column (X)
        int startY = 0; // Bottom of grid
        currentGridPosition = new Vector2Int(startX, startY);
        int destinationY = gridSize.y;
        destinationGridPosition = new Vector2Int(startX, destinationY);

        // Log the start and destination positions (for debugging) 
        Debug.Log($"Ship Start Position: {currentGridPosition}");
        Debug.Log($"Ship Destination Position: {destinationGridPosition}");

        // Convert the initial grid position to Unity world position
        transform.position = GridToWorld(currentGridPosition);
        travelPath.Add(currentGridPosition);
        filePath = Path.Combine(Application.persistentDataPath, "ShipTravelPath.txt");
        Debug.Log($"Path will be saved to: {filePath}");
    }

    void Update()
    {
        // If movement is active, move the ship step-by-step
        if (isMoving)
        {
            movementTimer += Time.deltaTime;
            if (movementTimer >= movementDelay)
            {
                movementTimer = 0f;
                MoveShipTowardsDestination();
            }
        }
    }

    public void StartMovement()
    {
        isMoving = true;
        Debug.Log("Movement started!");
    }

    public void MoveShipTowardsDestination()
    {
        if (currentGridPosition != destinationGridPosition)
        {
            Vector2Int direction = GetStepDirection();
            currentGridPosition += direction;
            transform.position = GridToWorld(currentGridPosition);
            travelPath.Add(currentGridPosition);
            Debug.Log($"Ship moved to: {currentGridPosition}");
        }
        else
        {
            // Stop movement and save the path when the destination is reached
            isMoving = false;
            SaveTravelPathToFile();
            Debug.Log($"Destination reached at: {destinationGridPosition}");
        }
    }

    private Vector2Int GetStepDirection()
    {
        // Move upward in Y; if hasCargo reverse the Y direction.
        int stepX = 0;
        int stepY = hasCargo ? -1 : 1;
        return new Vector2Int(stepX, stepY);
    }

    public Vector3 GridToWorld(Vector2Int gridPosition)
    {
        return new Vector3(gridPosition.x * gridCellSize, 0, gridPosition.y * gridCellSize);
    }

    public void SaveTravelPathToFile()
    {
        List<string> pathStrings = new List<string>();
        foreach (Vector2Int pos in travelPath)
        {
            pathStrings.Add($"{pos.x},{pos.y}");
        }
        File.WriteAllLines(filePath, pathStrings);
        Debug.Log($"Travel path saved to: {filePath}");
    }
}
