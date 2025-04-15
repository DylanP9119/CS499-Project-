using UnityEngine;

public class CargoBehavior : MonoBehaviour
{
    public Vector2Int currentGridPosition;
    public Vector2Int destinationGridPosition;
    public Vector2Int gridSize = new Vector2Int(400, 100);
    public float gridCellSize = 1f;
    public float movementDelay = 0.1f;
    private float movementTimer = 0f;
    public bool isCaptured = false;
    public bool isEvadingThisStep = false;

    void Start()
    {
        int startX = 0;
        int startY = Random.Range(0, gridSize.y);
        currentGridPosition = new Vector2Int(startX, startY);
        destinationGridPosition = new Vector2Int(gridSize.x, startY);
        transform.position = GridToWorld(currentGridPosition);
    }

    public void Step(object forceMoveObj = null)
    {
        bool forceMove = (forceMoveObj is bool flag && flag);
        if (forceMove)
        {
            // If captured, do not move.
            if (isCaptured)
                return;
            MoveShipTowardsDestination();
            return;
        }
        movementTimer += Time.deltaTime;
        if (movementTimer < movementDelay)
            return;
        movementTimer = 0f;
        if (isCaptured)
            return;
        isEvadingThisStep = false;

        MoveShipTowardsDestination();
    }

    public void MoveShipTowardsDestination()
    {
        int direction = 1;
        if (ReplayManager.Instance != null && ReplayManager.Instance.ReplayModeActive)
        {
            if (ReplayManager.Instance.replaySpeed < 0)
                direction = -1;
        }
        currentGridPosition += Vector2Int.right * direction;
        transform.position = GridToWorld(currentGridPosition);
    }

    public Vector3 GridToWorld(Vector2Int gridPosition)
    {
        return new Vector3(gridPosition.x * gridCellSize, 0, gridPosition.y * gridCellSize);
    }
}
