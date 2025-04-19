using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;

public class ReplayManager : MonoBehaviour
{
    public static ReplayManager Instance;

    public List<ReplayEvent> recordedEvents = new(); // All events go here
    public bool ReplayModeActive = false;
    public bool ReplayPaused = true;
    public float replayTime = 0f;
    public float replaySpeed = 1f;
    private int lastProcessedTick = -1;
    public float replayMovementTick = 0.5f;
    private int lastMovementTick = -1;

    //private ShipController shipController;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    void Update()
    {
        //Debug.Log($"[ReplayManager.Update] Running. Mode: {ReplayModeActive}, Paused: {ReplayPaused}");

        if (ReplayModeActive && !ReplayPaused && TimeControl.Instance != null)
        {
            //Debug.Log("[Tick Block Entered] Incrementing replayTime...");
            replayTime += Time.deltaTime * replaySpeed;
            int tick = Mathf.FloorToInt(replayTime / TimeControl.Instance.GetSpeed());
            //Debug.Log($"[TICK TEST] replayTime = {replayTime}");
            if (tick != lastProcessedTick)
            {
                Debug.Log($"[Replay Tick] Stepping from {lastProcessedTick} to {tick}");
                lastProcessedTick = tick;
                StepToTick(tick);
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            TimeControl.Instance.ToggleMovement(true); 
            SaveReplayToFile("replay.json");
            Debug.Log("[Keybind] Saved replay to replay.json");

            //Instance.replayTime = 0f;
            //ReplayPaused = false; // allow time to advance
            //StepToTick(0);
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadReplayFromFile("replay.json");
            Debug.Log("[Keybind] Loaded replay from replay.json");

            ReplayModeActive = true;
            ReplayPaused = false;
            StepToTick(0);
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            if (!ReplayModeActive || ReplayPaused || TimeControl.Instance == null)
            return;

            replayTime -= TimeControl.Instance.GetSpeed();
            if (replayTime < 0) replayTime = 0f;

            int newTick = Mathf.FloorToInt(replayTime / TimeControl.Instance.GetSpeed());
            StepToTick(newTick);

            Debug.Log($"[Keybind] Rewound to tick {newTick}");
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ReplayPaused = !ReplayPaused;
            Debug.Log($"[Keybind] ReplayPaused = {ReplayPaused}");
        }

        int movementTick = Mathf.FloorToInt(replayTime / replayMovementTick);
        if (movementTick != lastMovementTick)
        {
            foreach (GameObject ship in ShipController.Instance.allShips)
            {
                if (ship == null) continue;

                if (ship.CompareTag("Cargo"))
                    ship.GetComponent<CargoBehavior>()?.Step(true);
                else if (ship.CompareTag("Patrol"))
                    ship.GetComponent<PatrolBehavior>()?.Step(true);
                else if (ship.CompareTag("Pirate"))
                    ship.GetComponent<PirateBehavior>()?.Step(true);
            }

            lastMovementTick = movementTick;
        }

    }


    private void Start()
    {
        //shipController = FindObjectOfType<ShipController>();
    }

    public int GetNextShipId()
    {
        return recordedEvents.Count + 1; // You can replace this with a counter if needed
    }

    public void RecordShipSpawn(int shipId, string shipType, Vector3 position, Quaternion rotation, float simTimestamp, int spawnTick)
    {
        var evt = new ReplayEvent(shipId, shipType, position, rotation, simTimestamp, "spawn", -1, spawnTick);
        recordedEvents.Add(evt);
    }

    public void RecordCaptureEvent(int shipId, Vector3 position, Quaternion rotation, float simTimestamp, int pirateId)
    {
        var evt = new ReplayEvent(shipId, "Cargo", position, rotation, simTimestamp, "capture", pirateId);
        recordedEvents.Add(evt);
    }

    public void RecordRescueEvent(int shipId, Vector3 position, Quaternion rotation, float simTimestamp)
    {
        var evt = new ReplayEvent(shipId, "Cargo", position, rotation, simTimestamp, "rescue");
        recordedEvents.Add(evt);
    }

    public void RecordDefeatEvent(int pirateId, Vector3 position, Quaternion rotation, float simTimestamp)
    {
        var evt = new ReplayEvent(pirateId, "Pirate", position, rotation, simTimestamp, "defeat");
        recordedEvents.Add(evt);
    }

    public void RecordEvasionEvent(int shipId, Vector3 position, Quaternion rotation, float simTimestamp, int pirateId)
    {
        var evt = new ReplayEvent(shipId, "Cargo", position, rotation, simTimestamp, "evasion", pirateId);
        recordedEvents.Add(evt);
    }

    public void SaveReplayToFile(string filename)
    {
        string path = Path.Combine(Application.persistentDataPath, filename);
        ReplayData data = new() { events = recordedEvents };
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
        Debug.Log($"[Replay Saved] {path}");
    }

    public void LoadReplayFromFile(string filename)
    {
        string path = Path.Combine(Application.persistentDataPath, filename);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            ReplayData data = JsonUtility.FromJson<ReplayData>(json);
            recordedEvents = data.events.OrderBy(e => e.timestamp).ToList();
            ReplayModeActive = true;
            ReplayPaused = true;
            replayTime = TimeControl.Instance != null ? TimeControl.Instance.GetSpeed() : 1f;
            lastProcessedTick = -1;
            StepToTick(1);
            Debug.Log($"[Replay Loaded] {recordedEvents.Count} events");
        }
        else
        {
            Debug.LogWarning($"Replay file not found at: {path}");
        }

        if (recordedEvents.Count > 0)
        {
            Debug.Log($"[LOAD CHECK] First eventType = \"{recordedEvents[0].eventType}\"");
            Debug.Log($"Full JSON: {JsonUtility.ToJson(recordedEvents[0], true)}");
        }
        else
        {
            Debug.LogWarning("[LOAD CHECK] No events were loaded into recordedEvents.");
        }
    }
    
