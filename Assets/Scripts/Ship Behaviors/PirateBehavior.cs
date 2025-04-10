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
<<<<<<< Updated upstream
<<<<<<< Updated upstream
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
=======
    public float movementDelay = 0.1f; // Delay between movement steps
    private TimeControl timeControl;
    private float movementTimer;
    private List<Vector2Int> travelPath = new List<Vector2Int>(); // For replay (if needed)
    private string filePath;
    public bool hasCargo = false; // When true, let ShipInteractions handle movement

    void Start()
    {
        timeControl = FindObjectOfType<TimeControl>();
        int startX = Random.Range(0, gridSize.x);
        int startY = 0;
        currentGridPosition = new Vector2Int(startX, startY);
        int destinationY = gridSize.y;
        destinationGridPosition = new Vector2Int(startX, destinationY);
        transform.position = GridToWorld(currentGridPosition);
        travelPath.Add(currentGridPosition);
        filePath = Path.Combine(Application.persistentDataPath, "ShipTravelPath.txt");
>>>>>>> Stashed changes
=======
    public float movementDelay = 0.1f; // Delay between movement steps
    private TimeControl timeControl;
    private float movementTimer;
    private List<Vector2Int> travelPath = new List<Vector2Int>(); // For replay (if needed)
    private string filePath;
    public bool hasCargo = false; // When true, let ShipInteractions handle movement

    void Start()
    {
        timeControl = FindObjectOfType<TimeControl>();
        int startX = Random.Range(0, gridSize.x);
        int startY = 0;
        currentGridPosition = new Vector2Int(startX, startY);
        int destinationY = gridSize.y;
        destinationGridPosition = new Vector2Int(startX, destinationY);
        transform.position = GridToWorld(currentGridPosition);
        travelPath.Add(currentGridPosition);
        filePath = Path.Combine(Application.persistentDataPath, "ShipTravelPath.txt");
>>>>>>> Stashed changes
    }

    void Update()
    {
<<<<<<< Updated upstream
<<<<<<< Updated upstream
        // If movement is active, move the ship step-by-step
        if (isMoving)
=======
=======
>>>>>>> Stashed changes
        if (hasCargo) return;
        if (timeControl.ShouldMove())
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
    public void StartMovement()
    {
        isMoving = true;
        Debug.Log("Movement started!");
=======
    // This method may also be called via SendMessage("Step")
    public void Step()
    {
        // Optionally, you can call MoveShipTowardsDestination() here if desired.
>>>>>>> Stashed changes
=======
    // This method may also be called via SendMessage("Step")
    public void Step()
    {
        // Optionally, you can call MoveShipTowardsDestination() here if desired.
>>>>>>> Stashed changes
    }

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
            Debug.Log($"Ship moved to: {currentGridPosition}");
        }
        else
        {
            // Stop movement and save the path when the destination is reached
            isMoving = false;
            SaveTravelPathToFile();
            Debug.Log($"Destination reached at: {destinationGridPosition}");
=======
            // Record this movement event with the current global time.
            if (ReplayManager.Instance != null)
            {
                ReplayManager.Instance.RecordMovementEvent(ShipId, "Pirate", transform.position, transform.rotation, timeControl.GlobalTime);
            }
        }
        else
        {
            SaveTravelPathToFile();
>>>>>>> Stashed changes
=======
            // Record this movement event with the current global time.
            if (ReplayManager.Instance != null)
            {
                ReplayManager.Instance.RecordMovementEvent(ShipId, "Pirate", transform.position, transform.rotation, timeControl.GlobalTime);
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
<<<<<<< Updated upstream
<<<<<<< Updated upstream
        Debug.Log($"Travel path saved to: {filePath}");
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
    }
}
