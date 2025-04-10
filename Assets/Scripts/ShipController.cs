using System.Collections.Generic;
using UnityEngine;
<<<<<<< Updated upstream
using Recorder; 
using ReplayData;
using Replay;
=======
>>>>>>> Stashed changes

public class ShipController : MonoBehaviour
{
    public GameObject cargoPrefab;
    public GameObject patrolPrefab;
    public GameObject piratePrefab;
<<<<<<< Updated upstream
        public ReplayManager replay;
=======
    public ReplayManager replay;
    public bool isNight = false;
>>>>>>> Stashed changes

    public float cargoSpawnChance = 0.50f;
    public float patrolSpawnChance = 0.25f;
    public float pirateSpawnChance = 0.40f;
<<<<<<< Updated upstream

    float moveTimer = 0.0f;
    private TimeControl isPaused;

    //private Vector2Int gridsize = new Vector2Int(400,100);
    Vector2Int gridSize = ShipMovement.gridSize; // Access gridSize from ShipMovement, size of grid should not be made in ship movement. 
=======
    public float cargoNightChance = 0.50f;
    public float patrolNightChance = 0.25f;
    public float pirateNightChance = 0.40f;

    public TimeControl timeControl;
    private int cargoCounter = 1;
    private int patrolCounter = 1;
    private int pirateCounter = 1;
    private Vector2Int gridSize = new Vector2Int(400, 100);
    public List<GameObject> allShips = new List<GameObject>();
>>>>>>> Stashed changes

    void Start()
    {
        timeControl = FindObjectOfType<TimeControl>();
        if (replay == null)
<<<<<<< Updated upstream
                replay = GameObject.Find("ReplayManager").GetComponent<ReplayManager>();
=======
            replay = GameObject.Find("ReplayManager").GetComponent<ReplayManager>();
>>>>>>> Stashed changes
    }

    void Update()
    {
<<<<<<< Updated upstream
        if(isPaused.ShouldMove())
=======
        // Only run normal simulation when not in replay mode.
        if (timeControl.ShouldMove() && !replay.ReplayModeActive)
>>>>>>> Stashed changes
        {
            SpawnShip();
            // Tell each ship to perform its movement step.
            foreach (GameObject ship in allShips)
            {
<<<<<<< Updated upstream
                return;
            } 
            moveTimer += Time.deltaTime;    
            if(moveTimer >= 1)   
            {
                SpawnShip();
                moveTimer = 0;
=======
                if (ship != null && !replay.ReplayModeActive)
                    ship.SendMessage("Step", SendMessageOptions.DontRequireReceiver);
>>>>>>> Stashed changes
            }
            // (Optional) Check for interactions here.
            timeControl.ResetTimer();
        }
    }

