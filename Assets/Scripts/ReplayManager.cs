using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
        
        if (ReplayModeActive && !replayPaused)
        {
            // Advance replay time using unscaled deltaTime
            replayTime += Time.unscaledDeltaTime;
            replayTime = Mathf.Clamp(replayTime, 0, maxRecordedTime);
using System.IO;
using System.Linq;

public class ReplayManager : MonoBehaviour
{
        public static ReplayManager Instance { get; private set; }
  public Slider timelineSlider;
    public Button playPauseButton;
    public Text timeDisplay;
    public ShipController shipController;
    public string replayFileName = "S:\replay.json";
    public GameObject replayBoxUI;
    // List of all replay events (spawn and movement).
    private List<ReplayEvent> recordedEvents = new List<ReplayEvent>();
    private List<ReplayEvent> currentSessionEvents = new List<ReplayEvent>();
    private float replayTime;
    private bool replayPaused;
    private float maxRecordedTime;
    public bool ReplayModeActive { get; private set; }
    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        timelineSlider.onValueChanged.AddListener(UpdateReplayTime);
        playPauseButton.onClick.AddListener(TogglePlayPause);
        UIvisibility(false);
    }
    void Update()
    {
        HandleReplayInput();
        
        if (ReplayModeActive && !replayPaused)
        {
          // Advance replay time using unscaled deltaTime
            replayTime += Time.unscaledDeltaTime;
            replayTime = Mathf.Clamp(replayTime, 0, maxRecordedTime);
            timelineSlider.value = replayTime;
            UpdateDisplay();
            RebuildShipsAtCurrentTime();
        }
    }
  void HandleReplayInput()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
        SaveReplayToFile();
        }
    
        if (Input.GetKeyDown(KeyCode.L))
        {
       LoadReplayFromFile();
            // Enter replay mode using all recorded events.
            StartReplay(useCurrentSession: false);
        }
      
        if (Input.GetKeyDown(KeyCode.R))
        {
    if (!ReplayModeActive) 
            {
           currentSessionEvents = new List<ReplayEvent>(recordedEvents);
                // Start replay from the current simulation time.
                StartReplay(useCurrentSession: true);
            }
            else
            {

           StopReplay();
            }
        }
    }
   // Called by ShipController when a ship is spawned.
    public void RecordShipSpawn(ReplayEvent evt)
    {
        recordedEvents.Add(evt);
        if (evt.timestamp > maxRecordedTime) 
            maxRecordedTime = evt.timestamp;
    }
public void RecordMovementEvent(int shipId, string shipType, Vector3 position, Quaternion rotation, float timestamp)
{
    recordedEvents.Add(new ReplayEvent(shipId, shipType, position, rotation, timestamp, false));
    if (timestamp > maxRecordedTime) 
        maxRecordedTime = timestamp;
}
   public void StartReplay(bool useCurrentSession)
    {
        ReplayModeActive = true;
        UIvisibility(true);
        // Pause simulation movement.
        shipController.timeControl.ToggleMovement(true);
        shipController.ClearAllShips();

        if (useCurrentSession)
        {
            recordedEvents = new List<ReplayEvent>(currentSessionEvents);
        }
        
        maxRecordedTime = GetMaxTimestamp();
        // Initialize replayTime to the current simulation (global) time.
        replayTime = shipController.timeControl.GlobalTime;
        timelineSlider.maxValue = maxRecordedTime;
        timelineSlider.value = replayTime;
        UpdateDisplay();
        RebuildShipsAtCurrentTime();
    }
    public void StopReplay()
    {
        ReplayModeActive = false;
        UIvisibility(false);
        // Resume simulation movement.
        shipController.timeControl.ToggleMovement(false);
        shipController.ClearAllShips();
        recordedEvents = new List<ReplayEvent>(currentSessionEvents);
    }
   public void SaveReplayToFile()
    {
        ReplayData data = new ReplayData { events = recordedEvents };
        string json = JsonUtility.ToJson(data, true);
        string path = Path.Combine(Application.persistentDataPath, replayFileName);
        File.WriteAllText(path, json);
        Debug.Log($"Replay saved to: {path}");
    }
   public void LoadReplayFromFile()
    {
        string path = Path.Combine(Application.persistentDataPath, replayFileName);
        if (File.Exists(path))
        {
          string json = File.ReadAllText(path);
            ReplayData data = JsonUtility.FromJson<ReplayData>(json);
            recordedEvents = data.events;
            maxRecordedTime = GetMaxTimestamp();
            Debug.Log($"Replay loaded from: {path}");
        }
 else
        {
       Debug.LogError("No saved replay file found!");
        }
    }
  void UpdateReplayTime(float time)
    {
        replayTime = time;
        RebuildShipsAtCurrentTime();
        UpdateDisplay();
    }
   void TogglePlayPause()
    {
        replayPaused = !replayPaused;
    }
   // Rebuilds the current scene by grouping events per ship (using ShipId)
    // and instantiating a ship at the last recorded position for each ship that has an event with timestamp ≤ replayTime.
   void RebuildShipsAtCurrentTime()
    {
        shipController.ClearAllShips();
        var activeShips = new Dictionary<int, GameObject>();
   foreach (var evt in recordedEvents.Where(e => e.timestamp <= replayTime))
        {
   if (evt.isSpawnEvent)
            {
  // Create new ship instance
                var prefab = GetPrefabForType(evt.shipType);
                if (prefab != null)
                {


             var ship = Instantiate(prefab, evt.position, evt.rotation);
                    shipController.allShips.Add(ship);
                    activeShips[evt.shipId] = ship;
                    
                    // Disable movement scripts immediately
                    Destroy(ship.GetComponent<PirateBehavior>());
                    Destroy(ship.GetComponent<CargoBehavior>());
                    Destroy(ship.GetComponent<PatrolBehavior>());
                }
            }
     else if (activeShips.TryGetValue(evt.shipId, out var ship))
            {

    // Update existing ship position
                ship.transform.position = evt.position;
                ship.transform.rotation = evt.rotation;
     }
        }
    }
GameObject GetPrefabForType(string shipType)
{
    switch (shipType)
    {
        case "Cargo": return shipController.cargoPrefab;
        case "Patrol": return shipController.patrolPrefab;
        case "Pirate": return shipController.piratePrefab;
        default: return null;
    }
}
   void UpdateDisplay()
    {
        timeDisplay.text = $"Time: {replayTime:0.0}s";
        timelineSlider.value = replayTime;
    }

    float GetMaxTimestamp()
    {
        float max = 0;
        foreach (var evt in recordedEvents)
        {
   if (evt.timestamp > max)
                max = evt.timestamp;
        }
        return max;
    }
 public void UIvisibility(bool visible)
    {
        replayBoxUI.SetActive(visible);
    }
    [System.Serializable]
    class ReplayData
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
    public bool isSpawnEvent; // True if this event is the spawn event

    // Constructor for spawn events.
    public ReplayEvent(int id, string type, Vector3 pos, Quaternion rot, float time, bool isSpawn)
    {
        shipId = id;
        shipType = type;
        position = pos;
        rotation = rot;
        timestamp = time;
        isSpawnEvent = isSpawn;
    }
}

}




