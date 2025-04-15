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
    public bool useDayNightCycle = true;
    Vector2Int gridSize = new Vector2Int(400, 100);
    public List<GameObject> allShips = new List<GameObject>();

    void Start()
    {
        timeControl = FindObjectOfType<TimeControl>();
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
                simMinutesPassed += 1f;
                UpdateDayNightCycle();
                
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
                        if(cargo != null)
                            cargo.Step(true);
                    }
                    else if (ship.CompareTag("Patrol"))
                    {
                        var patrol = ship.GetComponent<PatrolBehavior>();
                        if(patrol != null)
                            patrol.Step(true);
                    }
                    else if (ship.CompareTag("Pirate"))
                    {
                        var pirate = ship.GetComponent<PirateBehavior>();
                        if(pirate != null)
                            pirate.Step(true);
                    }
                }
                ShipInteractions.Instance.CheckForInteractions(allShips);
                spawnTimer = 0f;
            }
        }
    }

    void UpdateDayNightCycle()
    {
        if (!useDayNightCycle)
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
        }
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

        // Cargo spawn.
        if (!isNight && Random.value <= cargoSpawnChance)
        {
            Vector3 spawnPos = GetUniqueSpawnPosition("Cargo", occupiedPositions);
            if (spawnPos != Vector3.zero)
            {
                int shipId = ReplayManager.Instance != null ? ReplayManager.Instance.GetNextShipId() : cargoCounter;
                string shipType = "Cargo";
                GameObject cargo = Instantiate(cargoPrefab, spawnPos, GetSpawnRotation("Cargo"));
                cargo.name = $"Cargo({cargoCounter++})";
                cargo.tag = "Cargo";
                allShips.Add(cargo);
                textController.UpdateShipEnter("cargo");
                if (ReplayManager.Instance != null)
                    ReplayManager.Instance.RecordShipSpawn(shipId, shipType, spawnPos, cargo.transform.rotation, simTime);
            }
        }
        // Patrol spawn.
        if (!isNight && Random.value <= patrolSpawnChance)
        {
            Vector3 spawnPos = GetUniqueSpawnPosition("Patrol", occupiedPositions);
            if (spawnPos != Vector3.zero)
            {
                int shipId = ReplayManager.Instance != null ? ReplayManager.Instance.GetNextShipId() : patrolCounter;
                string shipType = "Patrol";
                GameObject patrol = Instantiate(patrolPrefab, spawnPos, GetSpawnRotation("Patrol"));
                patrol.name = $"Patrol({patrolCounter++})";
                patrol.tag = "Patrol";
                allShips.Add(patrol);
                textController.UpdateShipEnter("patrol");
                if (ReplayManager.Instance != null)
                    ReplayManager.Instance.RecordShipSpawn(shipId, shipType, spawnPos, patrol.transform.rotation, simTime);
            }
        }
        // Pirate spawn.
        if (!isNight && Random.value <= pirateSpawnChance)
        {
            Vector3 spawnPos = GetUniqueSpawnPosition("Pirate", occupiedPositions);
            if (spawnPos != Vector3.zero)
            {
                int shipId = ReplayManager.Instance != null ? ReplayManager.Instance.GetNextShipId() : pirateCounter;
                string shipType = "Pirate";
                GameObject pirate = Instantiate(piratePrefab, spawnPos, GetSpawnRotation("Pirate"));
                pirate.name = $"Pirate({pirateCounter++})";
                pirate.tag = "Pirate";
                allShips.Add(pirate);
                textController.UpdateShipEnter("pirate");
                if (ReplayManager.Instance != null)
                    ReplayManager.Instance.RecordShipSpawn(shipId, shipType, spawnPos, pirate.transform.rotation, simTime);
            }
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
    public void ReplaySpawn(string shipType, Vector3 position, Quaternion rotation, string shipName, int shipId)
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
            textController.UpdateShipEnter(shipType);
        }
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
