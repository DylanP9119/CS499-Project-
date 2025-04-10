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
    }

    void Update()
    {
        if (hasCargo) return;
        if (timeControl.ShouldMove())
        {
            movementTimer += Time.deltaTime;
            if (movementTimer >= movementDelay)
            {
                movementTimer = 0f;
                MoveShipTowardsDestination();
            }
        }
    }

    // This method may also be called via SendMessage("Step")
    public void Step()
    {
        // Optionally, you can call MoveShipTowardsDestination() here if desired.
    }

    public void MoveShipTowardsDestination()
    {
        if (currentGridPosition != destinationGridPosition)
        {
            Vector2Int direction = GetStepDirection();
            currentGridPosition += direction;
            transform.position = GridToWorld(currentGridPosition);
            travelPath.Add(currentGridPosition);
            // Record this movement event with the current global time.
            if (ReplayManager.Instance != null)
            {
                ReplayManager.Instance.RecordMovementEvent(ShipId, "Pirate", transform.position, transform.rotation, timeControl.GlobalTime);
            }
        }
        else
        {
            SaveTravelPathToFile();
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
    }
}
