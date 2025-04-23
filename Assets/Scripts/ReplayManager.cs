using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using TMPro;

public class ReplayManager : MonoBehaviour
{
    public static ReplayManager Instance { get; private set; }
    public UILoadMenuController UiLoad;
    // Existing UI references
    public Button playPauseButton;
    public Button btnIncreaseSpeed;
    public Button btnDecreaseSpeed;
    public Button btnStepFrame;
    public Button btnStepBackFrame;

    //public Text timeDisplay;
    public ShipController shipController;

    public Sprite playSprite;
    public Sprite pauseSprite;


    public TMP_Text speedText;

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
    private bool replayPaused = true;
    private int replayTick = -1;
    private Dictionary<int, GameObject> replayedShips = new Dictionary<int, GameObject>();
    private int currentShipId;
    public bool ReplayModeActive = false;
    public bool ReplayPaused => replayPaused;
    public readonly float[] speeds = { -1.0f, 1.0f, 2.0f, 10.0f, 20.0f };
    private int currentSpeedIndex = 1;
    public float replaySpeed { get; private set; } = 1f;

    void Awake()
    {
        Instance = this;
        currentShipId = 1;
    }

    void Start()
    {
        if (DataPersistence.Instance.wasEnteredfromLoadScene == true)
        {
            LoadReplayFromFile();
            ProcessLoadedEvents();
        }
        playPauseButton?.onClick.AddListener(TogglePlayPause);
        btnIncreaseSpeed?.onClick.AddListener(IncreaseSpeed);
        btnDecreaseSpeed?.onClick.AddListener(DecreaseSpeed);
        btnStepFrame?.onClick.AddListener(AdvanceSimulationTick);
        UIvisibility(true);
    }

