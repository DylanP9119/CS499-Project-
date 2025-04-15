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
        int startX = Random.Range(0, gridSize.x);
        int startY = 0;
        currentGridPosition = new Vector2Int(startX, startY);
        destinationGridPosition = new Vector2Int(startX, gridSize.y);
        transform.position = GridToWorld(currentGridPosition);
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
        if (hasCargo)
            return;
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

    public Vector3 GridToWorld(Vector2Int gridPosition)
    {
        return new Vector3(gridPosition.x * gridCellSize, 0, gridPosition.y * gridCellSize);
    }
}
