using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.IO;

public class ReplayManager : MonoBehaviour
{
    public static ReplayManager Instance;

    public List<ReplayEvent> recordedEvents = new(); // All events go here
    public bool ReplayModeActive = false;
    public bool ReplayPaused = true;
    public float replayTime = 0f;
    public float replaySpeed = 1f;
    private int lastProcessedTick = -1;

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
        Debug.Log($"[ReplayManager.Update] Running. Mode: {ReplayModeActive}, Paused: {ReplayPaused}");

        if (ReplayModeActive && !ReplayPaused && TimeControl.Instance != null)
        {
            Debug.Log("[Tick Block Entered] Incrementing replayTime...");
            replayTime += Time.deltaTime * replaySpeed;
            int tick = Mathf.FloorToInt(replayTime / TimeControl.Instance.GetSpeed());
            Debug.Log($"[TICK TEST] replayTime = {replayTime}");
            if (tick != lastProcessedTick)
            {
                Debug.Log($"[Replay Tick] Stepping from {lastProcessedTick} to {tick}");
                lastProcessedTick = tick;
                StepToTick(tick);
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
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

            replayTime = TimeControl.Instance != null ? TimeControl.Instance.GetSpeed() : 1f;

            ReplayPaused = false;
            StepToTick(1);
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
    }


    private void Start()
    {
        //shipController = FindObjectOfType<ShipController>();
    }

    public int GetNextShipId()
    {
        return recordedEvents.Count + 1; // You can replace this with a counter if needed
    }

    public void RecordShipSpawn(int shipId, string shipType, Vector3 position, Quaternion rotation, float simTimestamp)
    {
        var evt = new ReplayEvent(shipId, shipType, position, rotation, simTimestamp, "spawn");
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
            recordedEvents = data.events;
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

        lastProcessedTick = targetTick;

        // Clear all ships
        ShipController.Instance.ClearAllShips();

        // Replay all events up to the target tick
        foreach (ReplayEvent evt in recordedEvents)
        {
            if (evt.timestamp > targetTick)
                break;

            // ✅ Process spawn/capture/etc. only once
            switch (evt.eventType)
            {
                case "spawn":
                    ShipController.Instance.ReplaySpawn(
                        evt.shipType,          // "Cargo", "Pirate", "Patrol"
                        evt.position,          // Vector3 position
                        evt.rotation,          // Quaternion rotation
                        $"{evt.shipType}({evt.shipId})", // Name like "Cargo(1)"
                        evt.shipId             // int shipId
                    );
                    break;
                case "capture":
                ReplayCapture(evt); break;
                case "rescue":
                    ReplayRescue(evt); break;
                case "defeat":
                    ReplayDefeat(evt); break;
                case "evasion":
                    ReplayEvasion(evt); break;
            }
        }

        // Set simulation time to match
        ShipController.SetTimeStepCounter(targetTick);
        ShipController.Instance.UpdateDayNightCycle();

        // Apply movement steps so ships are in the correct position
        foreach (GameObject ship in ShipController.Instance.allShips)
        {
            ApplySteps(ship, targetTick);
        }

        ShipInteractions.Instance.CheckForInteractions(ShipController.Instance.allShips);

        Debug.Log($"[REPLAY] Stepping to tick {targetTick} with {recordedEvents.Count} events");

    }

    private void ApplySteps(GameObject ship, int targetTick)
    {
        if (ship.CompareTag("Cargo"))
        {
            var cargo = ship.GetComponent<CargoBehavior>();
            for (int i = 0; i < targetTick; i++) cargo.Step(true);
        }
        else if (ship.CompareTag("Patrol"))
        {
            var patrol = ship.GetComponent<PatrolBehavior>();
            for (int i = 0; i < targetTick; i++) patrol.Step(true);
        }
        else if (ship.CompareTag("Pirate"))
        {
            var pirate = ship.GetComponent<PirateBehavior>();
            for (int i = 0; i < targetTick; i++) pirate.Step(true);
        }
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

    public ReplayEvent(int shipId, string shipType, Vector3 pos, Quaternion rot, float time, string eventType, int pirateId = -1)
    {
        this.shipId = shipId;
        this.shipType = shipType;
        this.position = pos;
        this.rotation = rot;
        this.timestamp = time;
        this.eventType = eventType;
        this.pirateId = pirateId;
    }
}

[System.Serializable]
public class ReplayData
{
    public List<ReplayEvent> events = new();
}