    void Update()
    {
     HandleReplayInput();
    
    if (ReplayModeActive && !replayPaused)
    {
        UpdateReplay();
    }
    if (!ReplayModeActive && timeControl.IsPaused )
    {
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
      string counters = string.Join(",",
        textController.cargoEntered,
        textController.cargoExited,
        textController.patrolEntered,
        textController.patrolExited,
        textController.pirateEntered,
        textController.pirateExited,
        textController.captureCount,
        textController.rescueCount,
        textController.piratesDestroyed,
        textController.successfulEvasions,
        textController.failedEvasions
    );
        foreach (GameObject ship in shipController.allShips)
        {
            if (ship == null) continue; // Skip destroyed GameObjects

            int shipId = ExtractShipId(ship);
            if (shipId == -1) continue;

            recordedEvents.Add(new ReplayEvent(
                shipId,
                ship.tag,
                ship.transform.position,
                ship.transform.rotation,
                currentTick,
                counters
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
            .GroupBy(e => e.t)
            .ToDictionary(g => g.Key, g => g.ToList());
        
        if (tickData.Count > 0)
        {
            maxRecordedTick = tickData.Keys.Max();
        }
    }
    void AdvanceSimulationTick()
    {
        // Only advance simulation tick if we're not in replay mode and simulation is paused.
        if (!ReplayModeActive && timeControl.IsPaused)
        {
            ShipController.Instance.ManualAdvanceTick();
        }
    }
void UpdateReplay()
{

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
    replaySpeed = speeds[1];
    //replaySpeed = speeds[1]; // breaks
    UpdateDisplay();
    ReplayModeActive = true;
    if (ShipController.Instance != null)
        ShipController.Instance.ClearAllShips();
    if (textController != null) 
        textController.ResetCounters();
    
    replayPaused = true;
    replayTime = 0;
    if (playPauseButton != null && playSprite != null)
    {
        playPauseButton.image.sprite = playSprite;
    }
    replayTick = -1;
    UIvisibility(true);
    UpdateDisplay();
    }

    void UpdateDisplay()
    {
        float timeLeft = (maxRecordedTick * timeControl.GetSpeed()) - replayTime;
        //timeDisplay.text = $"Tick: {replayTick} | Speed: {replaySpeed}x | Time: {replayTime:0.0}s";
        // Update sim-style clock UI in replay
        if (shipController != null && shipController.timeDisplayRun != null)
        {
            float totalMinutes = replayTick * 5f;
            int day = Mathf.FloorToInt(totalMinutes / 1440f) + 1;
            int hour = Mathf.FloorToInt((totalMinutes / 60f) % 24);
            int minute = Mathf.FloorToInt(totalMinutes % 60);
            bool isNight = (hour >= 12);
            string phase = isNight ? "Night" : "Day";

            shipController.timeDisplayRun.text = $"{phase} {day} — {hour:D2}:{minute:D2}";
        }

        if (shipController != null && shipController.timeDisplayRemaining != null)
        {
            float totalSimMinutes = maxRecordedTick * 5f;
            float minutesPassed = replayTick * 5f;
            float remainingMinutes = totalSimMinutes - minutesPassed;
            if (remainingMinutes < 0) remainingMinutes = 0;
            int remainingDays = Mathf.FloorToInt(remainingMinutes / 1440f);
            int remainingHours = Mathf.FloorToInt((remainingMinutes % 1440f) / 60f);
            int remainingMins = Mathf.FloorToInt(remainingMinutes % 60f);

            shipController.timeDisplayRemaining.text = $"Remaining: {remainingDays}d {remainingHours}h {remainingMins}m";
        }
    }

    void TogglePlayPause()
    {
        replayPaused = !replayPaused;
//        timeControl.TogglePlayPause();
        if (playPauseButton != null)
        {
            playPauseButton.image.sprite = replayPaused ? pauseSprite : playSprite;
        }
    }
    void IncreaseSpeed()
    {
        if (currentSpeedIndex < speeds.Length - 1)
        {
            currentSpeedIndex++;
            replaySpeed = speeds[currentSpeedIndex];
            Debug.Log($"Replay speed set to {replaySpeed}x");
            UpdateDisplay();
            speedText.text = replaySpeed + "X SPEED";
        }
    }

    void DecreaseSpeed()
    {
        if (currentSpeedIndex > 0)
        {
            currentSpeedIndex--;
            replaySpeed = speeds[currentSpeedIndex];
            if(!ReplayModeActive && replaySpeed == -1.0f)
            {
                replaySpeed = 1.0f;
                currentSpeedIndex++;
            }
            Debug.Log($"Replay speed set to {replaySpeed}x");
            UpdateDisplay();
            speedText.text = replaySpeed + "X SPEED";
        }
    }




    public void SaveReplayToFile()
    {
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
            mTick = maxRecordedTick,
            events = recordedEvents
        };
            DataPersistence.Instance.maxTick = maxRecordedTick;
            string json = JsonUtility.ToJson(headerdata, true);
            string fileName = string.IsNullOrEmpty(DataPersistence.Instance.fileNameString) ?
                          "Default" : DataPersistence.Instance.fileNameString;
            fileName += ".json";
//string path = Path.Combine(Application.dataPath, fileName);
 //File.WriteAllText(path, json);
     //   DataPersistence.Instance.replayEvents = recordedEvents;
//Debug.Log("saved");
        #if UNITY_WEBGL && !UNITY_EDITOR

        UIControllerScript.Instance.DownloadFile(fileName, json);
#else
        // For standalone builds, write file locally.
      //  string path = Path.Combine(Application.dataPath, fileName);
       // DataPersistence.Instance.path = path;
      //  File.WriteAllText(path, json);
     //   DataPersistence.Instance.replayEvents = recordedEvents;
        Debug.Log("saved");
#endif
    }

    

 public void LoadReplayFromFile()
{
        maxRecordedTick = DataPersistence.Instance.maxTick;
        recordedEvents = DataPersistence.Instance.replayEvents;
        ProcessLoadedEvents();
        Debug.Log($"Loaded {recordedEvents.Count} events from file");
        StartReplay();
}

    void ApplyTick(int tick)
    {
        ClearReplayedShips();
        if (tickData.TryGetValue(tick, out List<ReplayEvent> events))
        {
            foreach (ReplayEvent e in events)
            {
                GameObject ship = shipController.ReplaySpawn(
                    e.sT,
                    e.p,
                    e.r,
                    $"{e.sT}({e.sId})",
                    e.sId
                );
                replayedShips[e.sId] = ship;
                textController.ApplyCountersFromString(e.c);
            }
        }
    }
    public void UIvisibility(bool visible) => replayBoxUI.SetActive(visible);
    public int GetNextShipId() => currentShipId++;
}

[System.Serializable]
public class ReplayEvent
{
    public int sId;
    public string sT;
    public Vector3 p;
    public Quaternion r;
    public int t;
    public string c;

    public ReplayEvent(int id, string type, Vector3 pos, Quaternion rot, int tick, string counters)
    {
        sId = id;
        sT = type;
        p = pos;
        r = rot;
        t = tick;
        c = counters;
    }
}

[System.Serializable]
public class ReplayData
{
  //  public List<UILoadMenuController.MyData> header = new List<UILoadMenuController.MyData>();
    public List<ReplayEvent> events = new List<ReplayEvent>();
}