    private IEnumerator DelayedStartReplay()
    {
        yield return null; // wait one frame
        StepToTick(0);
    }

    public void StepToTick(int targetTick)
    {
        
        if (targetTick == lastProcessedTick)
        return;

        // Clear all ships
        ShipController.Instance.ClearAllShips();
        ShipInteractions.Instance.ResetState();

        // Replay all events up to the target tick
        foreach (ReplayEvent evt in recordedEvents
        .Where(e => e.timestamp <= targetTick)
        .OrderBy(e => e.timestamp))
        {
            switch (evt.eventType)
            {
                case "spawn":
                    HandleSpawnEvent(evt, targetTick);
                    break;
                case "capture":
                    HandleCaptureEvent(evt, targetTick);
                    break;
                case "rescue":
                    HandleRescueEvent(evt, targetTick);
                    break;
                //case "defeat":                                WORK ON LATER
                //    HandleDefeatEvent(evt, targetTick);
                //    break;
                //case "evasion":
                //    HandleEvasionEvent(evt, targetTick);
                //    break;
            }
        }

        // Set simulation time to match
        ShipController.SetTimeStepCounter(targetTick);
        ShipController.Instance.UpdateDayNightCycle();

        ShipInteractions.Instance.CheckForInteractions(ShipController.Instance.allShips);

        Debug.Log($"[REPLAY] Stepping to tick {targetTick} with {recordedEvents.Count} events");

    }

    private void HandleSpawnEvent(ReplayEvent evt, int targetTick)
    {
        // Spawn ship
        GameObject ship = ShipController.Instance.ReplaySpawn(
            evt.shipType,
            evt.position,
            evt.rotation,
            $"{evt.shipType}({evt.shipId})",
            evt.shipId
        );

        if (ship != null)
        {
            int steps = targetTick - evt.spawnTick;
            ApplySteps(ship, steps);

            Debug.Log($"[REPLAY] Spawned {evt.shipType} ID {evt.shipId} at tick {evt.spawnTick}. Applied {steps} steps.");
        }
    }

