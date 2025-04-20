using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;

public class ReplayManager : MonoBehaviour
{
    public static ReplayManager Instance { get; private set; }

    // Existing UI references
    public Button playPauseButton;
    public Button btnIncreaseSpeed;
    public Button btnDecreaseSpeed;
    public Text timeDisplay;
    public ShipController shipController;

    public TimeControl timeControl;

    public GameObject replayBoxUI;

    public TextController textController;
    // Recording system
    private List<ReplayEvent> recordedEvents = new List<ReplayEvent>();
    private Dictionary<int, List<ReplayEvent>> tickData = new Dictionary<int, List<ReplayEvent>>();
    private int currentTick;
    private int maxRecordedTick;   
    private int lastRecordedTick = -1;
    // Replay state
    public float replayTime;
    private bool replayPaused;
    private int replayTick = -1;
    private Dictionary<int, GameObject> replayedShips = new Dictionary<int, GameObject>();
    private int currentShipId;

    public bool ReplayModeActive = false;
    public bool ReplayPaused => replayPaused;
    public readonly float[] speeds = { -1f, 1f, 2f, 10f, 20f };
    private int currentSpeedIndex = 1;
    public float replaySpeed { get; private set; } = 1f;

    void Awake()
    {
        Instance = this;
        currentShipId = 1;
    }

    void Start()
    {
        if (DataPersistence.Instance.wasEnteredfromLoadScene)
        {
            LoadReplayFromFile();
            ProcessLoadedEvents();
        }
        
        playPauseButton?.onClick.AddListener(TogglePlayPause);
        btnIncreaseSpeed?.onClick.AddListener(IncreaseSpeed);
        btnDecreaseSpeed?.onClick.AddListener(DecreaseSpeed);
        UIvisibility(true);
        ReplayModeActive = false;
    }

    void Update()
    {
   HandleReplayInput();
    
    if (ReplayModeActive && !replayPaused)
    {
        UpdateReplay();
    }
    else if (!ReplayModeActive && !timeControl.IsPaused)
    {
        // Use the actual simulation time for recording ticks
        float simTime = ShipController.TimeStepCounter * timeControl.GetSpeed();
        int currentSimTick = Mathf.FloorToInt(simTime / timeControl.GetSpeed());
        
        if (currentSimTick != lastRecordedTick)
        {
            RecordTick(currentSimTick);
            lastRecordedTick = currentSimTick;
        }
    }


    }
    void HandleReplayInput()
    {
        if (Input.GetKeyDown(KeyCode.R))
            SaveReplayToFile();
        if (Input.GetKeyDown(KeyCode.L))
            LoadReplayFromFile();
    }
    public void RecordTick(int currentTick)
    {
        // Clear previous tick's data if it exists
   //     recordedEvents.RemoveAll(e => e.tick == currentTick);
  
        foreach (GameObject ship in shipController.allShips)
        {
            int shipId = ExtractShipId(ship);
            if (shipId == -1) continue;

            recordedEvents.Add(new ReplayEvent(
                shipId,
                ship.tag,
                ship.transform.position,
                ship.transform.rotation,
                currentTick
            ));
        }    
        if (currentTick > maxRecordedTick) maxRecordedTick = currentTick;
        
    }

    private int ExtractShipId(GameObject ship)
    {
        string name = ship.name;
        int openParen = name.IndexOf('(');
        int closeParen = name.IndexOf(')');
        if (openParen >= 0 && closeParen > openParen)
        {
            string idStr = name.Substring(openParen + 1, closeParen - openParen - 1);
            if (int.TryParse(idStr, out int id))
            {
                return id;
            }
        }
        return -1;
    }

    void ProcessLoadedEvents()
    {
        tickData = recordedEvents
            .GroupBy(e => e.tick)
            .ToDictionary(g => g.Key, g => g.ToList());
        
        if (tickData.Count > 0)
        {
            maxRecordedTick = tickData.Keys.Max();
        }
    }

void UpdateReplay()
{
    if (timeControl == null)
    {
        Debug.LogError("TimeControl reference is missing!");
        return;
    }

    replayTime += Time.unscaledDeltaTime * replaySpeed;
    replayTime = Mathf.Clamp(replayTime, 0, maxRecordedTick);
    
    int targetTick = Mathf.FloorToInt(replayTime);
    
    if (Mathf.Approximately(replayTime, maxRecordedTick))
        replayPaused = true;

    if (targetTick != replayTick)
    {
        ApplyTick(targetTick);
        replayTick = targetTick;
    }

    UpdateDisplay();
}

