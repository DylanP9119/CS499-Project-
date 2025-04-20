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
    public TimeControl timeControl;
    public TextController textController;

    private string replayFileName = "";
    public GameObject replayBoxUI;
    
    // Use a shorter tick interval for replay ship movement.
    public float replayMovementTick = 1; // ???

    //private List<ReplayEvent> recordedEvents = new List<ReplayEvent>();
    public float replayTime;  // Simulation time (seconds) in replay.
    private bool replayPaused;
    private float maxRecordedTime;
    
    private int maxTicks = 0;
    private int currentShipId;

    // Pointer for processing replay events.
    private int nextEventIndex = 0;

    // Mapping from ship ID to spawned GameObject.
    private Dictionary<int, List<ReplayEvent>> replayedShips = new();
    private List<ReplayEvent> recordedEvents = new List<ReplayEvent>();


    public bool ReplayModeActive { get; private set; }

    // Expose replayPaused state.
    public bool ReplayPaused => replayPaused;

    // Define discrete speeds: -1x, 1x, 2x, 10x, 20x.
    public readonly float[] speeds = { -1f, 1f, 2f, 10f, 20f };
    private int currentSpeedIndex = 1; // Start at 1x.
    public float replaySpeed { get; private set; } = 1f;

    // Track last movement tick for replay mode.
    private int lastProcessedTick = -1;
    private int temptick = 1;
    void Awake()
    {
        Instance = this;
        currentShipId = 1;
    }

    void Start()
    {
        ReplayModeActive = false;
        if(DataPersistence.Instance.wasEnteredfromLoadScene == true)
        {
            LoadReplayFromFile();
        }
        replayFileName = DataPersistence.Instance.path;
        if (playPauseButton != null)
            playPauseButton.onClick.AddListener(TogglePlayPause);
        if (btnIncreaseSpeed != null)
            btnIncreaseSpeed.onClick.AddListener(IncreaseSpeed);
        if (btnDecreaseSpeed != null)
            btnDecreaseSpeed.onClick.AddListener(DecreaseSpeed);

        UIvisibility(true);
    }

    void Update()
    {
        HandleReplayInput();

        if (ReplayModeActive && !replayPaused)
        {
            // Even if replay is paused, update movement ticks.
         //   replayTime += replaySpeed * Time.unscaledDeltaTime;
          //  replayTime = Mathf.Clamp(replayTime, 0, maxRecordedTime);
            replayTime += Time.deltaTime * TimeControl.Instance.GetSpeed();
            if (Mathf.Approximately(replayTime, maxTicks))
                replayPaused = true;

            // Compute simulation tick for replay events.
            int tick = Mathf.FloorToInt(replayTime / TimeControl.Instance.GetSpeed()); // changed since main check if work
            ShipController.SetTimeStepCounter(tick);
            if(ReplayModeActive == false)
            {
            if(tick != lastProcessedTick)
            {
                lastProcessedTick = tick;
                StepToTick(tick);
            }
            }
            if(ReplayModeActive == true)
            {
            if(temptick <= maxTicks)
            {
                StepToTick(temptick);
                UpdateDisplay();
                temptick++;
            }
            }
            // Use a separate tick for ship movement.
           
 //       int currentMovementTick = Mathf.FloorToInt(replayTime / replayMovementTick);
           
   //     UpdateDisplay();
  //      }
   // 
    //          ShipInteractions.Instance.CheckForInteractions(shipController.allShips); NOT IN JERIELS DOES IT WORK???
        }
    }

    void HandleReplayInput()
    {
        if (Input.GetKeyDown(KeyCode.R))
            SaveReplayToFile();
        if (Input.GetKeyDown(KeyCode.L))
            LoadReplayFromFile();
    }

///////////// BEGIIN JERIEL CHANGES 
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

    void DecreaseSpeed()
    {   
        if (replayTime < 0f) replayTime = 0f;  
        if (ReplayManager.Instance.ReplayModeActive && ReplayModeActive)
        {
            if (currentSpeedIndex >= 1)
            {
                currentSpeedIndex--;
                replaySpeed = speeds[currentSpeedIndex];
                Debug.Log($"Replay speed set to {replaySpeed}x");
                UpdateDisplay();
            }
            if (currentSpeedIndex == 0) //REVERSE I ADDED THIS
            { 
                replayTime -= TimeControl.Instance.GetSpeed();
                if (replayTime < 0f) replayTime = 0f;

                int tick = Mathf.FloorToInt(replayTime / TimeControl.Instance.GetSpeed());
                lastProcessedTick = -1; // Force replay to step again even if same tick
                StepToTick(tick);

                Debug.Log($"[REPLAY] Stepped backward to tick {tick}");           
            }
        }
    }

    public void StepToTick(int tick)
    {
        // Clear all current ships
        ShipController.Instance.ClearAllShips();

        // Load snapshot for this tick
        List<ReplayEvent> events = replayedShips[tick];

        foreach (ReplayEvent s in events)
        {
            GameObject prefab = GetPrefabForType(s.shipType);
            if (prefab == null)
            {
                Debug.LogWarning($"[Replay] No prefab found for {s.shipType}");
                continue;
            }

            GameObject obj = GameObject.Instantiate(prefab, s.position, s.rotation);
            obj.name = $"{s.shipType}({s.shipId})";
            obj.tag = s.shipType;
            ShipController.Instance.allShips.Add(obj);
        }

        Debug.Log($"[Replay] Rebuilt tick {tick} with {events.Count} ships.");
    }

    public void RecordSnapshotForTick(int tick, List<GameObject> allShips)
    {
        if(!ReplayModeActive)
        {
        List<ReplayEvent> snapshot = new List<ReplayEvent>(); 
        foreach (GameObject ship in allShips)
        {
            if (ship == null) continue;

            ReplayEvent s = new ReplayEvent
            {
                shipId = ExtractShipId(ship),
                shipType = ship.tag,
                position = ship.transform.position,
                rotation = ship.transform.rotation
            };
            snapshot.Add(s);
            recordedEvents.Add(s);
        }
        replayedShips[tick] = snapshot;
        Debug.Log($"[Snapshot] Recorded {snapshot.Count} ships at tick {tick}");
        }
    }

    private GameObject GetPrefabForType(string shipType)
    {
        switch (shipType)
        {
            case "Cargo": return ShipController.Instance.cargoPrefab;
            case "Pirate": return ShipController.Instance.piratePrefab;
            case "Patrol": return ShipController.Instance.patrolPrefab;
            default: return null;
        }
    }