    private void HandleCaptureEvent(ReplayEvent evt, int targetTick)
    {
        GameObject pirate = FindShip("Pirate", evt.pirateId);
        GameObject cargo = FindShip("Cargo", evt.shipId);

        if (pirate != null && cargo != null)
        {
            // Restore capture relationship
            ShipInteractions.Instance.ReviveCapturePair(pirate, cargo);
            
            // Apply movement steps since capture
            int steps = targetTick - Mathf.FloorToInt(evt.timestamp); 
            ApplySteps(pirate, steps);
            ApplySteps(cargo, steps);
        }
    }

    private void HandleRescueEvent(ReplayEvent evt, int targetTick)
    {
        GameObject cargo = FindShip("Cargo", evt.shipId);
        if (cargo != null)
        {
            // Find and remove associated pirate
            GameObject pirate = ShipInteractions.Instance.GetPirateForCargo(cargo);
            
            if (pirate != null)
            {
                ShipInteractions.Instance.UnmarkCapturePair(pirate, cargo);
                Destroy(pirate);
            }

            // Apply movement steps since rescue
            int steps = targetTick - Mathf.FloorToInt(evt.timestamp); 
            ApplySteps(cargo, steps);
        }
    }

    private GameObject FindShip(string type, int shipId)
    {
        return ShipController.Instance.allShips
            .FirstOrDefault(s => s != null && 
                                s.CompareTag(type) && 
                                ExtractShipId(s) == shipId);
    }

    private int ExtractShipId(GameObject ship)
    {
        if (ship == null || string.IsNullOrEmpty(ship.name)) 
        return 0;

        try
        {
            // Ship names are formatted like "Cargo(123)" or "Pirate(456)"
            int start = ship.name.IndexOf('(');
            int end = ship.name.IndexOf(')');
            
            if (start >= 0 && end > start)
            {
                string idString = ship.name.Substring(start + 1, end - start - 1);
                return int.Parse(idString);
            }
        }
        catch (System.Exception)
        {
            Debug.LogWarning($"Failed to parse ID from ship name: {ship.name}");
        }
        
        return 0; // Fallback for invalid format
    }

    private void ApplySteps(GameObject ship, int steps)
    {
        if (ship.CompareTag("Cargo"))
        {
            var cargo = ship.GetComponent<CargoBehavior>();
            for (int i = 0; i < steps; i++) cargo.Step(true);
        }
        else if (ship.CompareTag("Patrol"))
        {
            var patrol = ship.GetComponent<PatrolBehavior>();
            for (int i = 0; i < steps; i++) patrol.Step(true);
        }
        else if (ship.CompareTag("Pirate"))
        {
            var pirate = ship.GetComponent<PirateBehavior>();
            for (int i = 0; i < steps; i++) pirate.Step(true);
        }
        Debug.Log($"[STEPS] Applied {steps} steps to {ship.name}");
    }

    private void ReplayCapture(ReplayEvent evt)
    {
        Debug.Log($"[ReplayCapture] Ship {evt.shipId} captured by Pirate {evt.pirateId}");
        // We’ll implement this after spawning works!
    }

    private void ReplayRescue(ReplayEvent evt)
    {
        Debug.Log($"[ReplayRescue] Cargo {evt.shipId} was rescued");
    }

    private void ReplayDefeat(ReplayEvent evt)
    {
        Debug.Log($"[ReplayDefeat] Pirate {evt.shipId} was defeated");
    }

    private void ReplayEvasion(ReplayEvent evt)
    {
        Debug.Log($"[ReplayEvasion] Cargo {evt.shipId} evaded Pirate {evt.pirateId}");
    }
}

[System.Serializable]
public class ReplayEvent
{
    public string eventType; // spawn, capture, etc.
    public int shipId;
    public string shipType;
    public int pirateId; // only for capture/evasion
    public Vector3 position;
    public Quaternion rotation;
    public float timestamp;
    public int spawnTick;

    public ReplayEvent(int shipId, string shipType, Vector3 pos, Quaternion rot, float time, string eventType, int pirateId = -1, int spawnTick = 0)
    {
        this.shipId = shipId;
        this.shipType = shipType;
        this.position = pos;
        this.rotation = rot;
        this.timestamp = time;
        this.eventType = eventType;
        this.pirateId = pirateId;
        this.spawnTick = spawnTick;
    }
}

[System.Serializable]
public class ReplayData
{
    public List<ReplayEvent> events = new();
}

