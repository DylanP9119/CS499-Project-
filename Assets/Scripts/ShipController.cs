using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Recorder;
using ReplayData;
using Replay;

public class ShipController : MonoBehaviour
{
    public GameObject cargoPrefab;
    public GameObject patrolPrefab;
    public GameObject piratePrefab;
    public ReplayManager replay;

    public bool isNight = false;

    public float cargoSpawnChance = 0.50f;
    public float patrolSpawnChance = 0.25f;
    public float pirateSpawnChance = 0.40f;

    public float cargoNightChance = 0.50f;
    public float patrolNightChance = 0.25f;
    public float pirateNightChance = 0.40f;

    float moveTimer = 0.0f;
    private TimeControl isPaused;

    //private Vector2Int gridsize = new Vector2Int(400,100);
    Vector2Int gridSize = ShipMovement.gridSize; // Access gridSize from ShipMovement, size of grid should not be made in ship movement. 
    private List<GameObject> allShips = new List<GameObject>();
    public List<GameObject> GetAllShips() => allShips;


    void Start()
    {
        isPaused = FindFirstObjectByType<TimeControl>();
        if (replay == null)
            replay = GameObject.Find("ReplayManager").GetComponent<ReplayManager>();
    }
    void Update()
    {
        if (isPaused.ShouldMove())
        {
            if (replay.ReplayMode())
            {
                return;
            }
            // Check interactions after movement and spawns
            ShipInteractions.Instance.CheckForInteractions(allShips);

            moveTimer += Time.deltaTime;
            if (moveTimer >= 1)
            {
                SpawnShip();
                moveTimer = 0;
            }
        }
    }

    void SpawnShip() // This function will need to be updated for weighted probabilities. Check in with Jacob.
    {
        HashSet<Vector2Int> occupiedPositions = new HashSet<Vector2Int>(); // Track used spawn positions

        // Cargo Day Spawn
        if (isNight == false && Random.value < cargoSpawnChance)
        {
            Vector3 spawnPos = GetUniqueSpawnPosition("Cargo", occupiedPositions);
            if (spawnPos != Vector3.zero) // Ensures valid position found
            {
                GameObject cargo = Instantiate(cargoPrefab, spawnPos, GetSpawnRotation("Cargo"));
                allShips.Add(cargo);
                Debug.Log($"Cargo Ship Spawned at ({spawnPos.x}, {spawnPos.y})");
            }
        }

        // Patrol Day Spawn
        if (isNight == false && Random.value < patrolSpawnChance)
        {
            Vector3 spawnPos = GetUniqueSpawnPosition("Patrol", occupiedPositions);
            if (spawnPos != Vector3.zero)
            {
                GameObject patrol = Instantiate(patrolPrefab, spawnPos, GetSpawnRotation("Patrol"));
                allShips.Add(patrol);
                Debug.Log($"Patrol Ship Spawned at ({spawnPos.x}, {spawnPos.y})");
            }
        }

        // Pirate Day Spawn
        if (isNight == false && Random.value < pirateSpawnChance)
        {
            Vector3 spawnPos = GetUniqueSpawnPosition("Pirate", occupiedPositions);
            if (spawnPos != Vector3.zero)
            {
                GameObject pirate = Instantiate(piratePrefab, spawnPos, GetSpawnRotation("Pirate"));
                allShips.Add(pirate);
                Debug.Log($"Pirate Ship Spawned at ({spawnPos.x}, {spawnPos.y})");
            }
        }

        // Cargo Night Spawn
        if (isNight == true && Random.value < cargoNightChance)
        {
            Vector3 spawnPos = GetUniqueSpawnPosition("Cargo", occupiedPositions);
            if (spawnPos != Vector3.zero) // Ensures valid position found
            {
                GameObject cargo = Instantiate(cargoPrefab, spawnPos, GetSpawnRotation("Cargo"));
                allShips.Add(cargo);
                Debug.Log($"Cargo Ship Spawned at ({spawnPos.x}, {spawnPos.y})");
            }
        }

        // Patrol Night Spawn
        if (isNight == true && Random.value < patrolNightChance)
        {
            Vector3 spawnPos = GetUniqueSpawnPosition("Patrol", occupiedPositions);
            if (spawnPos != Vector3.zero)
            {
                GameObject patrol = Instantiate(patrolPrefab, spawnPos, GetSpawnRotation("Patrol"));
                allShips.Add(patrol);
                Debug.Log($"Patrol Ship Spawned at ({spawnPos.x}, {spawnPos.y})");
            }
        }

        // Pirate Night Spawn
        if (isNight == true && Random.value < pirateNightChance)
        {
            Vector3 spawnPos = GetUniqueSpawnPosition("Pirate", occupiedPositions);
            if (spawnPos != Vector3.zero)
            {
                GameObject pirate = Instantiate(piratePrefab, spawnPos, GetSpawnRotation("Pirate"));
                allShips.Add(pirate);
                Debug.Log($"Pirate Ship Spawned at ({spawnPos.x}, {spawnPos.y})");
            }
        }
    }


    Vector3 GetUniqueSpawnPosition(string shipType, HashSet<Vector2Int> occupiedPositions) // This function will need to be updated for weighted probabilities. Check in with Jacob.
    {
        float roll = Random.value;
        Vector2Int spawnPos;
        int maxAttempts = 10; // Avoid infinite loops

        for (int i = 0; i < maxAttempts; i++)
        {
            if (shipType == "Cargo")
            {
                int spawnY = Mathf.FloorToInt(gridSize.y * roll);
                spawnPos = new Vector2Int(0, spawnY);
            }
            else if (shipType == "Pirate")
            {
                int spawnX = Mathf.FloorToInt(gridSize.x * roll);
                spawnPos = new Vector2Int(spawnX, 0);
            }
            else if (shipType == "Patrol")
            {
                int spawnY = Mathf.FloorToInt(gridSize.y * roll);
                spawnPos = new Vector2Int(gridSize.x - 1, spawnY);
            }
            else
            {
                return Vector3.zero;
            }

            if (!occupiedPositions.Contains(spawnPos))
            {
                occupiedPositions.Add(spawnPos); // Mark as occupied
                return new Vector3(spawnPos.x, spawnPos.y, 0);
            }
        }

        Debug.LogWarning($"No valid spawn position found for {shipType}");
        return Vector3.zero; // Return invalid position if no spot found
    }

    // Making ships face the right orientation
    Quaternion GetSpawnRotation(string shipType)
    {
        if (shipType == "Cargo")
            return Quaternion.Euler(0, 90, 0); //Face East
        else if (shipType == "Patrol")
            return Quaternion.Euler(0, -90, 0); // Face West 
        else if (shipType == "Pirate")
            return Quaternion.Euler(0, 0, 0); // Face North

        return Quaternion.Euler(0, 0, 0); // Default
    }
    public void ClearAllShips()
    {
        foreach (GameObject ship in allShips)
        {
            Destroy(ship);
        }
        allShips.Clear();
    }
}

//Next steps: UI panel for user inputs
// Input field? Ask jacob what UI feature we are going to use
// 