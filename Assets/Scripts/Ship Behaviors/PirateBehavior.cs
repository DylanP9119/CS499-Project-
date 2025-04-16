using UnityEngine;

public class PirateBehavior : MonoBehaviour
{
    public Vector2Int currentGridPosition;
    public Vector2Int destinationGridPosition;
    public Vector2Int gridSize = new Vector2Int(400, 100);
    public float gridCellSize = 1f;
    public float movementDelay = 0.1f;
    private float movementTimer = 0f;
    public bool hasCargo = false;

    void Start()
    {
    if (ReplayManager.Instance != null && ReplayManager.Instance.ReplayModeActive)
    {
        currentGridPosition = WorldToGrid(transform.position);
        destinationGridPosition = new Vector2Int(currentGridPosition.x, gridSize.y);
    }        
    else
    {
        int startX = Random.Range(0, gridSize.x);
        int startY = 0;
        currentGridPosition = new Vector2Int(startX, startY);
        destinationGridPosition = new Vector2Int(startX, gridSize.y);
        transform.position = GridToWorld(currentGridPosition);
    }
    }

    public void Step(bool forceMove)
    {
        if (hasCargo)
            return;

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
        if (currentGridPosition != destinationGridPosition)
        {
            int direction = 1;
            if (ReplayManager.Instance != null && ReplayManager.Instance.ReplayModeActive && ReplayManager.Instance.replaySpeed < 0)
                direction = -1;
            currentGridPosition += Vector2Int.up * direction;
            transform.position = GridToWorld(currentGridPosition);
        }
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
