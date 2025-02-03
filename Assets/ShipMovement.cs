using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ShipMovement : MonoBehaviour
{
    public Vector2Int currentGridPosition; 
    public Vector2Int destinationGridPosition; 
    public Vector2Int gridSize = new Vector2Int(400, 100); 
    public float gridCellSize = 1f; // Size of each grid cell
    public float movementDelay = 0.1f; // Time delay between movements

    private float movementTimer;
    private List<Vector2Int> travelPath = new List<Vector2Int>(); // To store the path for replay
    private string filePath;
    private bool isMoving = false; // Flag to determine if the ship should move

    void Start()
    {
        // Spawn the ship at a random (X, Y) position
        int startX = Random.Range(0, gridSize.x); // Random column (X)
        int startY = Random.Range(0, gridSize.y); // Random row (Y)
        currentGridPosition = new Vector2Int(startX, startY);

        // Set the destination on the same Y, but random X
        int destinationX = Random.Range(0, gridSize.x);
        destinationGridPosition = new Vector2Int(destinationX, startY);

        // Log the start and destination positions (for debugging) 
        Debug.Log($"Ship Start Position: {currentGridPosition}");
        Debug.Log($"Ship Destination Position: {destinationGridPosition}");

        // Convert the initial grid position to Unity world position
        transform.position = GridToWorld(currentGridPosition);

        // Add the starting position to the travel path
        travelPath.Add(currentGridPosition);

        // Set file path to save the travel path
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
        // Move one step closer to the destination
        if (currentGridPosition != destinationGridPosition)
        {
            Vector2Int direction = GetStepDirection();
            currentGridPosition += direction;

            // Update the ship's position in Unity world space
            transform.position = GridToWorld(currentGridPosition);

            // Log the current position and add it to the travel path
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
        // Calculate the direction to move closer to the destination
        int stepX = destinationGridPosition.x > currentGridPosition.x ? 1 : (destinationGridPosition.x < currentGridPosition.x ? -1 : 0);
        int stepY = 0; // Y remains constant, only moving left to right, vice versa for simplicity

        return new Vector2Int(stepX, stepY);
    }

    private Vector3 GridToWorld(Vector2Int gridPosition)
    {
        // Convert grid coordinates to Unity world position
        return new Vector3(gridPosition.x * gridCellSize, 0, gridPosition.y * gridCellSize);
    }

    public void SaveTravelPathToFile()
    {
        // Convert the path to a readable format
        List<string> pathStrings = new List<string>();
        foreach (Vector2Int pos in travelPath)
        {
            pathStrings.Add($"{pos.x},{pos.y}");
        }

        // Write to a file
        File.WriteAllLines(filePath, pathStrings);
        Debug.Log($"Travel path saved to: {filePath}");
    }
}
