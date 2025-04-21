using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class ShipController : MonoBehaviour
{
    // Static simulation tick counter.
    public static int TimeStepCounter { get; private set; } = 0;
    // For simulation testing, we use a shorter tick duration.

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
    public static ShipController Instance;
    private int lastReplayTick = -1;
    private Dictionary<int, GameObject> replayedShips = new();
    public Text stepCounterText;



    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // prevent duplicates
            return;
        }
        Instance = this;
    }

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

        UpdateTimeDisplays();
    }

    void Update()
    {
        // Replay mode (when active)
        if (ReplayManager.Instance != null && ReplayManager.Instance.ReplayModeActive)
        {
            if (!ReplayManager.Instance.ReplayPaused)
            {
                int currentTick = Mathf.FloorToInt(ReplayManager.Instance.replayTime / timeControl.GetSpeed());
                if (currentTick != lastReplayTick)
                    {
                    lastReplayTick = currentTick;
                    SetTimeStepCounter(currentTick);
                    ShipInteractions.Instance.CheckForInteractions(allShips);

                    foreach (GameObject ship in allShips)
                    {
                        if (ship == null) continue;

                        if (ship.CompareTag("Cargo"))
                            ship.GetComponent<CargoBehavior>()?.Step(false);
                        else if (ship.CompareTag("Patrol"))
                            ship.GetComponent<PatrolBehavior>()?.Step(false);
                        else if (ship.CompareTag("Pirate"))
                            ship.GetComponent<PirateBehavior>()?.Step(false);
                    }
                }
            }

            return; // exit early if replay is active
        }
            // Simulation mode:
            if (!timeControl.IsPaused)
            {
                spawnTimer += Time.deltaTime;
                if (spawnTimer >= timeControl.GetSpeed())
                {
                    // Advance simulation tick.
                    TimeStepCounter++;
                    if (stepCounterText != null)
                    stepCounterText.text = $"Step: {TimeStepCounter}";
                    //Debug.Log($"Processing Tick: {TimeStepCounter}");
                    simMinutesPassed += 5f;
                    UpdateDayNightCycle();
                    int totalMinutes = Mathf.FloorToInt(simMinutesPassed);
                    int day = (totalMinutes / 1440) + 1;
                    int hour = (totalMinutes / 60) % 24;
                    int minute = totalMinutes % 60;

                    // Current time display
                    string phase = isNight ? "Night" : "Day";
                    timeDisplayRun.text = $"{phase} {day} — {hour:D2}:{minute:D2}";

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

                    float simTime = TimeStepCounter * 1f; //timeControl.GetSpeed();
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
             //       ReplayManager.Instance.RecordTick();
                    //ShipInteractions.Instance.CheckForInteractions(allShips);
                    spawnTimer = 0f;
                }
            }
        
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
        System.Random rand = new System.Random();
        double randomNumber = rand.NextDouble();

        // Step 4: Find the index using the cumulative distribution
        for (int i = 0; i < weights.Length; i++)
        {
            if (randomNumber <= cumulative[i])
            {
                //Debug.Log("Spawning at Grid# " + i);
                return i;
            }
        }

        return -1; // Fallback (shouldn't occur with valid input)
    }

    public void UpdateDayNightCycle()
    {
        // Always calculate day/night for clock display
        int hour = Mathf.FloorToInt(simMinutesPassed / 60f) % 24;
        bool newNight = (hour >= 12);

        isNight = newNight;
        
        // Only apply night logic (like 2x2 capture) if nightCaptureEnabled is true
        if (DataPersistence.Instance.nightCaptureEnabled)
        {
            ShipInteractions.Instance.isNight = isNight;
        }
        else
        {
            ShipInteractions.Instance.isNight = false; // Disable night effects, but clock still shows Night
        }
    }

    private void UpdateTimeDisplays()
    {
        // Force update of remaining time and clock display at tick 0
        float remainingMinutes = simulationLengthHours * 60f - simMinutesPassed;
        if (remainingMinutes < 0) remainingMinutes = 0;
        int remainingDays = Mathf.FloorToInt(remainingMinutes / 1440f);
        int remainingHours = Mathf.FloorToInt((remainingMinutes % 1440) / 60f);
        int remainingMins = Mathf.FloorToInt(remainingMinutes % 60f);

        timeDisplayRemaining.text = $"Remaining: {remainingDays}d {remainingHours}h {remainingMins}m";

        // Also update the day/night clock text on top bar
        int totalMinutes = Mathf.FloorToInt(simMinutesPassed);
        int day = (totalMinutes / 1440) + 1;
        int hour = (totalMinutes / 60) % 24;
        int minute = totalMinutes % 60;
        string phase = isNight ? "Night" : "Day";
        timeDisplayRun.text = $"{phase} {day} — {hour:D2}:{minute:D2}";
    }

    // Accepts a simTime parameter (in seconds) used for replay recording.
    void SpawnShip(float simTime)
    {
        HashSet<Vector3> occupiedPositions = new HashSet<Vector3>();
        foreach (GameObject ship in allShips)
        {
            if (ship != null)
                occupiedPositions.Add(ship.transform.position);
        }

        // Cargo Day spawn.
        if (!isNight && Random.value <= cargoSpawnChance)
        {
            Vector3 spawnPos = GetUniqueSpawnPosition("Cargo", occupiedPositions);
            //Debug.Log(spawnPos);
            if (spawnPos != Vector3.zero)
            {
                GameObject cargo = Instantiate(cargoPrefab, spawnPos, GetSpawnRotation("Cargo"));
                cargo.name = $"Cargo({cargoCounter++})";
                cargo.tag = "Cargo";
                allShips.Add(cargo);
                textController.UpdateShipEnter("cargo");
                if (ReplayManager.Instance != null && !ReplayManager.Instance.ReplayModeActive)
                {
                    //Debug.Log($"[RECORD] Cargo({cargo}) spawned at tick {ShipController.TimeStepCounter}");                    
                }
            }
        }
        // Cargo Night spawn.
        if (isNight && Random.value <= cargoNightChance)
        {
            Vector3 spawnPos = GetUniqueSpawnPosition("Cargo", occupiedPositions);
            if (spawnPos != Vector3.zero)
            {
                string shipType = "Cargo";
                GameObject cargo = Instantiate(cargoPrefab, spawnPos, GetSpawnRotation("Cargo"));
                cargo.name = $"Cargo({cargoCounter++})";
                cargo.tag = "Cargo";
                allShips.Add(cargo);
                textController.UpdateShipEnter("cargo");
                if (ReplayManager.Instance != null && !ReplayManager.Instance.ReplayModeActive)
                {
                    Debug.Log($"[RECORD] Spawned {shipType}({cargo}) at {spawnPos} on tick {ShipController.TimeStepCounter}");
                }
            }
        }
        // Patrol spawn.
        if (!isNight && Random.value <= patrolSpawnChance)
        {
            Vector3 spawnPos = GetUniqueSpawnPosition("Patrol", occupiedPositions);
            if (spawnPos != Vector3.zero)
            {
                string shipType = "Patrol";
                GameObject patrol = Instantiate(patrolPrefab, spawnPos, GetSpawnRotation("Patrol"));
                patrol.name = $"Patrol({patrolCounter++})";
                patrol.tag = "Patrol";
                allShips.Add(patrol);
                textController.UpdateShipEnter("patrol");
                if (ReplayManager.Instance != null && !ReplayManager.Instance.ReplayModeActive)
                {
                    //Debug.Log($"[RECORD] Spawned {shipType}({patrol}) at {spawnPos} on tick {ShipController.TimeStepCounter}");
                }
            }
        }
        // Patrol Night spawn.
        if (isNight && Random.value <= patrolNightChance)
        {
            Vector3 spawnPos = GetUniqueSpawnPosition("Patrol", occupiedPositions);
            if (spawnPos != Vector3.zero)
            {
                string shipType = "Patrol";
                GameObject patrol = Instantiate(patrolPrefab, spawnPos, GetSpawnRotation("Patrol"));
                patrol.name = $"Patrol({patrolCounter++})";
                patrol.tag = "Patrol";
                allShips.Add(patrol);
                textController.UpdateShipEnter("patrol");
                if (ReplayManager.Instance != null && !ReplayManager.Instance.ReplayModeActive)
                {
                    //Debug.Log($"[RECORD] Spawned {shipType}({patrol}) at {spawnPos} on tick {ShipController.TimeStepCounter}");
                }
            }
        }

        // Pirate spawn.
        if (!isNight && Random.value <= pirateSpawnChance)
        {
            Vector3 spawnPos = GetUniqueSpawnPosition("Pirate", occupiedPositions);
            if (spawnPos != Vector3.zero)
            {
                string shipType = "Pirate";
                GameObject pirate = Instantiate(piratePrefab, spawnPos, GetSpawnRotation("Pirate"));
                pirate.name = $"Pirate({pirateCounter++})";
                pirate.tag = "Pirate";
                allShips.Add(pirate);
                textController.UpdateShipEnter("pirate");
                if (ReplayManager.Instance != null && !ReplayManager.Instance.ReplayModeActive)
                {
                    //Debug.Log($"[RECORD] Spawned {shipType}({pirate}) at {spawnPos} on tick {ShipController.TimeStepCounter}");
                }
            }
        }
        // Pirate Night spawn.
        if (isNight && Random.value <= pirateNightChance)
        {
            Vector3 spawnPos = GetUniqueSpawnPosition("Pirate", occupiedPositions);
            if (spawnPos != Vector3.zero)
            {
                string shipType = "Pirate";
                GameObject pirate = Instantiate(piratePrefab, spawnPos, GetSpawnRotation("Pirate"));
                pirate.name = $"Pirate({pirateCounter++})";
                pirate.tag = "Pirate";
                allShips.Add(pirate);
                textController.UpdateShipEnter("pirate");
                if (ReplayManager.Instance != null && !ReplayManager.Instance.ReplayModeActive)
                {
                    //Debug.Log($"[RECORD] Spawned {shipType}({pirate}) at {spawnPos} on tick {ShipController.TimeStepCounter}");
                }
            }
        }
    }

    Vector3 GetUniqueSpawnPosition(string shipType, HashSet<Vector3> occupiedPositions)
    {
        if (shipType == "Cargo")
        {
            int spawnZ = 0;

            if (!isNight) {
                spawnZ = SelectIndexByWeight(DataPersistence.Instance.cargoGridPercentsD);
                //Debug.Log("Cargo SpawnZ = " + spawnZ);
            }
            else
            {
                spawnZ = SelectIndexByWeight(DataPersistence.Instance.cargoGridPercentsN);
                //Debug.Log("Cargo SpawnZ = " + spawnZ);
            }

                return new Vector3(0, 0, spawnZ);
            }
        else if (shipType == "Pirate")
        {
            int spawnX = 0;

            if (!isNight)
            {
                spawnX = SelectIndexByWeight(DataPersistence.Instance.pirateGridPercentsD);
                //Debug.Log("Pirate SpawnX = " + spawnX);
            }
            else
            {
                spawnX = SelectIndexByWeight(DataPersistence.Instance.pirateGridPercentsN);
                //Debug.Log("Pirate SpawnX = " + spawnX);
            }

                return new Vector3(spawnX, 0, 0);
        }
        else if (shipType == "Patrol")
        {
            int spawnZ = 0;

            if (!isNight)
            {
                spawnZ = SelectIndexByWeight(DataPersistence.Instance.patrolGridPercentsD);
                //Debug.Log("Patrol SpawnZ = " + spawnZ);
            }
            else
            {
                spawnZ = SelectIndexByWeight(DataPersistence.Instance.patrolGridPercentsN);
                //Debug.Log("Patrol SpawnZ = " + spawnZ);
            }
                return new Vector3(gridSize.x - 1, 0, spawnZ);
        }
        return Vector3.zero;
    }

    Quaternion GetSpawnRotation(string shipType)
    {
        if (shipType == "Cargo")
            return Quaternion.Euler(0, 90, 0);
        else if (shipType == "Patrol")
            return Quaternion.Euler(0, -90, 0);
        else if (shipType == "Pirate")
            return Quaternion.Euler(0, 0, 0);
        return Quaternion.identity;
    }

    // For replay mode: spawn a ship and update UI counter.
    public GameObject ReplaySpawn(string shipType, Vector3 position, Quaternion rotation, string shipName, int shipId)
    {
        GameObject prefab = null;
        if (shipType == "Cargo")
            prefab = cargoPrefab;
        else if (shipType == "Patrol")
            prefab = patrolPrefab;
        else if (shipType == "Pirate")
            prefab = piratePrefab;

        if (prefab != null)
        {
            GameObject ship = Instantiate(prefab, position, rotation);
            ship.name = shipName;
            ship.tag = shipType;
            allShips.Add(ship);
            replayedShips[shipId] = ship;
            textController.UpdateShipEnter(shipType);
            return ship;
            //Debug.Log($"[ReplaySpawn] Spawned {shipType}({shipId}) at {position}");
        }
        return null;
    }

    public void ClearAllShips()
    {
        foreach (GameObject ship in allShips)
        {
            if (ship != null)
                Destroy(ship);
        }
        allShips.Clear();
    }

    // Public helper to update the simulation tick (used in replay mode).
    public static void SetTimeStepCounter(int newTick)
    {
        TimeStepCounter = newTick;
    }


}