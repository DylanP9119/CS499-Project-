using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;

public class ReplayManager : MonoBehaviour
{
    public static ReplayManager Instance { get; private set; }

    public Button playPauseButton;
    public Button btnIncreaseSpeed;
    public Button btnDecreaseSpeed;
    public Text timeDisplay;
    public ShipController shipController;
    public string replayFileName = "replay.json";
    public GameObject replayBoxUI;
    public TextController textController;
    
    public float simulationTickDuration = 1f; // Must match ShipController.tickDuration for simulation.
    
    // Use a shorter tick interval for replay ship movement.
    public float replayMovementTick = 0.5f;

    private List<ReplayEvent> recordedEvents = new List<ReplayEvent>();
    public float replayTime;  // Simulation time (seconds) in replay.
    private bool replayPaused;
    private float maxRecordedTime;
    private int currentShipId;

    // Pointer for processing replay events.
    private int nextEventIndex = 0;

    // Mapping from ship ID to spawned GameObject.
    private Dictionary<int, GameObject> replayedShips = new Dictionary<int, GameObject>();

    public bool ReplayModeActive { get; private set; }

    // Expose replayPaused state.
    public bool ReplayPaused => replayPaused;

    // Define discrete speeds: -1x, 1x, 2x, 10x, 20x.
    public readonly float[] speeds = { -1f, 1f, 2f, 10f, 20f };
    private int currentSpeedIndex = 1; // Start at 1x.
    public float replaySpeed { get; private set; } = 1f;

    // Track last movement tick for replay mode.
    private int lastMovementTick = -1;

    void Awake()
    {
        Instance = this;
        currentShipId = 1;
    }

    void Start()
    {
        if (playPauseButton != null)
            playPauseButton.onClick.AddListener(TogglePlayPause);
        if (btnIncreaseSpeed != null)
            btnIncreaseSpeed.onClick.AddListener(IncreaseSpeed);
        if (btnDecreaseSpeed != null)
            btnDecreaseSpeed.onClick.AddListener(DecreaseSpeed);

        UIvisibility(false);
    }

    void Update()
    {
        HandleReplayInput();

        if (ReplayModeActive)
        {
            // Even if replay is paused, update movement ticks.
            replayTime += replaySpeed * Time.unscaledDeltaTime;
            replayTime = Mathf.Clamp(replayTime, 0, maxRecordedTime);

            if (Mathf.Approximately(replayTime, maxRecordedTime))
                replayPaused = true;

            // Compute simulation tick for replay events.
            int currentTick = Mathf.FloorToInt(replayTime / simulationTickDuration);
            ShipController.SetTimeStepCounter(currentTick);

            // Use a separate tick for ship movement.
            int currentMovementTick = Mathf.FloorToInt(replayTime / replayMovementTick);
            if (currentMovementTick != lastMovementTick)
            {
                // Explicitly force movement on each ship in replay.
                foreach (GameObject ship in shipController.allShips)
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
                lastMovementTick = currentMovementTick;
            }

            UpdateDisplay();

            if (replaySpeed > 0)
            {
                while (nextEventIndex < recordedEvents.Count &&
                       replayTime >= recordedEvents[nextEventIndex].timestamp)
                {
                    ProcessEvent(recordedEvents[nextEventIndex]);
                    nextEventIndex++;
                }
            }
            else if (replaySpeed < 0)
            {
                while (nextEventIndex > 0 && replayTime < recordedEvents[nextEventIndex - 1].timestamp)
                {
                    nextEventIndex--;
                    UndoEvent(recordedEvents[nextEventIndex]);
                }
            }

            ShipInteractions.Instance.CheckForInteractions(shipController.allShips);
        }
    }

    void HandleReplayInput()
    {
        if (Input.GetKeyDown(KeyCode.R))
            SaveReplayToFile();
        if (Input.GetKeyDown(KeyCode.L))
            LoadReplayFromFile();
    }

    public int GetNextShipId() => currentShipId++;

    public void RecordShipSpawn(int shipId, string shipType, Vector3 position, Quaternion rotation, float simTimestamp)
    {
        var evt = new ReplayEvent(shipId, shipType, position, rotation, simTimestamp, "spawn");
        recordedEvents.Add(evt);
        if (evt.timestamp > maxRecordedTime)
            maxRecordedTime = evt.timestamp;
    }

    public void RecordCaptureEvent(int shipId, Vector3 position, Quaternion rotation, float simTimestamp)
    {
        var evt = new ReplayEvent(shipId, "Cargo", position, rotation, simTimestamp, "capture");
        recordedEvents.Add(evt);
        if (evt.timestamp > maxRecordedTime)
            maxRecordedTime = evt.timestamp;
    }

    public void RecordRescueEvent(int shipId, Vector3 position, Quaternion rotation, float simTimestamp)
    {
        var evt = new ReplayEvent(shipId, "Cargo", position, rotation, simTimestamp, "rescue");
        recordedEvents.Add(evt);
        if (evt.timestamp > maxRecordedTime)
            maxRecordedTime = evt.timestamp;
    }

