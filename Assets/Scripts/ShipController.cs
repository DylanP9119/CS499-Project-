using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ShipController : MonoBehaviour
{
    public GameObject ship;

    public float spawnChance;
    private float chance = 0.0f;
    float moveTimer = 0.0f;

    public GameObject cargoPrefab;
    public GameObject patrolPrefab;
    public GameObject piratePrefab;

    private TimeControl isPaused;

    

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
        chance = Random.Range(0.1f,1.0f);                            
        if(chance <= spawnChance)
        {
            Debug.Log(chance);

            GameObject newShip = null;
            string shipType = "";


            if(chance <= 0.50f)
                {
                    newShip = Instantiate(cargoPrefab, GetSpawnPosition("Cargo"), GetSpawnRotation(""));
                    shipType = "Cargo Ship";
            }
            else if(chance <= 0.50f + .25f )
                {
                    newShip =  Instantiate(patrolPrefab, GetSpawnPosition("Cargo"), GetSpawnRotation(""));
                    shipType = "Patrol Ship";
            }
            else
                {
                    newShip = Instantiate(piratePrefab, GetSpawnPosition("Cargo"), GetSpawnRotation(""));
                    shipType = "Pirate Ship";
            }
            
            if (newShip != null)
            {
                Debug.Log($"{shipType} Spawned at {newShip.transform.position} with rotation {newShip.transform.rotation.eulerAngles}");
            }
        }
    }
    
    Vector3 GetSpawnPosition(string shipType)
    {
        Vector2Int gridSize = ShipMovement.gridSize; // Access gridSize from ShipMovement, size of grid should not be made in ship movement. 

        if (shipType == "Cargo")
        {
            return new Vector3(0, Random.Range(0, gridSize.y), 0); // Left Side (west)
        }
        else if (shipType == "Patrol")
        {
            return new Vector3(gridSize.x - 1, Random.Range(0, gridSize.y), 0); // Right Side (east)
        }
        else if (shipType == "Pirate")
        {
            return new Vector3(Random.Range(0, gridSize.x), 0, 0); // Bottom Part (South)
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
