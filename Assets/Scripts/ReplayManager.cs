using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;

public class ReplayManager : MonoBehaviour
{
    public static ReplayManager Instance { get; private set; }
    public Button playPauseButton, btnIncreaseSpeed, btnDecreaseSpeed;
    public Text timeDisplay;
    public ShipController shipController;
    public string replayFileName = "replay.json";
    public GameObject replayBoxUI;
    public TextController textController;
    public float simulationTickDuration = 1f;
    public float replayMovementTick = 0.5f;
    private List<ReplayEvent> recordedEvents = new List<ReplayEvent>();
    public float replayTime;
    private bool replayPaused;
    private float maxRecordedTime;
    private int currentShipId = 1;
    private int nextEventIndex = 0;
    private Dictionary<int, GameObject> replayedShips = new Dictionary<int, GameObject>();
    public bool ReplayModeActive { get; private set; }
    public bool ReplayPaused => replayPaused;
    public readonly float[] speeds = { -1f, 1f, 2f, 10f, 20f };
    private int currentSpeedIndex = 1;
    public float replaySpeed { get; private set; } = 1f;
    private int lastMovementTick = -1;

    void Awake() => Instance = this;

    void Start()
    {
        playPauseButton?.onClick.AddListener(TogglePlayPause);
        btnIncreaseSpeed?.onClick.AddListener(IncreaseSpeed);
        btnDecreaseSpeed?.onClick.AddListener(DecreaseSpeed);
        UIvisibility(true);
    }

    void Update()
    {
        HandleReplayInput();
        if (!ReplayModeActive || replayPaused) return;

        replayTime += replaySpeed * Time.unscaledDeltaTime;
        replayTime = Mathf.Clamp(replayTime, 0, maxRecordedTime);

        if (Mathf.Approximately(replayTime, maxRecordedTime)) replayPaused = true;

        ShipController.SetTimeStepCounter(Mathf.FloorToInt(replayTime / simulationTickDuration));
        UpdateMovement();
        UpdateDisplay();
        ProcessEvents();
        ShipInteractions.Instance.CheckForInteractions(shipController.allShips);
    }

    void UpdateMovement()
    {
        int currentMovementTick = Mathf.FloorToInt(replayTime / replayMovementTick);
        if (currentMovementTick != lastMovementTick)
        {
            foreach (GameObject ship in shipController.allShips)
            {
                if (ship == null) continue;
                if (ship.CompareTag("Cargo")) ship.GetComponent<CargoBehavior>()?.Step(true);
                else if (ship.CompareTag("Patrol")) ship.GetComponent<PatrolBehavior>()?.Step(true);
                else if (ship.CompareTag("Pirate")) ship.GetComponent<PirateBehavior>()?.Step(true);
            }
            lastMovementTick = currentMovementTick;
        }
    }

    void ProcessEvents()
    {
        if (replaySpeed > 0) ProcessForward();
        else if (replaySpeed < 0) ProcessReverse();
    }

    void ProcessForward()
    {
        while (nextEventIndex < recordedEvents.Count && replayTime >= recordedEvents[nextEventIndex].timestamp)
        {
            ProcessEvent(recordedEvents[nextEventIndex]);
            nextEventIndex++;
        }
    }

    void ProcessReverse()
    {
        while (nextEventIndex > 0 && replayTime < recordedEvents[nextEventIndex - 1].timestamp)
        {
            nextEventIndex--;
            UndoEvent(recordedEvents[nextEventIndex]);
        }
    }

    void ProcessEvent(ReplayEvent evt)
    {
        switch (evt.eventType)
        {
            case "spawn":
                shipController.ReplaySpawn(evt.shipType, evt.position, evt.rotation, $"{evt.shipType}({evt.shipId})", evt.shipId);
                replayedShips[evt.shipId] = shipController.allShips.Last();
                break;
            case "capture":
                if (replayedShips.TryGetValue(evt.shipId, out GameObject cargo) && 
                   replayedShips.TryGetValue(evt.secondaryShipId, out GameObject pirate))
                {
                    cargo.GetComponent<CargoBehavior>().isCaptured = true;
                    pirate.GetComponent<PirateBehavior>().hasCargo = true;
                    cargo.transform.position = evt.position;
                    pirate.transform.position = evt.position;
                }
                break;
            case "rescue":
                if (replayedShips.TryGetValue(evt.shipId, out GameObject ship))
                    ship.GetComponent<CargoBehavior>().isCaptured = false;
                break;
        }
    }

