using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Recorder;
using ReplayData;
using Replay;
using static UnityEngine.Rendering.GPUSort;
using UnityEngine.UI;
//using System;

public class ShipController : MonoBehaviour
{
    public GameObject cargoPrefab;
    public GameObject patrolPrefab;
    public GameObject piratePrefab;
    public TextController textController;
    private float globalTime = 0f;
    public Text timeDisplayRun;
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
    public TimeControl timeControl;
    public float simulationSpeed = 1f;
    private float simMinutesPassed = 0f;  // simulated time
    public float simulationLengthHours = 24f; // default, override from UI
    public bool useDayNightCycle = true;
    public static int TimeStepCounter = 0;

    //private Vector2Int gridsize = new Vector2Int(400,100);
    Vector2Int gridSize = new Vector2Int(400, 100); // i dont think vector2int should affect gridsize./ scared to change  
    //Vector2Int gridSize = ShipMovement.gridSize; REMOVED SHIPMOVEMENT
    private List<GameObject> allShips = new List<GameObject>();
    public List<GameObject> GetAllShips() => allShips;

    void Start()
    {
        isPaused = FindFirstObjectByType<TimeControl>();
        if (UIControllerScript.Instance)
        {
            cargoSpawnChance = UIControllerScript.Instance.cargoDayPercent / 100f;
            cargoNightChance = UIControllerScript.Instance.cargoNightPercent / 100f;
            patrolSpawnChance = UIControllerScript.Instance.patrolDayPercent / 100f;
            patrolNightChance = UIControllerScript.Instance.patrolNightPercent / 100f;
            pirateSpawnChance = UIControllerScript.Instance.pirateDayPercent / 100f;
            pirateNightChance = UIControllerScript.Instance.pirateNightPercent / 100f;
        }
    }

    void Update()
    {
        if (isPaused.ShouldMove())
        {
            moveTimer += Time.deltaTime;

            if (moveTimer >= 1f)
            {
                TimeStepCounter++;
                // Simulated time: 1 real second = 5 simulated minutes
                simMinutesPassed += 5f;
                UpdateDayNightCycle();

                int totalMinutes = Mathf.FloorToInt(simMinutesPassed);
                int day = (totalMinutes / 1440) + 1;
                int hour = (totalMinutes / 60) % 24;
                int minute = totalMinutes % 60;

                // Show updated time *before* checking for simulation end
                timeDisplayRun.text = $"Day {day} — {hour:D2}:{minute:D2}";

                if (simMinutesPassed >= simulationLengthHours * 60f)
                {
                    Debug.Log("[SIM END] Reached simulation limit.");
                    isPaused.ToggleMovement(true); // Pause the simulation
                    return;
                }

                SpawnShip();
                foreach (GameObject ship in allShips)
                {
                    if (ship != null)
                    {
                        ship.SendMessage("Step", SendMessageOptions.DontRequireReceiver);
                    }
                }
                ShipInteractions.Instance.CheckForInteractions(allShips);
                moveTimer = 0f;
            }
        }
    }

    void UpdateDayNightCycle()
    {
        if (!useDayNightCycle) // for easier toggle UI stuff
        {
            isNight = false;
            ShipInteractions.Instance.isNight = false;
            return;
        }

        int hour = Mathf.FloorToInt(simMinutesPassed / 60f) % 24;
        bool newNight = (hour >= 12);

        if (newNight != isNight)
        {
            isNight = newNight;
            ShipInteractions.Instance.isNight = isNight;
            Debug.Log($"[DAY/NIGHT] Transitioned to {(isNight ? "Night" : "Day")} at hour {hour}");
        }
    }