    void SpawnShip()
    {
<<<<<<< Updated upstream
        // Cargo Spawn
        if (Random.value < cargoSpawnChance)
        {
            Vector3 spawnPos = GetSpawnPosition("Cargo");
            GameObject ship = Instantiate(cargoPrefab, spawnPos, GetSpawnRotation("Cargo"));
            ship.GetComponent<Record>().shipType = "Cargo";
            Debug.Log($"Cargo Ship Spawned at ({spawnPos.x}, {spawnPos.y})");
        }

        // Patrol Spawn
        if (Random.value < patrolSpawnChance)
        {
            Vector3 spawnPos = GetSpawnPosition("Patrol");
            GameObject ship = Instantiate(patrolPrefab, spawnPos, GetSpawnRotation("Patrol"));
            ship.GetComponent<Record>().shipType = "Patrol";
            Debug.Log($"Patrol Ship Spawned at ({spawnPos.x}, {spawnPos.y})");
        }

        // Pirate Spawn
        if (Random.value < pirateSpawnChance)
        {
            Vector3 spawnPos = GetSpawnPosition("Pirate");
            GameObject ship = Instantiate(piratePrefab, spawnPos, GetSpawnRotation("Pirate"));
            ship.GetComponent<Record>().shipType = "Pirate";
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
=======
        HashSet<Vector3> occupiedPositions = new HashSet<Vector3>();

        if (!isNight)
        {
            if (Random.value < cargoSpawnChance) SpawnShipType("Cargo", cargoPrefab, ref cargoCounter, occupiedPositions);
            if (Random.value < patrolSpawnChance) SpawnShipType("Patrol", patrolPrefab, ref patrolCounter, occupiedPositions);
            if (Random.value < pirateSpawnChance) SpawnShipType("Pirate", piratePrefab, ref pirateCounter, occupiedPositions);
        }
        else
        {
            if (Random.value < cargoNightChance) SpawnShipType("Cargo", cargoPrefab, ref cargoCounter, occupiedPositions);
            if (Random.value < patrolNightChance) SpawnShipType("Patrol", patrolPrefab, ref patrolCounter, occupiedPositions);
            if (Random.value < pirateNightChance) SpawnShipType("Pirate", piratePrefab, ref pirateCounter, occupiedPositions);
        }
    }

    void SpawnShipType(string shipType, GameObject prefab, ref int counter, HashSet<Vector3> occupiedPositions)
    {
        Vector3 spawnPos = GetUniqueSpawnPosition(shipType, occupiedPositions);
        if (spawnPos != Vector3.zero)
        {
            Quaternion rotation = GetSpawnRotation(shipType);
            GameObject ship = Instantiate(prefab, spawnPos, rotation);
            int shipId = counter++;
            ship.name = $"{shipType}({shipId})";
            allShips.Add(ship);

            // Set the ShipId property on the appropriate behavior script.
            if (shipType == "Cargo")
            {
                var cargo = ship.GetComponent<CargoBehavior>();
                if (cargo != null)
                    cargo.ShipId = shipId;
            }
            else if (shipType == "Patrol")
            {
                var patrol = ship.GetComponent<PatrolBehavior>();
                if (patrol != null)
                    patrol.ShipId = shipId;
            }
            else if (shipType == "Pirate")
            {
                var pirate = ship.GetComponent<PirateBehavior>();
                if (pirate != null)
                    pirate.ShipId = shipId;
            }

            // Record the spawn event (flagged as a spawn event).
            replay.RecordShipSpawn(new ReplayManager.ReplayEvent(shipId, shipType, spawnPos, rotation, timeControl.GlobalTime, true));
        }
    }

    Vector3 GetUniqueSpawnPosition(string shipType, HashSet<Vector3> occupiedPositions)
    {
        float roll = Random.value;
        Vector3 spawnPos = Vector3.zero;
        int maxAttempts = 10;

        for (int i = 0; i < maxAttempts; i++)
        {
            if (shipType == "Cargo")
                spawnPos = new Vector3(0, 0, Mathf.FloorToInt(gridSize.y * roll));
            else if (shipType == "Pirate")
                spawnPos = new Vector3(Mathf.FloorToInt(gridSize.x * roll), 0, 0);
            else if (shipType == "Patrol")
                spawnPos = new Vector3(gridSize.x - 1, 0, Mathf.FloorToInt(gridSize.y * roll));

            if (!occupiedPositions.Contains(spawnPos))
            {
                occupiedPositions.Add(spawnPos);
                return spawnPos;
            }
        }
>>>>>>> Stashed changes
        return Vector3.zero;
    }

    Quaternion GetSpawnRotation(string shipType)
    {
        return shipType switch
        {
            "Cargo" => Quaternion.Euler(0, 90, 0),
            "Patrol" => Quaternion.Euler(0, -90, 0),
            _ => Quaternion.Euler(0, 0, 0),
        };
    }

    public void ClearAllShips()
{
    foreach (ShipMovement ship in FindObjectsOfType<ShipMovement>())
    {
<<<<<<< Updated upstream
        Destroy(ship.gameObject);
    }
}
}

//Next steps: UI panel for user inputs
// Input field? Ask jacob what UI feature we are going to use
// 
=======
        foreach (GameObject ship in allShips)
        {
            if (ship != null)
                Destroy(ship);
        }
        allShips.Clear();
    }
}
>>>>>>> Stashed changes
