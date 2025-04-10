using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
<<<<<<< Updated upstream
<<<<<<< Updated upstream
using ReplayData;
using System.IO;
using Recorder; 
using System.Reflection; 
=======
using System.IO;
using System.Linq;
>>>>>>> Stashed changes

public class ReplayManager : MonoBehaviour
{
<<<<<<< Updated upstream
    public class ReplayManager : MonoBehaviour
    {       
        private Camera mainCamera;
        private Camera replayCameraComponent;
    [System.Serializable]
        public class ReplayDataWrapper
        {
            public List<RecordData> records;
        }
    [System.Serializable]
    public class RecordData
    {
        public string gameObjectPath;
        public bool isInstantiated;
        public int firstFrameIndex;
        public int deletedFrame;
        public List<FrameData> frames;
        public string shipType;
    }

    [System.Serializable]
    public class FrameData
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public Vector3 rbVelocity;
        public Vector3 rbAngularVelocity;
    }
 
 


    // Helper method to get GameObject path
    private string GetGameObjectPath(GameObject go)
    {
        if (go == null) return "";
        string path = "/" + go.name;
        Transform parent = go.transform.parent;
        while (parent != null)
        {
            path = "/" + parent.name + path;
            parent = parent.parent;
        }
            return path;
        }

        // Helper method to find GameObject by path
        private GameObject FindGameObjectByPath(string path)
        {
            string[] parts = path.Split('/');
            if (parts.Length < 2) return null; // Empty or invalid path

            Transform current = null;
            for (int i = 1; i < parts.Length; i++)
            {
                string part = parts[i];
                if (current == null)
                {
                    current = GameObject.Find(part)?.transform;
                }
                else
                {
                    current = current.Find(part);
                }
                if (current == null) break;
            }
            return current?.gameObject;
        }

