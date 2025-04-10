using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CargoBehavior : MonoBehaviour
{
    public int ShipId; // Assigned by ShipController on spawn.
    public Vector2Int currentGridPosition;
    public Vector2Int destinationGridPosition;
    public Vector2Int gridSize = new Vector2Int(400, 100);
    public float gridCellSize = 1f;
    public float movementDelay = 0.1f;
    private TimeControl timeControl;
    private float movementTimer;
    private List<Vector2Int> travelPath = new List<Vector2Int>();
    private string filePath;
    public bool isCaptured = false;

    void Start()
    {
        timeControl = FindObjectOfType<TimeControl>();
        int startX = 0;
        int startY = Random.Range(0, gridSize.y);
        currentGridPosition = new Vector2Int(startX, startY);
        int destinationX = gridSize.x;
        destinationGridPosition = new Vector2Int(destinationX, startY);
        transform.position = GridToWorld(currentGridPosition);
        travelPath.Add(currentGridPosition);
        filePath = Path.Combine(Application.persistentDataPath, "ShipTravelPath.txt");
    }

    void Update()
    {
        if (isCaptured || !timeControl.ShouldMove())
            return;
        movementTimer += Time.deltaTime;
        if (movementTimer >= movementDelay)
        {
            movementTimer = 0f;
            MoveShipTowardsDestination();
        }
    }

    public void Step() { }

    public void MoveShipTowardsDestination()
    {
        if (currentGridPosition != destinationGridPosition)
        {
            Vector2Int direction = GetStepDirection();
            currentGridPosition += direction;
            transform.position = GridToWorld(currentGridPosition);
            travelPath.Add(currentGridPosition);
            if (ReplayManager.Instance != null)
            {
                ReplayManager.Instance.RecordMovementEvent(ShipId, "Cargo", transform.position, transform.rotation, timeControl.GlobalTime);
            }
        }
        else
        {
            SaveTravelPathToFile();
        }
    }

    private Vector2Int GetStepDirection()
    {
        int stepX = 1;
        int stepY = 0;
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