    void ProcessEvent(ReplayEvent evt)
    {
        if (evt.eventType == "spawn")
        {
            shipController.ReplaySpawn(evt.shipType, evt.position, evt.rotation, $"{evt.shipType}({evt.shipId})", evt.shipId);
            GameObject spawned = shipController.allShips.LastOrDefault();
            if (spawned != null)
                replayedShips[evt.shipId] = spawned;
        }
        else if (evt.eventType == "capture")
        {
            if (replayedShips.TryGetValue(evt.shipId, out GameObject ship))
            {
                CargoBehavior cargo = ship.GetComponent<CargoBehavior>();
                if (cargo != null)
                {
                    cargo.isCaptured = true;
                    textController.UpdateCaptures(true);
                    Debug.Log($"[Replay] Capture event: Cargo ship ID {evt.shipId} marked as captured.");
                }
                PirateBehavior pirate = ship.GetComponent<PirateBehavior>();
                if (pirate != null)
                {
                    pirate.hasCargo = true;
                    Debug.Log($"[Replay] Capture event: Pirate ship ID {evt.shipId} marked as having cargo.");
                }
            }
        }
        else if (evt.eventType == "rescue")
        {
            if (replayedShips.TryGetValue(evt.shipId, out GameObject ship))
            {
                CargoBehavior cargo = ship.GetComponent<CargoBehavior>();
                if (cargo != null)
                {
                    cargo.isCaptured = false;
                    textController.UpdateCaptures(false);
                    Debug.Log($"[Replay] Rescue event: Cargo ship ID {evt.shipId} marked as rescued.");
                }
            }
        }
    }

    void UndoEvent(ReplayEvent evt)
    {
        if (evt.eventType == "spawn")
        {
            if (replayedShips.TryGetValue(evt.shipId, out GameObject ship))
            {
                shipController.allShips.Remove(ship);
                Destroy(ship);
                replayedShips.Remove(evt.shipId);
            }
        }
        else if (evt.eventType == "capture")
        {
            if (replayedShips.TryGetValue(evt.shipId, out GameObject ship))
            {
                CargoBehavior cargo = ship.GetComponent<CargoBehavior>();
                if (cargo != null)
                {
                    cargo.isCaptured = false;
                    textController.UndoCapture();
                    Debug.Log($"[Replay] Undo Capture: Cargo ship ID {evt.shipId} marked as not captured.");
                }
                PirateBehavior pirate = ship.GetComponent<PirateBehavior>();
                if (pirate != null)
                    pirate.hasCargo = false;
            }
        }
        else if (evt.eventType == "rescue")
        {
            if (replayedShips.TryGetValue(evt.shipId, out GameObject ship))
            {
                CargoBehavior cargo = ship.GetComponent<CargoBehavior>();
                if (cargo != null)
                {
                    cargo.isCaptured = true;
                    textController.UndoRescue();
                    Debug.Log($"[Replay] Undo Rescue: Cargo ship ID {evt.shipId} marked as captured again.");
                }
            }
        }
    }

    void StartReplay()
    {
        ReplayModeActive = true;
        UIvisibility(true);
        replayPaused = false; // Start replay unpaused.
        replayTime = 0;
        nextEventIndex = 0;
        textController.ResetCounters();
        shipController.ClearAllShips();
        recordedEvents = recordedEvents.OrderBy(e => e.timestamp).ToList();
        maxRecordedTime = recordedEvents.Count > 0 ? recordedEvents.Max(e => e.timestamp) : 0;
        lastMovementTick = -1;
        UpdateDisplay();
    }

    void StopReplay()
    {
        ReplayModeActive = false;
        UIvisibility(false);
        shipController.ClearAllShips();
    }

    public void SaveReplayToFile()
    {
        if (recordedEvents.Count == 0)
        {
            Debug.LogWarning("No events to save!");
            return;
        }
        var data = new ReplayData { events = recordedEvents };
        string json = JsonUtility.ToJson(data, true);
        string path = Path.Combine(Application.persistentDataPath, replayFileName);
        File.WriteAllText(path, json);
        Debug.Log($"Saved {recordedEvents.Count} events to: {path}");
    }

    public void LoadReplayFromFile()
    {
        string path = Path.Combine(Application.persistentDataPath, replayFileName);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            ReplayData data = JsonUtility.FromJson<ReplayData>(json);
            recordedEvents = data?.events ?? new List<ReplayEvent>();
            Debug.Log($"Loaded {recordedEvents.Count} events");
            StartReplay();
        }
        else
        {
            Debug.LogWarning("No replay file found.");
        }
    }

    void UpdateDisplay()
    {
        float timeLeft = maxRecordedTime - replayTime;
        timeDisplay.text = $"{timeLeft:0.0}s remaining  [Speed: {replaySpeed}x]";
    }

    void TogglePlayPause() => replayPaused = !replayPaused;

    void IncreaseSpeed()
    {
        if (currentSpeedIndex < speeds.Length - 1)
        {
            currentSpeedIndex++;
            replaySpeed = speeds[currentSpeedIndex];
            Debug.Log($"Replay speed set to {replaySpeed}x");
            UpdateDisplay();
        }
    }

    void DecreaseSpeed()
    {
        if (currentSpeedIndex > 0)
        {
            currentSpeedIndex--;
            replaySpeed = speeds[currentSpeedIndex];
            Debug.Log($"Replay speed set to {replaySpeed}x");
            UpdateDisplay();
        }
    }

    public void UIvisibility(bool visible) => replayBoxUI.SetActive(visible);
}

[System.Serializable]
public struct ReplayEvent
{
    public int shipId;
    public string shipType;
    public Vector3 position;
    public Quaternion rotation;
    public float timestamp;
    public string eventType;

    public ReplayEvent(int id, string type, Vector3 pos, Quaternion rot, float time, string evtType)
    {
        shipId = id;
        shipType = type;
        position = pos;
        rotation = rot;
        timestamp = time;
        eventType = evtType;
    }
}

[System.Serializable]
public class ReplayData
{
    public List<ReplayEvent> events;
}