        public void SaveReplay(string fileName)
        {
            List<RecordData> recordDataList = new List<RecordData>();

            foreach (Record record in records)
            {
                RecordData rd = new RecordData();
                rd.gameObjectPath = GetGameObjectPath(record.GetGameObject());
                rd.isInstantiated = record.IsInstantiated();
                rd.firstFrameIndex = record.GetFirstFrameIndex();
                rd.deletedFrame = record.GetRecordDeletedFrame();

                // Get all frames from the record
                List<Frame> frames = new List<Frame>();
                for (int i = 0; i < record.GetLength(); i++)
                {
                    frames.Add(record.GetFrameAtIndex(i));
                }

                rd.frames = new List<FrameData>();
                foreach (Frame frame in frames)
                {
                    FrameData fd = new FrameData();
                    fd.position = frame.GetPosition();
                    fd.rotation = frame.GetRotation();
                    fd.scale = frame.GetScale();
                    fd.rbVelocity = frame.GetRBVelocity();
                    fd.rbAngularVelocity = frame.GetRBAngularVelocity();

                    rd.frames.Add(fd);
                }
                recordDataList.Add(rd);
            }

            ReplayDataWrapper wrapper = new ReplayDataWrapper();
            wrapper.records = recordDataList;

            string json = JsonUtility.ToJson(wrapper, true);
            string filePath = Path.Combine(Application.persistentDataPath, fileName + ".json");
            File.WriteAllText(filePath, json);
            Debug.Log("Replay saved to: " + filePath);
        }
            private Camera mainCameraReference;

public void PrepareReplayScene()
{
    DontDestroyOnLoad(mainCamera.gameObject);
    // Rest of existing PrepareReplayScene code
    foreach (Record record in FindObjectsOfType<Record>())
    {
        if (record != null && record.gameObject != null)
        {
            Destroy(record.gameObject);
        }
    }
    records.Clear();
    DeletedPool.Clear();
}

public void LoadReplay(string fileName)
{
    PrepareReplayScene();
    
    string path = Path.Combine(Application.persistentDataPath, fileName + ".json");
    if (!File.Exists(path)) return;

    string json = File.ReadAllText(path);
    ReplayDataWrapper wrapper = JsonUtility.FromJson<ReplayDataWrapper>(json);

    foreach (RecordData rd in wrapper.records)
    {
        GameObject prefab = GetPrefabByType(rd.shipType);
        if (prefab != null)
        {
            // Instantiate at recorded position from first frame
            Vector3 spawnPos = rd.frames.Count > 0 ? rd.frames[0].position : Vector3.zero;
            Quaternion spawnRot = rd.frames.Count > 0 ? rd.frames[0].rotation : Quaternion.identity;
            
            GameObject ship = Instantiate(prefab, spawnPos, spawnRot);
            Record record = ship.GetComponent<Record>();
            record.Initialize();

            // Restore all frames
            List<Frame> frames = new List<Frame>();
            foreach (FrameData fd in rd.frames)
            {
                Frame frame = new Frame(fd.position, fd.rotation, fd.scale);
                frame.SetRBVelocities(fd.rbVelocity, fd.rbAngularVelocity);
                frames.Add(frame);
            }
            
            typeof(Record).GetField("frames", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(record, frames);

            records.Add(record);
        }
    }
}

private GameObject GetPrefabByType(string shipType)
{
    return shipType switch
    {
        "Cargo" => FindObjectOfType<ShipController>().cargoPrefab,
        "Patrol" => FindObjectOfType<ShipController>().patrolPrefab,
        "Pirate" => FindObjectOfType<ShipController>().piratePrefab,
        _ => null
    };
}
    
        public enum ReplayState { PAUSE, PLAYING, TRAVEL_BACK }
        //States
        ReplayState state = ReplayState.PAUSE;
=======
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
>>>>>>> Stashed changes

    void Start()
    {
        timelineSlider.onValueChanged.AddListener(UpdateReplayTime);
        playPauseButton.onClick.AddListener(TogglePlayPause);
        UIvisibility(false);
    }

<<<<<<< Updated upstream
        [Header("Maximum frames recorded")]
        [SerializeField] private int recordMaxLength = 3600; // 60fps * 60seconds = 3600 frames 
        private int maximumLength = 0;

        [Header("Optimization frame interpolation")]
        [SerializeField] private bool interpolation = false;
        //timer to record with intervals
        private float recordTimer = 0;
        [Tooltip("Time between recorded frames")]
        [SerializeField] private float recordInterval = 0.2f;

        //replay current frame index
        private int frameIndex = 0;
        //replay timer used for interpolation
        private float replayTimer = 0;

        //replay speeds
        private float[] speeds = { 0.25f, 0.5f, 1.0f, 2.0f, 4.0f };
        private int speedIndex = 2;
        private float slowMotionTimer = 0;

        //UI elements
        private bool usingSlider = false;
        [Header("Replay System UI")]
        public Slider timeLine;
        public GameObject replayBoxUI;

        //--------Replay cameras----------------
        //gameplay camera recorded
        private Camera current;
        //created replay camera to move freely
        private Camera[] cameras;
        private int cameraIndex = 0;

        //Deleted gameobjects pool
        private List<GameObject> DeletedPool = new List<GameObject>();

        private void Awake()
        {
            //needs to have a consistent frame rate,
            //if the frameRate is increased to 144 f.e., the replay would last a maximum of 69 seconds.
            //This is due to how the unity's internal animator recorder works, as it can only record up to 10000 frames, no more.
            //At 60fps the replay can reach up to 166 seconds.
            Application.targetFrameRate = 5;
=======
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
>>>>>>> Stashed changes
        }
    }

    void HandleReplayInput()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
=======
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
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
<<<<<<< Updated upstream
        // Add to ReplayManager class
        private void HandleDeletedObjects(Record rec, int frameIndex)
{
    GameObject deletedGO = rec.GetGameObject(); // Now using valid method
    if (deletedGO.activeInHierarchy && frameIndex >= rec.GetRecordDeletedFrame())
    {
        deletedGO.SetActive(false);
    }
}
=======
    }
>>>>>>> Stashed changes
=======
    }
