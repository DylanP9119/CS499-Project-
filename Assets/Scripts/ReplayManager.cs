using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;

public class ReplayManager : MonoBehaviour
{
    public static ReplayManager Instance;

    private Dictionary<int, List<ShipSnapshot>> tickSnapshots = new();
    private bool isRecording = true;

    public bool ReplayModeActive = false;
    public bool ReplayPaused = true;

    public float replayTime = 0f;
    public float replaySpeed = 1f;

    private int lastProcessedTick = -1;

    void Awake()
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
        if (ReplayModeActive && !ReplayPaused && TimeControl.Instance != null)
        {
            replayTime += Time.deltaTime * replaySpeed;
            int tick = Mathf.FloorToInt(replayTime / TimeControl.Instance.GetSpeed());

            if (tick != lastProcessedTick)
            {
                lastProcessedTick = tick;
                StepToTick(tick);
            }
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            if (ReplayModeActive && TimeControl.Instance != null)
            {
                replayTime -= TimeControl.Instance.GetSpeed();
                if (replayTime < 0f) replayTime = 0f;

                int tick = Mathf.FloorToInt(replayTime / TimeControl.Instance.GetSpeed());
                lastProcessedTick = -1; // Force replay to step again even if same tick
                StepToTick(tick);

                Debug.Log($"[REPLAY] Stepped backward to tick {tick}");
            }
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadReplayFromFile("replay.json");
            replayTime = 0f;
            lastProcessedTick = -1;
            ReplayPaused = true;
            ReplayModeActive = true;
            StepToTick(0);
            Debug.Log("[REPLAY] Loaded and paused replay at tick 0.");
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            SaveReplayToFile("replay.json");
            Debug.Log("[REPLAY] Saved snapshot replay to replay.json.");
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ReplayPaused = !ReplayPaused;
            Debug.Log($"[REPLAY] {(ReplayPaused ? "Paused" : "Playing")}");
        }
    }

    public void RecordSnapshotForTick(int tick, List<GameObject> allShips)
    {
        if (!isRecording) return;

        List<ShipSnapshot> snapshot = new();

        foreach (GameObject ship in allShips)
        {
            if (ship == null) continue;

            ShipSnapshot s = new ShipSnapshot
            {
                shipId = ExtractShipId(ship),
                shipType = ship.tag,
                position = ship.transform.position,
                rotation = ship.transform.rotation
            };

            snapshot.Add(s);
        }

        tickSnapshots[tick] = snapshot;
        Debug.Log($"[Snapshot] Recorded {snapshot.Count} ships at tick {tick}");
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
                return id;
        }
        return -1;
    }

    // Called externally to turn off recording when sim ends
    public void StopRecording()
    {
        isRecording = false;
    }

    // Optional access for replay later
    public Dictionary<int, List<ShipSnapshot>> GetSnapshots() => tickSnapshots;

    public void StepToTick(int tick)
    {
        if (!tickSnapshots.ContainsKey(tick))
        {
            Debug.LogWarning($"[Replay] No snapshot recorded for tick {tick}");
            return;
        }

        // Clear all current ships
        ShipController.Instance.ClearAllShips();

        // Load snapshot for this tick
        List<ShipSnapshot> snapshot = tickSnapshots[tick];

        foreach (ShipSnapshot s in snapshot)
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

        Debug.Log($"[Replay] Rebuilt tick {tick} with {snapshot.Count} ships.");
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

    public void SaveReplayToFile(string fileName)
    {
        ReplaySnapshotData data = new();
        foreach (var pair in tickSnapshots)
        {
            TickSnapshot t = new TickSnapshot();
            t.tick = pair.Key;
            t.ships = pair.Value;
            data.timeline.Add(t);
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(Path.Combine(Application.persistentDataPath, fileName), json);
    }

    public void LoadReplayFromFile(string fileName)
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);
        if (!File.Exists(path))
        {
            Debug.LogWarning("[REPLAY] File not found: " + path);
            return;
        }

        string json = File.ReadAllText(path);
        ReplaySnapshotData data = JsonUtility.FromJson<ReplaySnapshotData>(json);

        tickSnapshots.Clear();
        foreach (TickSnapshot tick in data.timeline)
        {
            tickSnapshots[tick.tick] = tick.ships;
        }

        Debug.Log($"[REPLAY] Loaded {tickSnapshots.Count} ticks from {fileName}");
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
// patrol never change rotation
// should just get rotation changes when it happens
// only if capture happens
// so change rotation when pirate and cargo are equal position
[System.Serializable]
public class ReplayData
{
    public List<ReplayEvent> ets = new();
}

[System.Serializable]
public class ShipSnapshot
{
    public int sID;
    public string st;
    public Vector3 p;
    public Quaternion r;
}

[System.Serializable]
public class TickSnapshot
{
    public int t;
    public List<ShipSnapshot> ships = new();
}

[System.Serializable]
public class ReplaySnapshotData
{
    public List<TickSnapshot> tl = new();
}