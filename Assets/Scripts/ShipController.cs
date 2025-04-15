using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class ShipController : MonoBehaviour
{
    // Static simulation tick counter.
    public static int TimeStepCounter { get; private set; } = 0;

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
    private TimeControl isPaused;
    private int cargoCounter = 1, patrolCounter = 1, pirateCounter = 1;
    public float simulationLengthHours = 24f;
    private float simMinutesPassed = 0f;
    public bool useDayNightCycle = true;
    Vector2Int gridSize = new Vector2Int(400, 100);
    public List<GameObject> allShips = new List<GameObject>();

    void Start()
    {
        isPaused = FindObjectOfType<TimeControl>();
    }

    void Update()
    {
        // Run simulation only if not in replay mode.
        if (ReplayManager.Instance != null && ReplayManager.Instance.ReplayModeActive)
            return;

        if (isPaused.ShouldMove())
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= 1f)
            {
                TimeStepCounter++; // increment simulation tick each second
                simMinutesPassed += 5f;
                UpdateDayNightCycle();
                SpawnShip();
                spawnTimer = 0f;
            }
        }

        // Update ship movements.
        foreach (GameObject ship in allShips)
        {
            if (ship != null)
                ship.SendMessage("Step", SendMessageOptions.DontRequireReceiver);
        }

        // Check interactions in normal simulation.
        ShipInteractions.Instance.CheckForInteractions(allShips);
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

    void SpawnShip()
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
                    ReplayManager.Instance.RecordShipSpawn(shipId, shipType, spawnPos, cargo.transform.rotation);
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
                    ReplayManager.Instance.RecordShipSpawn(shipId, shipType, spawnPos, patrol.transform.rotation);
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
                    ReplayManager.Instance.RecordShipSpawn(shipId, shipType, spawnPos, pirate.transform.rotation);
            }
        }
    }

    Vector3 GetUniqueSpawnPosition(string shipType, HashSet<Vector3> occupiedPositions)
    {
        float roll = Random.value;
        Vector3 spawnPos = Vector3.zero;
        int maxAttempts = 400;
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
            else return Vector3.zero;
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
            // Update UI counter for replay spawn.
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

    // Public helper to update the tick (used in replay mode).
    public static void SetTimeStepCounter(int newTick)
    {
        TimeStepCounter = newTick;
    }
}