    void UndoEvent(ReplayEvent evt)
    {
        switch (evt.eventType)
        {
            case "spawn":
                if (replayedShips.TryGetValue(evt.shipId, out GameObject ship))
                {
                    shipController.allShips.Remove(ship);
                    Destroy(ship);
                    replayedShips.Remove(evt.shipId);
                }
                break;
            case "capture":
                if (replayedShips.TryGetValue(evt.shipId, out GameObject cargo) && 
                   replayedShips.TryGetValue(evt.secondaryShipId, out GameObject pirate))
                {
                    cargo.GetComponent<CargoBehavior>().isCaptured = false;
                    pirate.GetComponent<PirateBehavior>().hasCargo = false;
                    cargo.transform.position = evt.position - new Vector3(0, 0, 2);
                    pirate.transform.position = evt.position;
                }
                break;
            case "rescue":
                if (replayedShips.TryGetValue(evt.shipId, out GameObject rescuedShip))
                    rescuedShip.GetComponent<CargoBehavior>().isCaptured = true;
                break;
        }
    }

    void HandleReplayInput()
    {
        if (Input.GetKeyDown(KeyCode.R)) SaveReplayToFile();
        if (Input.GetKeyDown(KeyCode.L)) LoadReplayFromFile();
    }

    public int GetNextShipId() => currentShipId++;

    public void RecordShipSpawn(int shipId, string shipType, Vector3 position, Quaternion rotation, float simTimestamp)
    {
        var evt = new ReplayEvent(shipId, shipType, position, rotation, simTimestamp, "spawn");
        recordedEvents.Add(evt);
        if (evt.timestamp > maxRecordedTime) maxRecordedTime = evt.timestamp;
    }

    public void RecordCaptureEvent(int cargoId, int pirateId, Vector3 position, Quaternion rotation, float simTimestamp)
    {
        var evt = new ReplayEvent(cargoId, "Cargo", position, rotation, simTimestamp, "capture", pirateId);
        recordedEvents.Add(evt);
        if (evt.timestamp > maxRecordedTime) maxRecordedTime = evt.timestamp;
    }

    public void RecordRescueEvent(int shipId, Vector3 position, Quaternion rotation, float simTimestamp)
    {
        var evt = new ReplayEvent(shipId, "Cargo", position, rotation, simTimestamp, "rescue");
        recordedEvents.Add(evt);
        if (evt.timestamp > maxRecordedTime) maxRecordedTime = evt.timestamp;
    }

    void StartReplay()
    {
        ReplayModeActive = true;
        UIvisibility(true);
        replayPaused = false;
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
        UIvisibility(true);
        shipController.ClearAllShips();
    }

    public void SaveReplayToFile()
    {
        if (recordedEvents.Count == 0) return;
        string path = Path.Combine(Application.persistentDataPath, replayFileName);
        File.WriteAllText(path, JsonUtility.ToJson(new ReplayData { events = recordedEvents }, true));
    }

    public void LoadReplayFromFile()
    {
        string path = Path.Combine(Application.persistentDataPath, replayFileName);
        if (File.Exists(path))
        {
            recordedEvents = JsonUtility.FromJson<ReplayData>(File.ReadAllText(path)).events;
            StartReplay();
        }
    }

    void UpdateDisplay() => timeDisplay.text = $"{maxRecordedTime - replayTime:0.0}s remaining  [Speed: {replaySpeed}x]";
    void TogglePlayPause() => replayPaused = !replayPaused;

    void IncreaseSpeed()
    {
        if (currentSpeedIndex < speeds.Length - 1 && ReplayModeActive)
        {
            currentSpeedIndex++;
            replaySpeed = speeds[currentSpeedIndex];
            UpdateDisplay();
        }
    }

    void DecreaseSpeed()
    {
        if (currentSpeedIndex > 0 && ReplayModeActive)
        {
            currentSpeedIndex--;
            replaySpeed = speeds[currentSpeedIndex];
            UpdateDisplay();
        }
    }

    public void UIvisibility(bool visible) => replayBoxUI.SetActive(visible);
}

[System.Serializable]
public struct ReplayEvent
{
    public int shipId;
    public int secondaryShipId;
    public string shipType;
    public Vector3 position;
    public Quaternion rotation;
    public float timestamp;
    public string eventType;

    public ReplayEvent(int id, string type, Vector3 pos, Quaternion rot, float time, string evtType, int secondaryId = -1)
    {
        shipId = id;
        secondaryShipId = secondaryId;
        shipType = type;
        position = pos;
        rotation = rot;
        timestamp = time;
        eventType = evtType;
    }
}

[System.Serializable]
public class ReplayData { public List<ReplayEvent> events; }