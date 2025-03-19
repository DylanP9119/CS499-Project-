using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ShipController : MonoBehaviour
{
    public GameObject cargoPrefab;
    public GameObject patrolPrefab;
    public GameObject piratePrefab;

    public float cargoSpawnChance = 0.50f;
    public float patrolSpawnChance = 0.25f;
    public float pirateSpawnChance = 0.40f;

    float moveTimer = 0.0f;
    private TimeControl isPaused;

    //private Vector2Int gridsize = new Vector2Int(400,100);
    Vector2Int gridSize = ShipMovement.gridSize; // Access gridSize from ShipMovement, size of grid should not be made in ship movement. 

    void Start()
    {
        isPaused = FindFirstObjectByType<TimeControl>();
    }
    void Update()
    {
        if(isPaused.ShouldMove())
        {
            moveTimer += Time.deltaTime;    
            if(moveTimer >= 1)   
            {
                SpawnShip();
                moveTimer = 0;
            }
        }
    }

    void SpawnShip()
    {
        // Cargo Spawn
        if (Random.value < cargoSpawnChance)
        {
            Vector3 spawnPos = GetSpawnPosition("Cargo");
            Instantiate(cargoPrefab, spawnPos, GetSpawnRotation("Cargo"));
            Debug.Log($"Cargo Ship Spawned at ({spawnPos.x}, {spawnPos.y})");
        }

        // Patrol Spawn
        if (Random.value < patrolSpawnChance)
        {
            Vector3 spawnPos = GetSpawnPosition("Patrol");
            Instantiate(patrolPrefab, spawnPos, GetSpawnRotation("Patrol"));
            Debug.Log($"Patrol Ship Spawned at ({spawnPos.x}, {spawnPos.y})");
        }

        // Pirate Spawn
        if (Random.value < pirateSpawnChance)
        {
            Vector3 spawnPos = GetSpawnPosition("Pirate");
            Instantiate(piratePrefab, spawnPos, GetSpawnRotation("Pirate"));
            Debug.Log($"Pirate Ship Spawned at ({spawnPos.x}, {spawnPos.y})");
        }
    }
    
    Vector3 GetSpawnPosition(string shipType)
    { 
        if (shipType == "Cargo")
        {
            int spawnY = Mathf.FloorToInt(gridSize.y * Random.value); // Row select
            return new Vector3(0, spawnY, 0); // Left Side (west)
        }
        else if (shipType == "Patrol")
        {
            int spawnY = Mathf.FloorToInt(gridSize.y * Random.value); // Row select
            return new Vector3(gridSize.x - 1, spawnY, 0); // Right Side (east)
        } 
        else if (shipType == "Pirate")
        {
            int spawnX = Mathf.FloorToInt(gridSize.x * Random.value); // Row select
            return new Vector3(spawnX, 0, 0); // Bottom Part (South)
        }
        return Vector3.zero;
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

        return Quaternion.identity; // Default
    }
}


//Next steps: UI panel for user inputs
// Input field? Ask jacob what UI feature we are going to use
// 