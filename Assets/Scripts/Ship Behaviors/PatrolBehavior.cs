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
        int startX = gridSize.x;
        int startY = Random.Range(0, gridSize.y);
        currentGridPosition = new Vector2Int(startX, startY);
        destinationGridPosition = new Vector2Int(0, startY);
        transform.position = GridToWorld(currentGridPosition);
    }

    public void Step()
    {
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
            if (ReplayManager.Instance != null && ReplayManager.Instance.ReplayModeActive)
            {
                if (ReplayManager.Instance.replaySpeed < 0)
                    direction = -1;
            }
            // Default movement to the left; reverse if needed.
            currentGridPosition += Vector2Int.left * direction;
            transform.position = GridToWorld(currentGridPosition);
        }
    }

    public Vector3 GridToWorld(Vector2Int gridPosition)
    {
        return new Vector3(gridPosition.x * gridCellSize, 0, gridPosition.y * gridCellSize);
    }
}