    void SpawnShip() // This function will need to be updated for weighted probabilities. Check in with Jacob.
    {

        HashSet<Vector3> occupiedPositions = new HashSet<Vector3>(); // Track used spawn positions

        foreach (GameObject ship in allShips)
        {
            if (ship != null)
                occupiedPositions.Add(ship.transform.position);
        }

        // Cargo Day Spawn
        if (isNight == false && Random.value <= cargoSpawnChance)
        {
            Vector3 spawnPos = GetUniqueSpawnPosition("Cargo", occupiedPositions);
            if (spawnPos != Vector3.zero) // Ensures valid position found
            {
                GameObject cargo = Instantiate(cargoPrefab, spawnPos, GetSpawnRotation("Cargo"));
                cargo.name = $"Cargo({cargoCounter++})";
                textController.UpdateShipEnter("cargo");
                allShips.Add(cargo);

                //textController.UpdateShipEnter("cargo");

                //Debug.Log($"[SPAWN] {cargo.name} spawned at {spawnPos}");

                //Debug.Log($"Cargo Ship Spawned at ({spawnPos.x}, {spawnPos.y})");
            }
            else
            {
                Debug.LogWarning($"[SPAWN FAILED] Cargo could not find valid spawn position at frame {Time.frameCount}");
            }
        }

        // Patrol Day Spawn
        if (isNight == false && Random.value <= patrolSpawnChance)
        {
            Vector3 spawnPos = GetUniqueSpawnPosition("Patrol", occupiedPositions);
            if (spawnPos != Vector3.zero)
            {
                GameObject patrol = Instantiate(patrolPrefab, spawnPos, GetSpawnRotation("Patrol"));
                patrol.name = $"Patrol({patrolCounter++})";
                textController.UpdateShipEnter("patrol");
                allShips.Add(patrol);

                //textController.UpdateShipEnter("patrol");
                //Debug.Log($"Patrol Ship Spawned at ({spawnPos.x}, {spawnPos.y})");
            }
            else
            {
                Debug.LogWarning($"[SPAWN FAILED] Patrol could not find valid spawn position at frame {Time.frameCount}");
            }
        }

        // Pirate Day Spawn
        if (isNight == false && Random.value <= pirateSpawnChance)
        {
            Vector3 spawnPos = GetUniqueSpawnPosition("Pirate", occupiedPositions);
            if (spawnPos != Vector3.zero)
            {
                GameObject pirate = Instantiate(piratePrefab, spawnPos, GetSpawnRotation("Pirate"));
                pirate.name = $"Pirate({pirateCounter++})";
                textController.UpdateShipEnter("pirate");
                allShips.Add(pirate);

                //textController.UpdateShipEnter("pirate");
                //Debug.Log($"Pirate Ship Spawned at ({spawnPos.x}, {spawnPos.y})");
            }
            else
            {
                Debug.LogWarning($"[SPAWN FAILED] pirate could not find valid spawn position at frame {Time.frameCount}");
            }
        }

        // Cargo Night Spawn
        if (isNight == true && Random.value <= cargoNightChance)
        {
            Vector3 spawnPos = GetUniqueSpawnPosition("Cargo", occupiedPositions);
            if (spawnPos != Vector3.zero) // Ensures valid position found
            {
                GameObject cargo = Instantiate(cargoPrefab, spawnPos, GetSpawnRotation("Cargo"));
                allShips.Add(cargo);
                Debug.Log($"Cargo Ship Spawned at ({spawnPos.x}, {spawnPos.y})");

                textController.UpdateShipEnter("cargo");
            }
        }

        // Patrol Night Spawn
        if (isNight == true && Random.value <= patrolNightChance)
        {
            Vector3 spawnPos = GetUniqueSpawnPosition("Patrol", occupiedPositions);
            if (spawnPos != Vector3.zero)
            {
                GameObject patrol = Instantiate(patrolPrefab, spawnPos, GetSpawnRotation("Patrol"));
                allShips.Add(patrol);
                Debug.Log($"Patrol Ship Spawned at ({spawnPos.x}, {spawnPos.y})");

                textController.UpdateShipEnter("patrol");
            }
        }

        // Pirate Night Spawn
        if (isNight == true && Random.value <= pirateNightChance)
        {
            Vector3 spawnPos = GetUniqueSpawnPosition("Pirate", occupiedPositions);
            if (spawnPos != Vector3.zero)
            {
                GameObject pirate = Instantiate(piratePrefab, spawnPos, GetSpawnRotation("Pirate"));
                allShips.Add(pirate);
                Debug.Log($"Pirate Ship Spawned at ({spawnPos.x}, {spawnPos.y})");

                textController.UpdateShipEnter("pirate");
            }
        }
        //Debug.Log($"[SYNC DEBUG] Step {Time.frameCount} � Cargo({cargoCounter}), Patrol({patrolCounter}), Pirate({pirateCounter})");
    }

    Vector3 GetUniqueSpawnPosition(string shipType, HashSet<Vector3> occupiedPositions) // This function will need to be updated for weighted probabilities. Check in with Jacob.
    {
        //float roll = Random.value;
        Vector3 spawnPos = Vector3.zero;
        int maxAttempts = 9999; // Avoid infinite loops

        for (int i = 0; i < maxAttempts; i++)
        {
            if (shipType == "Cargo")
            {
                int spawnZ = Random.Range(0, gridSize.y);
                spawnPos = new Vector3(0, 0, spawnZ);
            }
            else if (shipType == "Pirate")
            {
                int spawnX = Random.Range(0, gridSize.x);
                spawnPos = new Vector3(spawnX, 0, 0);
            }
            else if (shipType == "Patrol")
            {
                int spawnZ = Random.Range(0, gridSize.y);
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
            else
            {
                Debug.Log($"[BLOCKED] {shipType} attempted spawn at {spawnPos} -> already occupied this frame.");
            }
            if (shipType == "Pirate")
            {
                int pirateRowBlocked = 0;
                for (int x = 0; x < gridSize.x; x++)
                {
                    if (occupiedPositions.Contains(new Vector3(x, 0, 0)))
                        pirateRowBlocked++;
                }
                Debug.LogWarning($"[SPAWN SATURATION] {pirateRowBlocked} pirate row positions are blocked.");
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