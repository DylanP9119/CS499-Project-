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
    if (ReplayManager.Instance != null && ReplayManager.Instance.ReplayModeActive)
    {
        currentGridPosition = WorldToGrid(transform.position);
        destinationGridPosition = new Vector2Int(gridSize.x, currentGridPosition.y);
    }
    else
    {
        currentGridPosition = WorldToGrid(transform.position);
        destinationGridPosition = new Vector2Int(gridSize.x, currentGridPosition.y);
    }
    }

    // When forceMove is true, bypass internal timer.
    public void Step(bool forceMove)
    {
        if (forceMove)
        {
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
        if (ReplayManager.Instance != null && ReplayManager.Instance.ReplayModeActive && ReplayManager.Instance.replaySpeed < 0)
            direction = -1;
        currentGridPosition += Vector2Int.right * direction;
        transform.position = GridToWorld(currentGridPosition);
    }
public Vector2Int WorldToGrid(Vector3 worldPosition)
{
    int x = Mathf.FloorToInt(worldPosition.x / gridCellSize);
    int y = Mathf.FloorToInt(worldPosition.z / gridCellSize); // Z-axis corresponds to grid Y
    return new Vector2Int(x, y);
}

    public Vector3 GridToWorld(Vector2Int gridPosition)
    {
        return new Vector3(gridPosition.x * gridCellSize, 0, gridPosition.y * gridCellSize);
    }
}
