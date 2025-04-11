using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Recorder;
using ReplayData;
using Replay;
using static UnityEngine.Rendering.GPUSort;
//using System;

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

    private int cargoCounter = 1;
    private int pirateCounter = 1;
    private int patrolCounter = 1;

    public float simulationSpeed = 1f;

    //private Vector2Int gridsize = new Vector2Int(400,100);
    Vector2Int gridSize = new Vector2Int(400, 100); // i dont think vector2int should affect gridsize./ scared to change  
    //Vector2Int gridSize = ShipMovement.gridSize; REMOVED SHIPMOVEMENT
    private List<GameObject> allShips = new List<GameObject>();
    public List<GameObject> GetAllShips() => allShips;

    void Start()
    {
        isPaused = FindFirstObjectByType<TimeControl>();
        if (replay == null)
            replay = GameObject.Find("ReplayManager").GetComponent<ReplayManager>();
        if (UIControllerScript.Instance)
        {
            cargoSpawnChance = UIControllerScript.Instance.cargoDayPercent;
            cargoNightChance = UIControllerScript.Instance.cargoNightPercent;
            patrolSpawnChance = UIControllerScript.Instance.patrolDayPercent;
            patrolNightChance = UIControllerScript.Instance.patrolNightPercent;
            pirateSpawnChance = UIControllerScript.Instance.pirateDayPercent;
            pirateNightChance = UIControllerScript.Instance.pirateNightPercent;
        }
    }

    void Update()
    {
        if (isPaused.ShouldMove())
        {
            if (replay.ReplayMode())
            {
                return;
            }
            moveTimer += Time.deltaTime;

            if (moveTimer >= 1f)
            {
                SpawnShip();
                foreach (GameObject ship in allShips)
                {
                    if (ship != null)
                    {
                        Debug.Log($"[ShipController] Step() called on: {ship.name}");
                        ship.SendMessage("Step", SendMessageOptions.DontRequireReceiver);
                    }
                }
                ShipInteractions.Instance.CheckForInteractions(allShips);
                moveTimer = 0f;
            }
        }
    }

    void SpawnShip() // This function will need to be updated for weighted probabilities. Check in with Jacob.
    {
        HashSet<Vector3> occupiedPositions = new HashSet<Vector3>(); // Track used spawn positions

        // Cargo Day Spawn
        if (isNight == false && Random.value < cargoSpawnChance)
        {
            Vector3 spawnPos = GetUniqueSpawnPosition("Cargo", occupiedPositions);
            if (spawnPos != Vector3.zero) // Ensures valid position found
            {
                GameObject cargo = Instantiate(cargoPrefab, spawnPos, GetSpawnRotation("Cargo"));
                cargo.name = $"Cargo({cargoCounter++})";
                allShips.Add(cargo);
                //Debug.Log($"[SPAWN] {cargo.name} spawned at {spawnPos}");

                //Debug.Log($"Cargo Ship Spawned at ({spawnPos.x}, {spawnPos.y})");
            }
        }

        // Patrol Day Spawn
        if (isNight == false && Random.value < patrolSpawnChance)
        {
            Vector3 spawnPos = GetUniqueSpawnPosition("Patrol", occupiedPositions);
            if (spawnPos != Vector3.zero)
            {
                GameObject patrol = Instantiate(patrolPrefab, spawnPos, GetSpawnRotation("Patrol"));
                patrol.name = $"Patrol({patrolCounter++})";
                allShips.Add(patrol);
                //Debug.Log($"Patrol Ship Spawned at ({spawnPos.x}, {spawnPos.y})");
            }
        }

        // Pirate Day Spawn
        if (isNight == false && Random.value < pirateSpawnChance)
        {
            Vector3 spawnPos = GetUniqueSpawnPosition("Pirate", occupiedPositions);
            if (spawnPos != Vector3.zero)
            {
                GameObject pirate = Instantiate(piratePrefab, spawnPos, GetSpawnRotation("Pirate"));
                pirate.name = $"Pirate({pirateCounter++})";
                allShips.Add(pirate);
                //Debug.Log($"Pirate Ship Spawned at ({spawnPos.x}, {spawnPos.y})");
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

    Vector3 GetUniqueSpawnPosition(string shipType, HashSet<Vector3> occupiedPositions) // This function will need to be updated for weighted probabilities. Check in with Jacob.
    {
        float roll = Random.value;
        Vector3 spawnPos = Vector3.zero;
        int maxAttempts = 10; // Avoid infinite loops

        for (int i = 0; i < maxAttempts; i++)
        {
            if (shipType == "Cargo")
            {
                int spawnZ = Mathf.FloorToInt(gridSize.y * roll);
                spawnPos = new Vector3(0, 0, spawnZ);
            }
            else if (shipType == "Pirate")
            {
                int spawnX = Mathf.FloorToInt(gridSize.x * roll);
                spawnPos = new Vector3(spawnX, 0, 0);
            }
            else if (shipType == "Patrol")
            {
                int spawnZ = Mathf.FloorToInt(gridSize.y * roll);
                spawnPos = new Vector3(gridSize.x - 1, 0, spawnZ);
            }
            else
            {
                return Vector3.zero;
            }

            if (!occupiedPositions.Contains(spawnPos))
            {
                occupiedPositions.Add(spawnPos); // Mark as occupied
                return spawnPos;
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