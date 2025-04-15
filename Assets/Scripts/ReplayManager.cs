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

    private List<ReplayEvent> recordedEvents = new List<ReplayEvent>();
    public float replayTime;  // made public to allow ShipController access.
    private bool replayPaused;
    private float maxRecordedTime;
    private int currentShipId;

    // Next event pointer – events are processed in order as replayTime increases.
    private int nextEventIndex = 0;

    // Mapping from ship ID to the spawned GameObject.
    private Dictionary<int, GameObject> replayedShips = new Dictionary<int, GameObject>();

    public bool ReplayModeActive { get; private set; }

    // Define discrete speeds: -1x, 1x, 2x, 10x, 20x.
    public readonly float[] speeds = { -1f, 1f, 2f, 10f, 20f };
    private int currentSpeedIndex = 1; // Start at 1x.
    public float replaySpeed { get; private set; } = 1f;

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
            if (!replayPaused)
            {
                // Advance replay time.
                replayTime += replaySpeed * Time.unscaledDeltaTime;
                // Clamp replayTime between 0 and maxRecordedTime.
                replayTime = Mathf.Clamp(replayTime, 0, maxRecordedTime);

                // When at the end, automatically pause replay.
                if (Mathf.Approximately(replayTime, maxRecordedTime))
                {
                    replayPaused = true;
                }

                // Update simulation tick each update.
                int currentTick = Mathf.FloorToInt(replayTime);
                ShipController.SetTimeStepCounter(currentTick);

                UpdateDisplay();

                // Process forward events when replaySpeed is positive.
                if (replaySpeed > 0)
                {
                    while (nextEventIndex < recordedEvents.Count &&
                           replayTime >= recordedEvents[nextEventIndex].timestamp)
                    {
                        ProcessEvent(recordedEvents[nextEventIndex]);
                        nextEventIndex++;
                    }
                }
                // Process reverse events when replaySpeed is negative.
                else if (replaySpeed < 0)
                {
                    while (nextEventIndex > 0 && replayTime < recordedEvents[nextEventIndex - 1].timestamp)
                    {
                        nextEventIndex--;
                        UndoEvent(recordedEvents[nextEventIndex]);
                    }
                }
            }

            // Update movement of already-spawned ships only if replay is not paused.
            if (!replayPaused)
            {
                foreach (GameObject ship in shipController.allShips)
                {
                    if (ship != null)
                        ship.SendMessage("Step", SendMessageOptions.DontRequireReceiver);
                }
            }
            // Process interactions (including moving captured pairs).
            if (!replayPaused)
            {
                ShipInteractions.Instance.CheckForInteractions(shipController.allShips);
            }
        }
    }

    void HandleReplayInput()
    {
        if (Input.GetKeyDown(KeyCode.R))
            SaveReplayToFile();
        if (Input.GetKeyDown(KeyCode.L))
            LoadReplayFromFile();
        if (Input.GetKeyDown(KeyCode.S))
            ToggleReplay();
    }

    public int GetNextShipId() => currentShipId++;

    public void RecordShipSpawn(int shipId, string shipType, Vector3 position, Quaternion rotation)
    {
        var evt = new ReplayEvent(shipId, shipType, position, rotation, Time.time, "spawn");
        recordedEvents.Add(evt);
        if (evt.timestamp > maxRecordedTime)
            maxRecordedTime = evt.timestamp;
    }

    public void RecordCaptureEvent(int shipId, Vector3 position, Quaternion rotation)
    {
        var evt = new ReplayEvent(shipId, "Cargo", position, rotation, Time.time, "capture");
        recordedEvents.Add(evt);
        if (evt.timestamp > maxRecordedTime)
            maxRecordedTime = evt.timestamp;
    }

    public void RecordRescueEvent(int shipId, Vector3 position, Quaternion rotation)
    {
        var evt = new ReplayEvent(shipId, "Cargo", position, rotation, Time.time, "rescue");
        recordedEvents.Add(evt);
        if (evt.timestamp > maxRecordedTime)
            maxRecordedTime = evt.timestamp;
    }

    void ProcessEvent(ReplayEvent evt)
    {
        if (evt.eventType == "spawn")
        {
            // Spawn the ship at its recorded time.
            shipController.ReplaySpawn(evt.shipType, evt.position, evt.rotation, $"{evt.shipType}({evt.shipId})", evt.shipId);
            GameObject spawned = shipController.allShips.LastOrDefault();
            if (spawned != null)
            {
                replayedShips[evt.shipId] = spawned;
            }
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

    // New method to undo events when reversing time.
    void UndoEvent(ReplayEvent evt)
    {
        if (evt.eventType == "spawn")
        {
            // Remove the spawned ship if it exists.
            if (replayedShips.TryGetValue(evt.shipId, out GameObject ship))
            {
                shipController.allShips.Remove(ship);
                Destroy(ship);
                replayedShips.Remove(evt.shipId);
            }
        }
        else if (evt.eventType == "capture")
        {
            // Undo capture: mark cargo as not captured and update UI, and pirate no longer has cargo.
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
            // Undo rescue: mark cargo as captured again and update UI.
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

    public void ToggleReplay()
    {
        if (!ReplayModeActive)
            StartReplay();
        else
            StopReplay();
    }

    void StartReplay()
    {
        ReplayModeActive = true;
        UIvisibility(true);
        replayPaused = true; // Start paused so you can inspect if necessary.
        replayTime = 0;
        nextEventIndex = 0;
        textController.ResetCounters();
        shipController.ClearAllShips();
        recordedEvents = recordedEvents.OrderBy(e => e.timestamp).ToList();
        maxRecordedTime = recordedEvents.Count > 0 ? recordedEvents.Max(e => e.timestamp) : 0;
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
        // Show remaining time.
        float timeLeft = maxRecordedTime - replayTime;
        timeDisplay.text = $"{timeLeft:0.0}s remaining  [Speed: {replaySpeed}x]";
    }

    void TogglePlayPause() => replayPaused = !replayPaused;

    // Discrete speed controls.
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

    // Expose replay pause state for other systems.
    public bool ReplayPaused => replayPaused;
}

[System.Serializable]
public class ReplayData
{
    public List<ReplayEvent> events;
}

[System.Serializable]
public struct ReplayEvent
{
    public int shipId;
    public string shipType;
    public Vector3 position;
    public Quaternion rotation;
    public float timestamp;
    public string eventType;  // "spawn", "capture", or "rescue"

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
