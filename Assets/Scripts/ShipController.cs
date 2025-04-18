using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class ShipController : MonoBehaviour
{
    // Static simulation tick counter.
    public static int TimeStepCounter { get; private set; } = 0;
    // For simulation testing, we use a shorter tick duration.
    public float simulationTickDuration = 1f;

    public GameObject cargoPrefab;
    public GameObject patrolPrefab;
    public GameObject piratePrefab;
    public TextController textController;
    public Text timeDisplayRun;
    public Text timeDisplayRunReplay;     // replay mode clock
    public Text timeDisplayRemaining;
    public bool isNight = false;
    public float cargoSpawnChance = 0.50f;
    public float patrolSpawnChance = 0.25f;
    public float pirateSpawnChance = 0.40f;
    public float cargoNightChance = 0.50f;
    public float patrolNightChance = 0.25f;
    public float pirateNightChance = 0.40f;
    float spawnTimer = 0.0f;
    private TimeControl timeControl;
    private int cargoCounter = 1, patrolCounter = 1, pirateCounter = 1;
    public float simulationLengthHours = 24f;
    private float simMinutesPassed = 0f;
    Vector2Int gridSize = new Vector2Int(400, 100);
    public List<GameObject> allShips = new List<GameObject>();

    void Start()
    {
        timeControl = FindObjectOfType<TimeControl>();
        if (DataPersistence.Instance != null)
        {
            cargoSpawnChance = DataPersistence.Instance.cargoDayPercent / 100f;
            cargoNightChance = DataPersistence.Instance.cargoNightPercent / 100f;
            pirateSpawnChance = DataPersistence.Instance.pirateDayPercent / 100f;
            pirateNightChance = DataPersistence.Instance.pirateNightPercent / 100f;
            patrolSpawnChance = DataPersistence.Instance.patrolDayPercent / 100f;
            patrolNightChance = DataPersistence.Instance.patrolNightPercent / 100f;

            simulationLengthHours = (DataPersistence.Instance.dayCount * 24) + DataPersistence.Instance.hourCount;
        }
    }

    void Update()
    {
        // If in Replay Mode, handle via ReplayManager.
        if (ReplayManager.Instance != null && ReplayManager.Instance.ReplayModeActive)
        {
            if (!ReplayManager.Instance.ReplayPaused)
            {
                int currentTick = Mathf.FloorToInt(ReplayManager.Instance.replayTime / simulationTickDuration);
                SetTimeStepCounter(currentTick);
                ShipInteractions.Instance.CheckForInteractions(allShips);
            }
            return;
        }

        // Simulation mode:
        if (!timeControl.IsPaused)
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= simulationTickDuration)
            {
                // Advance simulation tick.
                TimeStepCounter++;
                simMinutesPassed += 5f;
                UpdateDayNightCycle();
                int totalMinutes = Mathf.FloorToInt(simMinutesPassed);
                int day = (totalMinutes / 1440) + 1;
                int hour = (totalMinutes / 60) % 24;
                int minute = totalMinutes % 60;

                // Current time display
                timeDisplayRun.text = $"Day {day} — {hour:D2}:{minute:D2}";

                // Calculate remaining time
                float remainingMinutes = simulationLengthHours * 60f - simMinutesPassed;
                if (remainingMinutes < 0) remainingMinutes = 0;
                int remainingDays = Mathf.FloorToInt(remainingMinutes / 1440f);
                int remainingHours = Mathf.FloorToInt((remainingMinutes % 1440) / 60f);
                int remainingMins = Mathf.FloorToInt(remainingMinutes % 60f);

                timeDisplayRemaining.text = $"Remaining: {remainingDays}d {remainingHours}h {remainingMins}m";

                if (simMinutesPassed >= simulationLengthHours * 60f)
                {
                    Debug.Log("[SIM END] Reached simulation limit.");
                    timeControl.ToggleMovement(true); // Pause simulation
                    return;
                }

                float simTime = TimeStepCounter * simulationTickDuration;
                SpawnShip(simTime);

                // Explicitly call Step on each ship's behavior component.
                foreach (GameObject ship in allShips)
                {
                    if (ship == null)
                        continue;
                    if (ship.CompareTag("Cargo"))
                    {
                        var cargo = ship.GetComponent<CargoBehavior>();
                        if (cargo != null)
                            cargo.Step(true);
                    }
                    else if (ship.CompareTag("Patrol"))
                    {
                        var patrol = ship.GetComponent<PatrolBehavior>();
                        if (patrol != null)
                            patrol.Step(true);
                    }
                    else if (ship.CompareTag("Pirate"))
                    {
                        var pirate = ship.GetComponent<PirateBehavior>();
                        if (pirate != null)
                            pirate.Step(true);
                    }
                }
                ShipInteractions.Instance.CheckForInteractions(allShips);
                spawnTimer = 0f;
            }
        }
    }

    void UpdateSpawnChancesFromUI()
    {
        // Implementation depends on UIControllerScript
    }

    void UpdateDayNightCycle()
    {
        if (!useDayNightCycle) return;
        int hour = Mathf.FloorToInt(simMinutesPassed / 60f) % 24;
        ShipInteractions.Instance.isNight = isNight = (hour >= 12);
    }

    void UpdateTimeDisplays()
    {
        int totalMinutes = Mathf.FloorToInt(simMinutesPassed);
        int day = (totalMinutes / 1440) + 1;
        int hour = (totalMinutes / 60) % 24;
        int minute = totalMinutes % 60;
        timeDisplayRun.text = $"Day {day} — {hour:D2}:{minute:D2}";

        float remainingMinutes = simulationLengthHours * 60f - simMinutesPassed;
        int remainingDays = Mathf.FloorToInt(remainingMinutes / 1440f);
        int remainingHours = Mathf.FloorToInt((remainingMinutes % 1440) / 60f);
        int remainingMins = Mathf.FloorToInt(remainingMinutes % 60f);
        timeDisplayRemaining.text = $"Remaining: {remainingDays}d {remainingHours}h {remainingMins}m";
    }

    void StepAllShips()
    {
        foreach (GameObject ship in allShips)
        {
                   if (ship == null) continue;
    
            if (ship == null) continue;
            if (ship.CompareTag("Cargo")) ship.GetComponent<CargoBehavior>()?.Step(true);
            else if (ship.CompareTag("Patrol")) ship.GetComponent<PatrolBehavior>()?.Step(true);
            else if (ship.CompareTag("Pirate")) ship.GetComponent<PirateBehavior>()?.Step(true);
        }
    }

    void SpawnShip(float simTime)
    {
        HashSet<Vector3> occupiedPositions = new HashSet<Vector3>();
        foreach (GameObject ship in allShips)
            if (ship != null) occupiedPositions.Add(ship.transform.position);

        TrySpawnShip("Cargo", occupiedPositions, simTime, isNight);
        TrySpawnShip("Patrol", occupiedPositions, simTime, isNight);
        TrySpawnShip("Pirate", occupiedPositions, simTime, isNight);
    }

    void TrySpawnShip(string shipType, HashSet<Vector3> occupiedPositions, float simTime, bool isNight)
    {
        Vector3 spawnPos = GetUniqueSpawnPosition(shipType, occupiedPositions);
        if (spawnPos == Vector3.zero) return;

        GameObject prefab = GetPrefab(shipType);
        if (prefab == null) return;

        GameObject ship = Instantiate(prefab, spawnPos, GetSpawnRotation(shipType));
        InitializeShipBehavior(ship, shipType, spawnPos);
        ship.name = $"{shipType}({GetCounter(ref shipType)})";
        ship.tag = shipType;
        allShips.Add(ship);
        textController.UpdateShipEnter(shipType.ToLower());

        if (ReplayManager.Instance != null)
        {
            int shipId = ReplayManager.Instance.GetNextShipId();
            // CHANGED: Use step-based timestamp instead of cumulativeSimTime
            float stepTime = TimeStepCounter * ReplayManager.Instance.simulationTickDuration;
            ReplayManager.Instance.RecordShipSpawn(shipId, shipType, spawnPos, ship.transform.rotation, stepTime);
        }
    }

    Vector3 GetUniqueSpawnPosition(string shipType, HashSet<Vector3> occupiedPositions)
    {
        int maxAttempts = 400;
        Vector3 spawnPos = Vector3.zero;
        for (int i = 0; i < maxAttempts; i++)
        {
            float roll = Random.value;
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
                return Vector3.zero;
            if (!occupiedPositions.Contains(spawnPos))
            {
                occupiedPositions.Add(spawnPos);
                return spawnPos;
            }
        }
        return Vector3.zero;
    }

    Vector3 CalculateSpawnPosition(string shipType, float cellSize)
    {
        float roll = Random.value;
        return shipType switch
        {
            "Cargo" => new Vector3(0, 0, Mathf.FloorToInt(gridSize.y * roll) * cellSize),
            "Pirate" => new Vector3(Mathf.FloorToInt(gridSize.x * roll) * cellSize, 0, 0),
            "Patrol" => new Vector3((gridSize.x - 1) * cellSize, 0, Mathf.FloorToInt(gridSize.y * roll) * cellSize),
            _ => Vector3.zero
        };
    }

    Quaternion GetSpawnRotation(string shipType)
    {
        return shipType switch
        {
            "Cargo" => Quaternion.Euler(0, 90, 0),
            "Patrol" => Quaternion.Euler(0, -90, 0),
            "Pirate" => Quaternion.Euler(0, 0, 0),
            _ => Quaternion.identity
        };
    }

    int GetCounter(ref string shipType)
    {
        return shipType switch
        {
            "Cargo" => cargoCounter++,
            "Patrol" => patrolCounter++,
            "Pirate" => pirateCounter++,
            _ => 0
        };
    }

    GameObject GetPrefab(string shipType)
    {
        return shipType switch
        {
            "Cargo" => cargoPrefab,
            "Patrol" => patrolPrefab,
            "Pirate" => piratePrefab,
            _ => null
        };
    }
    public static int SelectIndexByWeight(double[] weights)
    {
        // Step 1: Calculate the sum of all weights
        double totalWeight = 0;
        foreach (double weight in weights)
        {
            totalWeight += weight;
        }

        // Step 2: Normalize the weights and create cumulative distribution
        double[] cumulative = new double[weights.Length];
        double cumulativeSum = 0;
        for (int i = 0; i < weights.Length; i++)
        {
            cumulativeSum += weights[i] / totalWeight; // Normalize weight
            cumulative[i] = cumulativeSum;
        }

        // Step 3: Generate a random number between 0 and 1

        double randomNumber = Random.value;

        // Step 4: Find the index using the cumulative distribution
        for (int i = 0; i < cumulative.Length; i++)
        {
            if (randomNumber <= cumulative[i])
            {
                return i;
            }
        }

        return -1; // Fallback (shouldn't occur with valid input)
    }
    void InitializeShipBehavior(GameObject ship, string shipType, Vector3 spawnPos)
    {
        switch (shipType)
        {
            case "Cargo":
                var cargo = ship.GetComponent<CargoBehavior>();
                cargo.gridSize = gridSize;
                cargo.currentGridPosition = new Vector2Int(0, Mathf.FloorToInt(spawnPos.z));
                cargo.destinationGridPosition = new Vector2Int(gridSize.x, cargo.currentGridPosition.y);
                break;
            case "Patrol":
                var patrol = ship.GetComponent<PatrolBehavior>();
                patrol.gridSize = gridSize;
                patrol.currentGridPosition = new Vector2Int(gridSize.x - 1, Mathf.FloorToInt(spawnPos.z));
                patrol.destinationGridPosition = new Vector2Int(0, patrol.currentGridPosition.y);
                break;
            case "Pirate":
                var pirate = ship.GetComponent<PirateBehavior>();
                pirate.gridSize = gridSize;
                pirate.currentGridPosition = new Vector2Int(Mathf.FloorToInt(spawnPos.x), 0);
                pirate.destinationGridPosition = new Vector2Int(pirate.currentGridPosition.x, gridSize.y);
                break;
        }
    }

 public void ReplaySpawn(string shipType, Vector3 position, Quaternion rotation, string shipName, int shipId)
{
    GameObject prefab = GetPrefab(shipType);
    if (prefab == null)
    {
        Debug.LogError($"Prefab not found for {shipType}");
        return;
    }

    GameObject ship = Instantiate(prefab, position, rotation);
    if (ship == null)
    {
        Debug.LogError("Failed to instantiate ship: " + shipType);
        return;
    }

    ship.name = shipName;
    ship.tag = shipType;
    allShips.Add(ship);

    if (textController != null)
    {
        textController.UpdateShipEnter(shipType.ToLower());
    }
    else
    {
        Debug.LogWarning("TextController reference is missing in ShipController");
    }
}
    public void ClearAllShips()
    {
        foreach (GameObject ship in allShips)
            if (ship != null) Destroy(ship);
        allShips.Clear();
    }

    public static void SetTimeStepCounter(int newTick) => TimeStepCounter = newTick;
}