>>>>>>> Stashed changes

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
<<<<<<< Updated upstream
<<<<<<< Updated upstream
                    //update slider value
                    timeLine.value = frameIndex;

                    if (frameIndex < recordMaxLength - 1 && frameIndex < timeLine.maxValue - 1)
                    {
                        for (int i = 0; i < records.Count; i++)
                        {
                            //Check for instantiated and deleted GOs
                            int auxIndex = frameIndex - records[i].GetFirstFrameIndex();
                            HandleDeletedObjects(records[i], frameIndex);
                            HandleInstantiatedObjects(records[i], auxIndex);

                            //if record exists at frameIndex moment
                            if (IsRecordActiveInReplay(records[i], frameIndex))
                            {
                                //transforms
                                if (interpolation)
                                {
                                    float max = Application.targetFrameRate * recordInterval;
                                    float value = replayTimer / max;
                                    InterpolateTransforms(records[i], auxIndex, value);
                                }
                                else
                                {
                                    //not slowmotion
                                    if (speeds[speedIndex] >= 1)
                                        SetTransforms(records[i], auxIndex);
                                    else
                                    {
                                        if (slowMotionTimer == 0)
                                            SetTransforms(records[i], auxIndex);
                                        else //interpolate slow motion frames
                                            InterpolateTransforms(records[i], auxIndex, slowMotionTimer);
                                    }
                                }


                                //animations 
                                Animator animator = records[i].GetAnimator();
                                if (animator != null)
                                {
                                    float time = (animator.recorderStopTime - animator.recorderStartTime) / records[i].GetLength();

                                    if (interpolation)
                                        time = (animator.recorderStopTime - animator.recorderStartTime) / records[i].GetAnimFramesRecorded();

                                    //Speed of replay
                                    time *= speeds[speedIndex];

                                    if (animator.playbackTime + time <= animator.recorderStopTime)
                                        animator.playbackTime += time;
                                }
                            }
                        }

                        if (interpolation)
                        {
                            replayTimer += speeds[speedIndex];
                            float frames = Application.targetFrameRate * recordInterval;
                            if (replayTimer >= frames)
                            {
                                replayTimer = 0;
                                frameIndex++;
                            }
                        }
                        else
                        {
                            if (speeds[speedIndex] >= 1)
                                frameIndex += (int)speeds[speedIndex];
                            else
                            {
                                slowMotionTimer += speeds[speedIndex];

                                if (slowMotionTimer >= 1f)
                                {
                                    frameIndex++;
                                    slowMotionTimer = 0;
                                }
                            }
                        }
                    }
                    else
                        PauseResume();


                }
                //TRAVEL BACK IN TIME FUNCTIONALITY
                else if (state == ReplayState.TRAVEL_BACK)
                {
                    TravelBack();
=======
=======
>>>>>>> Stashed changes
                    var ship = Instantiate(prefab, evt.position, evt.rotation);
                    shipController.allShips.Add(ship);
                    activeShips[evt.shipId] = ship;
                    
                    // Disable movement scripts immediately
                    Destroy(ship.GetComponent<PirateBehavior>());
                    Destroy(ship.GetComponent<CargoBehavior>());
                    Destroy(ship.GetComponent<PatrolBehavior>());
<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
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


<<<<<<< Updated upstream
<<<<<<< Updated upstream
        //This function is responsible for activating and deactivating instantiated GO, dependenig on the current time of the replay 
void HandleInstantiatedObjects(Record rec, int index)
{
    GameObject go = rec.GetGameObject();
    if (go == null) return;

    // Always apply transforms regardless of active state
    int auxIndex = index - rec.GetFirstFrameIndex();
    if (auxIndex >= 0 && auxIndex < rec.GetLength())
    {
        Frame frame = rec.GetFrameAtIndex(auxIndex);
        if (frame != null)
        {
            go.transform.position = frame.GetPosition();
            go.transform.rotation = frame.GetRotation();
            go.transform.localScale = frame.GetScale();
        }
    }

    // Update activation state after setting transforms
    go.SetActive(IsRecordActiveInReplay(rec, index));
}



        void CheckDeletedObjects(Record rec)
        {
            //the deletion of the record is already out of the replay
            if (rec.GetRecordDeletedFrame() == 0)
            {
                //DELETE GAMEOBJECT
                GameObject delGO = rec.GetDeletedGO();
                Record r = delGO.GetComponent<Record>();
                if (r != null)
                    records.Remove(r);

                RemoveRecordsFromList(delGO);
                DeletedPool.Remove(delGO);
                Destroy(delGO);
            }
        }

        //Function that checks in the given frame (index), if the record is active
        bool IsRecordActiveInReplay(Record rec, int index)
        {
            bool ret = false;

            int instantiatedFrame = rec.GetFirstFrameIndex();
            int deletedFrame = rec.GetRecordDeletedFrame();

            //it has not been instantiated neither deleted
            if (rec.IsInstantiated() == false && deletedFrame == -1)
            {
                ret = true;
            }
            //it has been instantiated and deleted
            else if (rec.IsInstantiated() && deletedFrame != -1)
            {
                if (index >= instantiatedFrame && index < deletedFrame)
                    ret = true;
            }
            //it has been only instantiated
            else if (rec.IsInstantiated())
            {
                if (index >= instantiatedFrame)
                    ret = true;
            }
            //it has been only deleted
            else if (deletedFrame != -1)
            {
                if (index < deletedFrame)
                    ret = true;
            }

            return ret;
        }


        //Custom function to delete gameobjects that are recorded.
        //REALLY IMPORTANT to use this function if the deleted GO is using a record component
        public void DestroyRecordedGO(GameObject obj)
        {
            DeletedPool.Add(obj);
            obj.SetActive(false);

            Record r = obj.GetComponent<Record>();
            if (r != null)
            {
                r.SetRecordDeletedFrame(GetReplayLength() - 1);
                r.SetDeletedGameObject(obj);
            }

            SetDeleteChildrenRecords(obj, obj);
        }

        //Set deleted frame and go deleted to childs with also records
        private void SetDeleteChildrenRecords(GameObject deletedGO, GameObject obj)
        {
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                GameObject child = obj.transform.GetChild(i).gameObject;

                Record r = child.GetComponent<Record>();
                if (r != null)
                {
                    r.SetRecordDeletedFrame(GetReplayLength() - 1);
                    r.SetDeletedGameObject(deletedGO);
                }

                SetDeleteChildrenRecords(deletedGO, child);
            }
        }

        //function to remove all the deleted records from the list of records
        private void RemoveRecordsFromList(GameObject obj)
        {
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                GameObject child = obj.transform.GetChild(i).gameObject;
                Record r = child.GetComponent<Record>();
                if (r != null)
                    records.Remove(r);

                RemoveRecordsFromList(child);
            }
        }

        //------------------------------------ END OF INSTANTIATION AND DELETION METHODS -------------------------------------//

        //Add record to records list
        public void AddRecord(Record r)
        {
            records.Add(r);
        }

        //Get max length of recordable frames
        public int GetMaxLength()
        {
            return recordMaxLength;
        }

        public int GetAnimatorRecordLength()
        {
            int ret = recordMaxLength;

            if (interpolation)
                ret = recordMaxLength * Application.targetFrameRate * (int)recordInterval;
            return ret;
        }

        //Actual replay length
        public int GetReplayLength()
        {
            int value = 0;

            for (int i = 0; i < records.Count; i++)
                if (records[i].GetLength() > value)
                    value = records[i].GetLength();

            return value;
        }

        //Return if in replayMode or not
        public bool ReplayMode()
        {
            return isReplayMode;
        }

        //set transforms from the frame at record[index]
        void SetTransforms(Record rec, int index)
        {
            GameObject go = rec.GetGameObject();

            Frame f = rec.GetFrameAtIndex(index);
            if (f == null) return;

            go.transform.position = f.GetPosition();
            go.transform.rotation = f.GetRotation();
            go.transform.localScale = f.GetScale();
        }

        void InterpolateTransforms(Record rec, int index, float value)
        {
            GameObject go = rec.GetGameObject();

            Frame actual = rec.GetFrameAtIndex(index);
            Frame next = rec.GetFrameAtIndex(index + 1);
            if (actual == null || next == null) return;

            go.transform.position = Vector3.Lerp(actual.GetPosition(), next.GetPosition(), value);
            go.transform.rotation = Quaternion.Lerp(actual.GetRotation(), next.GetRotation(), value);
            go.transform.localScale = Vector3.Lerp(actual.GetScale(), next.GetScale(), value);
        }



        public void InstantiateReplayCamera()
        {
            // Create new camera object
            GameObject replayCam = new GameObject("ReplayCamera");
            
            // Add and configure camera component
            replayCameraComponent = replayCam.AddComponent<Camera>();
            replayCameraComponent.CopyFrom(mainCamera);
            replayCameraComponent.depth = mainCamera.depth + 1;

        }





        //Slider event: has been clicked
        public void SliderClick()
        {
            usingSlider = true;
        }

        //Slider event: has been released
        public void SliderRelease()
        {
            //set frame to slider value
            frameIndex = (int)timeLine.value;
            replayTimer = 0;

            for (int i = 0; i < records.Count; i++)
            {
                //Check for instantiated and deleted GO
                int auxIndex = frameIndex - records[i].GetFirstFrameIndex();
                HandleDeletedObjects(records[i], frameIndex);
                HandleInstantiatedObjects(records[i], auxIndex);

                if (IsRecordActiveInReplay(records[i], frameIndex))
                {
                    SetTransforms(records[i], auxIndex);

                    Animator animator = records[i].GetAnimator();
                    if (animator != null)
                    {
                        float time = animator.recorderStartTime + (animator.recorderStopTime - animator.recorderStartTime) / records[i].GetLength() * auxIndex;

                        if (time > animator.recorderStopTime)
                            time = animator.recorderStopTime;

                        animator.playbackTime = time;
                    }
                }
            }

            usingSlider = false;
        }



        //------------- REPLAY TOOLS -------------------//

        // Start replay mode
        public void EnterReplayMode()
        {    
             isReplayMode = true;

            if (records.Count == 0)
         {
        Debug.LogWarning("No replay data to display!");
        return;
    }

                   //   InstantiateReplayCamera();
            //initial frameIndex 
            frameIndex = 0;

            //slider max value
            timeLine.maxValue = GetReplayLength();
            timeLine.value = frameIndex;

            //Enable UI
            UIvisibility(true);

            state = ReplayState.PAUSE;
            Time.timeScale = 0f;
            speedIndex = 2;

            //set gameobjects states to starting frame
            for (int i = 0; i < records.Count; i++)
            {
                records[i].SetKinematic(true);
                records[i].ManageScripts(false);

                int auxIndex = frameIndex - records[i].GetFirstFrameIndex();

                if (IsRecordActiveInReplay(records[i], frameIndex))
                {
                    SetTransforms(records[i], auxIndex);

                    //animations
                    Animator animator = records[i].GetAnimator();
                    if (animator != null)
                    {
                        //stop recording animator
                        animator.StopRecording();

                        //start animator replayMode
                        animator.StartPlayback();
                        animator.playbackTime = animator.recorderStartTime;
                    }
                }

                //Check for instantiated and deleted GO
                HandleInstantiatedObjects(records[i], auxIndex);
                HandleDeletedObjects(records[i], frameIndex);
            }
        }

        //Exit replay mode
        public void QuitReplayMode()
        {
            //destroy deleted gameobject and records
            foreach (GameObject go in DeletedPool)
            {
                Record r = go.GetComponent<Record>();
                if (r != null)
                    records.Remove(r);

                RemoveRecordsFromList(go);
                Destroy(go);
            }
            DeletedPool.Clear();

            //set gameobjects transforms back to current state
            for (int i = 0; i < records.Count; i++)
            {
                records[i].SetKinematic(false);
                records[i].ManageScripts(true);

                //Check for instantiated GO
                HandleInstantiatedObjects(records[i], records[i].GetLength() - 1);

                //reset transforms
                SetTransforms(records[i], records[i].GetLength() - 1);

                //reset rigidBody velocities
                Rigidbody rb = records[i].GetRigidbody();
                if (rb != null)
                {
                    rb.linearVelocity = records[i].GetFrameAtIndex(records[i].GetLength() - 1).GetRBVelocity();
                    rb.angularVelocity = records[i].GetFrameAtIndex(records[i].GetLength() - 1).GetRBAngularVelocity();
                }

                //reset animations
                Animator animator = records[i].GetAnimator();
                if (animator != null)
                {
                    animator.playbackTime = animator.recorderStopTime;
                    animator.StopPlayback();
                    records[i].SetStartRecording(false);
                }
                records[i].ClearFrameList();
            }

            //Disable UI
            UIvisibility(false);


            isReplayMode = false;

            //optional
            Time.timeScale = 1f;
        }

        //Start replay from begining
        public void RestartReplay()
        {
            frameIndex = 0;
            timeLine.value = frameIndex;

            for (int i = 0; i < records.Count; i++)
            {
                int auxIndex = frameIndex - records[i].GetFirstFrameIndex();

                //Check for instantiated and deleted GO
                HandleDeletedObjects(records[i], frameIndex);
                HandleInstantiatedObjects(records[i], auxIndex);

                if (IsRecordActiveInReplay(records[i], frameIndex))
                {
                    SetTransforms(records[i], auxIndex);

                    //animations
                    Animator animator = records[i].GetAnimator();
                    if (animator != null)
                    {
                        animator.playbackTime = animator.recorderStartTime;
                    }
                }
            }
        }

        //Pause / Resume function
        public void PauseResume()
        {
            if (state == ReplayState.PAUSE)
            {
                state = ReplayState.PLAYING;
                Time.timeScale = 1;
            }
            else
            {
                state = ReplayState.PAUSE;
                Time.timeScale = 0;
            }
        }

        //Advances one frame 
        public void GoForward()
        {
            state = ReplayState.PAUSE;
            Time.timeScale = 0;

            if (frameIndex < recordMaxLength - 1)
            {
                if (interpolation)
                {
                    replayTimer++;

                    if (replayTimer >= Application.targetFrameRate * recordInterval)
                    {
                        replayTimer = 0;
                        frameIndex++;
                    }
                }
                else
                {
                    frameIndex++;
                }

                timeLine.value = frameIndex;

                for (int i = 0; i < records.Count; i++)
                {
                    //Check for instantiated and deleted GO
                    HandleDeletedObjects(records[i], frameIndex);
                    HandleInstantiatedObjects(records[i], frameIndex - records[i].GetFirstFrameIndex());
                    int auxIndex = frameIndex - records[i].GetFirstFrameIndex();

                    if (IsRecordActiveInReplay(records[i], frameIndex))
                    {
                        if (interpolation)
                        {
                            float max = Application.targetFrameRate * recordInterval;
                            float value = replayTimer / max;
                            InterpolateTransforms(records[i], auxIndex, value);
                        }
                        else
                            SetTransforms(records[i], auxIndex);

                        //animations
                        Animator animator = records[i].GetAnimator();
                        if (animator != null)
                        {
                            float time = (animator.recorderStopTime - animator.recorderStartTime) / records[i].GetLength();

                            if (interpolation)
                                time = (animator.recorderStopTime - animator.recorderStartTime) / records[i].GetAnimFramesRecorded();

                            animator.playbackTime += time;
                        }
                    }
                }
            }
        }

        //Back one frame
        public void GoBack()
        {
            state = ReplayState.PAUSE;
            Time.timeScale = 0;

            if (frameIndex > 0)
            {

                if (interpolation)
                {
                    replayTimer--;

                    if (replayTimer <= 0)
                    {
                        replayTimer = Application.targetFrameRate * recordInterval;
                        frameIndex--;
                    }
                }
                else
                {
                    frameIndex--;
                }

                timeLine.value = frameIndex;

                for (int i = 0; i < records.Count; i++)
                {
                    //Check for instantiated and deleted GO
                    HandleDeletedObjects(records[i], frameIndex);
                    HandleInstantiatedObjects(records[i], frameIndex - records[i].GetFirstFrameIndex());
                    int auxIndex = frameIndex - records[i].GetFirstFrameIndex();

                    if (IsRecordActiveInReplay(records[i], frameIndex))
                    {
                        if (interpolation)
                        {
                            float max = Application.targetFrameRate * recordInterval;
                            float value = replayTimer / max;
                            InterpolateTransforms(records[i], auxIndex, value);
                        }
                        else
                            SetTransforms(records[i], auxIndex);

                        //animations
                        Animator animator = records[i].GetAnimator();
                        if (animator != null)
                        {
                            float time = (animator.recorderStopTime - animator.recorderStartTime) / records[i].GetLength();

                            if (interpolation)
                                time = (animator.recorderStopTime - animator.recorderStartTime) / records[i].GetAnimFramesRecorded();

                            animator.playbackTime -= time;
                        }
                    }
                }
            }
        }

        //Increase replay speed
        public void SpeedUp()
        {
            if (speedIndex < speeds.Length - 1)
                speedIndex++;
            Time.timeScale = speeds[speedIndex];
        }

        //Decrease replay speed
        public void SpeedDown()
        {
            if (speedIndex > 0)
                speedIndex--;
            Time.timeScale = speeds[speedIndex];
        }

        //Change to next camera in scene
        public void NextCamera()
        {
            for (int i = 0; i < cameras.Length; i++)
            {
                if (cameras[i] == Camera.main)
                    cameraIndex = i;
            }

            cameraIndex++;

            if (cameras.Length == cameraIndex)
            {
                cameraIndex = 0;
                cameras[cameras.Length - 1].enabled = false;
                cameras[cameraIndex].enabled = true;
            }
            else
            {
                cameras[cameraIndex - 1].enabled = false;
                cameras[cameraIndex].enabled = true;
            }
        }

        //Change to previous camera in scene
        public void PreviousCamera()
        {
            for (int i = 0; i < cameras.Length; i++)
            {
                if (cameras[i] == Camera.main)
                    cameraIndex = i;
            }

            cameraIndex--;

            if (cameraIndex < 0)
            {
                cameraIndex = cameras.Length - 1;
                cameras[0].enabled = false;
                cameras[cameraIndex].enabled = true;
            }
            else
            {
                cameras[cameraIndex + 1].enabled = false;
                cameras[cameraIndex].enabled = true;
            }
        }

        // visibility UI of replay
        public void UIvisibility(bool b)
        {
            replayBoxUI.SetActive(b);
        }



        //------------------------------------------------------------------------
        //------------------- TRAVEL BACK IN TIME FUNCTIONS ----------------------
        //------------------------------------------------------------------------

        float timerTravelBack = 0;
        bool travelBackTime = false;

        public void StartTravelBack(float time)
        {
            isReplayMode = true;
            timerTravelBack = time;
            travelBackTime = true;
            state = ReplayState.TRAVEL_BACK;
            speedIndex = 2;

            frameIndex = GetReplayLength();
            replayTimer = recordTimer;

            for (int i = 0; i < records.Count; i++)
            {
                //start playback animations
                Animator animator = records[i].GetAnimator();
                if (animator != null)
                {
                    //stop recording animator
                    animator.StopRecording();

                    //start animator replayMode
                    animator.StartPlayback();
                    animator.playbackTime = animator.recorderStopTime;
                }

                records[i].SetKinematic(true);
                records[i].ManageScripts(false);
            }
        }

        public void StartTravelBack()
        {
            isReplayMode = true;
            timerTravelBack = 1f;
            state = ReplayState.TRAVEL_BACK;
            speedIndex = 2;

            frameIndex = GetReplayLength();
            replayTimer = recordTimer;

            for (int i = 0; i < records.Count; i++)
            {
                //start playback animations
                Animator animator = records[i].GetAnimator();
                if (animator != null)
                {
                    //stop recording animator
                    animator.StopRecording();

                    //start animator replayMode
                    animator.StartPlayback();
                    animator.playbackTime = animator.recorderStopTime;
                }

                records[i].SetKinematic(true);
                records[i].ManageScripts(false);
            }
        }

        void TravelBack()
        {
            if (frameIndex > 0 && timerTravelBack > 0)
            {
                if (interpolation)
                {
                    replayTimer--;

                    if (replayTimer <= 0)
                    {
                        replayTimer = Application.targetFrameRate * recordInterval;
                        frameIndex--;
                    }
                }
                else
                {
                    frameIndex--;
                }

                for (int i = 0; i < records.Count; i++)
                {
                    //Check for instantiated and deleted GO
                    HandleDeletedObjects(records[i], frameIndex);
                    HandleInstantiatedObjects(records[i], frameIndex - records[i].GetFirstFrameIndex());
                    int auxIndex = frameIndex - records[i].GetFirstFrameIndex();

                    if (IsRecordActiveInReplay(records[i], frameIndex))
                    {
                        if (interpolation)
                        {
                            float max = Application.targetFrameRate * recordInterval;
                            float value = replayTimer / max;
                            InterpolateTransforms(records[i], auxIndex, value);
                        }
                        else
                            SetTransforms(records[i], auxIndex);

                        //animations
                        Animator animator = records[i].GetAnimator();
                        if (animator != null)
                        {
                            float time = (animator.recorderStopTime - animator.recorderStartTime) / records[i].GetLength();

                            if (interpolation)
                                time = (animator.recorderStopTime - animator.recorderStartTime) / records[i].GetAnimFramesRecorded();

                            time *= speeds[speedIndex];

                            animator.playbackTime -= time;
                        }
                    }
                }

                if (travelBackTime)
                    timerTravelBack -= Time.deltaTime;
            }
            else
            {
                ExitTravelBack();
            }
        }

        public void ExitTravelBack()
        {
            for (int i = 0; i < records.Count; i++)
            {
                //reset animations
                Animator animator = records[i].GetAnimator();
                if (animator != null)
                {
                    animator.StopPlayback();
                    records[i].SetStartRecording(false);
                }

                int auxIndex = frameIndex - records[i].GetFirstFrameIndex();

                records[i].SetKinematic(false);
                records[i].ManageScripts(true);

                //reset rigidBody velocities
                Rigidbody rb = records[i].GetRigidbody();
                if (rb != null && IsRecordActiveInReplay(records[i], frameIndex))
                {
                    rb.linearVelocity = records[i].GetFrameAtIndex(auxIndex).GetRBVelocity();
                    rb.angularVelocity = records[i].GetFrameAtIndex(auxIndex).GetRBAngularVelocity();
                }

                //handle deleted records 
                if (records[i].GetRecordDeletedFrame() != -1 && frameIndex < records[i].GetRecordDeletedFrame())
                {
                    DeletedPool.Remove(records[i].GetDeletedGO());
                }

                //handle instantiated records
                if (records[i].IsInstantiated() && frameIndex < records[i].GetFirstFrameIndex())
                {
                    DestroyRecordedGO(records[i].GetGameObject());
                }

                records[i].ClearFrameList();
            }

            foreach (GameObject go in DeletedPool)
            {
                Record r = go.GetComponent<Record>();
                if (r != null)
                    records.Remove(r);

                RemoveRecordsFromList(go);
                Destroy(go);
            }
            DeletedPool.Clear();

            state = ReplayState.PAUSE;
            travelBackTime = false;
            isReplayMode = false;
        }
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes

GameObject GetPrefabForType(string shipType)
{
    switch (shipType)
    {
        case "Cargo": return shipController.cargoPrefab;
        case "Patrol": return shipController.patrolPrefab;
        case "Pirate": return shipController.piratePrefab;
        default: return null;
<<<<<<< Updated upstream
    }

<<<<<<< Updated upstream
=======
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
>>>>>>> Stashed changes
=======
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
>>>>>>> Stashed changes
}
