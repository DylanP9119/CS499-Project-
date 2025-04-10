using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CargoBehavior : MonoBehaviour
{
    public int ShipId; // Assigned by ShipController on spawn.
    public Vector2Int currentGridPosition;
    public Vector2Int destinationGridPosition;
    public Vector2Int gridSize = new Vector2Int(400, 100);
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
    public float gridCellSize = 1f; // Size of each grid cell
    public float movementDelay = 0.1f; // Time delay between movements

=======
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
    public float gridCellSize = 1f;
    public float movementDelay = 0.1f;
    private TimeControl timeControl;
>>>>>>> Stashed changes
    private float movementTimer;
    private List<Vector2Int> travelPath = new List<Vector2Int>();
    private string filePath;
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
    private bool isMoving = false; // Flag to determine if the ship should move

    void Start()
    {
        // Spawn the ship at a random (X, Y) position
        int startX = 0; // Random column (X)
        int startY = Random.Range(0, gridSize.y); // Random row (Y)
=======
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
    public bool isCaptured = false;

    void Start()
    {
        timeControl = FindObjectOfType<TimeControl>();
        int startX = 0;
        int startY = Random.Range(0, gridSize.y);
<<<<<<< Updated upstream
<<<<<<< Updated upstream
>>>>>>> Stashed changes
        currentGridPosition = new Vector2Int(startX, startY);
        int destinationX = gridSize.x;
        destinationGridPosition = new Vector2Int(destinationX, startY);
<<<<<<< Updated upstream

        // Log the start and destination positions (for debugging) 
        Debug.Log($"Ship Start Position: {currentGridPosition}");
        Debug.Log($"Ship Destination Position: {destinationGridPosition}");

        // Convert the initial grid position to Unity world position
=======
>>>>>>> Stashed changes
        transform.position = GridToWorld(currentGridPosition);
        travelPath.Add(currentGridPosition);
        filePath = Path.Combine(Application.persistentDataPath, "ShipTravelPath.txt");
<<<<<<< Updated upstream
        Debug.Log($"Path will be saved to: {filePath}");
=======
>>>>>>> Stashed changes
=======
        currentGridPosition = new Vector2Int(startX, startY);
        int destinationX = gridSize.x;
        destinationGridPosition = new Vector2Int(destinationX, startY);
        transform.position = GridToWorld(currentGridPosition);
        travelPath.Add(currentGridPosition);
        filePath = Path.Combine(Application.persistentDataPath, "ShipTravelPath.txt");
>>>>>>> Stashed changes
=======
        currentGridPosition = new Vector2Int(startX, startY);
        int destinationX = gridSize.x;
        destinationGridPosition = new Vector2Int(destinationX, startY);
        transform.position = GridToWorld(currentGridPosition);
        travelPath.Add(currentGridPosition);
        filePath = Path.Combine(Application.persistentDataPath, "ShipTravelPath.txt");
>>>>>>> Stashed changes
    }

    void Update()
    {
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
        // If movement is active, move the ship step-by-step
        if (isMoving)
=======
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
        if (isCaptured || !timeControl.ShouldMove())
            return;
        movementTimer += Time.deltaTime;
        if (movementTimer >= movementDelay)
>>>>>>> Stashed changes
        {
            movementTimer += Time.deltaTime;
            if (movementTimer >= movementDelay)
            {
                movementTimer = 0f;
                MoveShipTowardsDestination();
            }
        }
    }

<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
    public void StartMovement()
    {
        isMoving = true;
        Debug.Log("Movement started!");
    }
=======
    public void Step() { }
>>>>>>> Stashed changes
=======
    public void Step() { }
>>>>>>> Stashed changes
=======
    public void Step() { }
>>>>>>> Stashed changes

    public void MoveShipTowardsDestination()
    {
        if (currentGridPosition != destinationGridPosition)
        {
            Vector2Int direction = GetStepDirection();
            currentGridPosition += direction;
            transform.position = GridToWorld(currentGridPosition);
            travelPath.Add(currentGridPosition);
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
            Debug.Log($"Ship moved to: {currentGridPosition}");
        }
        else
        {
            // Stop movement and save the path when the destination is reached
            isMoving = false;
            SaveTravelPathToFile();
            Debug.Log($"Destination reached at: {destinationGridPosition}");
=======
            if (ReplayManager.Instance != null)
            {
                ReplayManager.Instance.RecordMovementEvent(ShipId, "Cargo", transform.position, transform.rotation, timeControl.GlobalTime);
            }
        }
        else
        {
            SaveTravelPathToFile();
>>>>>>> Stashed changes
=======
            if (ReplayManager.Instance != null)
            {
                ReplayManager.Instance.RecordMovementEvent(ShipId, "Cargo", transform.position, transform.rotation, timeControl.GlobalTime);
            }
        }
        else
        {
            SaveTravelPathToFile();
>>>>>>> Stashed changes
=======
            if (ReplayManager.Instance != null)
            {
                ReplayManager.Instance.RecordMovementEvent(ShipId, "Cargo", transform.position, transform.rotation, timeControl.GlobalTime);
            }
        }
        else
        {
            SaveTravelPathToFile();
>>>>>>> Stashed changes
        }
    }

    private Vector2Int GetStepDirection()
    {
        int stepX = 1;
        int stepY = 0;
        return new Vector2Int(stepX, stepY);
    }

    private Vector3 GridToWorld(Vector2Int gridPosition)
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
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
        Debug.Log($"Travel path saved to: {filePath}");
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
    }
}