    void ApplyTick(int tick)
    {
        ClearReplayedShips();

        if (tickData.TryGetValue(tick, out List<ReplayEvent> events))
        {
            foreach (ReplayEvent e in events)
            {
                GameObject ship = shipController.ReplaySpawn(
                    e.shipType,
                    e.position,
                    e.rotation,
                    $"{e.shipType}({e.shipId})",
                    e.shipId
                );
                replayedShips[e.shipId] = ship;
            }
        }
    }

    void ClearReplayedShips()
    {
        foreach (GameObject ship in replayedShips.Values)
        {
            if (ship != null)
            {
                Destroy(ship);
            }
        }
        replayedShips.Clear();
        
        shipController.ClearAllShips();
    }
    

    public void StartReplay()
    {
    if (recordedEvents.Count == 0)
    {
        Debug.LogWarning("No replay data to play");
        return;
    }
   // replaySpeed = speeds[1];
    ReplayModeActive = true;
    if (ShipController.Instance != null)
        ShipController.Instance.ClearAllShips();
    if (textController != null)
        textController.ResetCounters();
    
    replayPaused = true;
    replayTime = 0;
    replayTick = -1;
    UIvisibility(true);
    }

    // Rest of the original methods remain unchanged...
    void UpdateDisplay()
    {
        float timeLeft = (maxRecordedTick * timeControl.GetSpeed()) - replayTime;
        timeDisplay.text = $"Tick: {replayTick} | Speed: {replaySpeed}x | Time: {replayTime:0.0}s";
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

    public void SaveReplayToFile()
    {
        ReplayData data = new ReplayData();
        data.events = recordedEvents;
        var headerdata= new UILoadMenuController.MyData
        {
            saveName = DataPersistence.Instance.fileNameString,
            days = DataPersistence.Instance.dayCount,
            hours = DataPersistence.Instance.hourCount,
            cDay = DataPersistence.Instance.cargoDayPercent,
            cNight = DataPersistence.Instance.cargoNightPercent,
            piDay = DataPersistence.Instance.pirateDayPercent,
            piNight = DataPersistence.Instance.pirateNightPercent,
            paDay = DataPersistence.Instance.patrolDayPercent,
            paNight = DataPersistence.Instance.patrolNightPercent,
            pNightCap = DataPersistence.Instance.nightCaptureEnabled,
            events = data.events
        };
        data.header.Add(headerdata);
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(DataPersistence.Instance.path, json);
        Debug.Log("EVENTS SAVED " + data.events.Count); 
    }

    public void LoadReplayFromFile()
    {
        if (File.Exists(DataPersistence.Instance.path))
        {
            string json = File.ReadAllText(DataPersistence.Instance.path);
            ReplayData data = JsonUtility.FromJson<ReplayData>(json);
            recordedEvents = data.events;
            ProcessLoadedEvents();
            Debug.Log($"Loaded {recordedEvents.Count} events");
            StartReplay(); 
        }
        else
        {
            Debug.LogWarning("No replay file found.");
        }
    }

    public void UIvisibility(bool visible) => replayBoxUI.SetActive(visible);
    public int GetNextShipId() => currentShipId++;
}

[System.Serializable]
public class ReplayEvent
{
    public int shipId;
    public string shipType;
    public Vector3 position;
    public Quaternion rotation;
    public int tick;

    public ReplayEvent(int id, string type, Vector3 pos, Quaternion rot, int t)
    {
        shipId = id;
        shipType = type;
        position = pos;
        rotation = rot;
        tick = t;
    }
}

[System.Serializable]
public class ReplayData
{
    public List<UILoadMenuController.MyData> header = new List<UILoadMenuController.MyData>();
    public List<ReplayEvent> events = new List<ReplayEvent>();
}