///////////////// END JERIEL CHANGES 


    void IncreaseSpeed()
    {
        if (ReplayManager.Instance.ReplayModeActive && ReplayModeActive)
        {
            if (currentSpeedIndex < speeds.Length - 1)
            {
                currentSpeedIndex++;
                replaySpeed = speeds[currentSpeedIndex];
                Debug.Log($"Replay speed set to {replaySpeed}x");
                UpdateDisplay();
            }
        }
    }


    void StartReplay()
    {
        ReplayModeActive = true;
        UIvisibility(true);
        replayPaused = false; // Start replay unpaused.
        replayTime = 0;  
        lastProcessedTick = -1;

        //nextEventIndex = 0;
        textController.ResetCounters();
        shipController.ClearAllShips();

        //recordedEvents = recordedEvents.OrderBy(e => e.timestamp).ToList();
        //maxRecordedTime = recordedEvents.Count > 0 ? recordedEvents.Max(e => e.timestamp) : 0;
 
    }

    void StopReplay()
    {
        ReplayModeActive = false;
        UIvisibility(false);
        shipController.ClearAllShips();
    }

    public void SaveReplayToFile()
    {
        UIReplayData tempheader = new UIReplayData();
        foreach(var pair in replayedShips)
        {
            ReplayData t = new ReplayData();
            t.tick = pair.Key;
            t.events = pair.Value;
            tempheader.timeline.Add(t);
        }

        var data = new UILoadMenuController.MyData
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
            events = tempheader.timeline
        };
        
        tempheader.header.Add(data);
        string json = JsonUtility.ToJson(tempheader, true);
        File.WriteAllText(DataPersistence.Instance.path, json);
      //  DataPersistence.Instance.replayEvents = recordedEvents;
        maxTicks = replayedShips.Count;
        Debug.Log($"Saved {replayedShips.Count} events to: {DataPersistence.Instance.path}");
    }

    public void LoadReplayFromFile()
    {
    /*    if (DataPersistence.Instance.replayEvents != null)
    //    {
     //       recordedEvents = DataPersistence.Instance.replayEvents;
     //       Debug.Log($"Loaded {DataPersistence.Instance.replayEvents.Count} events");
    //        StartReplay();
      //  }
    //    else
    //    {
    //        Debug.LogWarning("No replay file found.");
    */  // }


        /// FOR TESTING HARD CODED PURPOSES ONLY COMMENT OUT
        ReplayModeActive = true;
        string path = Path.Combine(Application.persistentDataPath, DataPersistence.Instance.path);
        if (!File.Exists(path))
        {
            Debug.LogWarning("[REPLAY] File not found: " + path);
            return;
        }

    string json = File.ReadAllText(path);
    UIReplayData data = JsonUtility.FromJson<UIReplayData>(json);
    replayedShips.Clear();
    foreach(ReplayData tick in data.timeline )
    {
        replayedShips[tick.tick] = tick.events;
    }
    StartReplay();

    }

    void UpdateDisplay()
    {
        float timeLeft = maxRecordedTime - replayTime;
        timeDisplay.text = $"{timeLeft:0.0}s remaining  [Speed: {replaySpeed}x]";
    }

    void TogglePlayPause() => replayPaused = !replayPaused;



    public void UIvisibility(bool visible) => replayBoxUI.SetActive(visible);

}
[System.Serializable]
public class ReplayEvent
{
    public int shipId;
    public string shipType;
    public Vector3 position;
    public Quaternion rotation;
    //public string eventType;

  //  public ReplayEvent(int id, string type, Vector3 pos, Quaternion rot)
  //  {
  //      shipId = id;
  //      shipType = type;
  //      position = pos;
  //      rotation = rot;
  //      //timestamp = time;
        //eventType = evtType;
  //  }
}

[System.Serializable]
public class ReplayData
{
    public int tick; 
    public List<ReplayEvent> events = new();
}
[System.Serializable]
public class UIReplayData
{
    public List<UILoadMenuController.MyData> header = new List<UILoadMenuController.MyData>();
    public List<ReplayData> timeline = new();
}