using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using TMPro;
using System.IO;


public class UILoadMenuController : MonoBehaviour
{
    [SerializeField] private string mainMenu = "mainMenu";
    [SerializeField] private string simMenu = "MainScene";
    public UIControllerScript UI;
    public GameObject LoadPanel;
    public GameObject scrollContent;
    public void BackButton()
    {
        SceneManager.LoadScene(mainMenu);
    }

    public void PlayButton()
    {
        DataPersistence.Instance.wasEnteredfromLoadScene = true;
        SceneManager.LoadScene(simMenu);
        
    }


    [DllImport("__Internal")]
    public static extern void ShowFileUpload();

    public void OpenFileUpload()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
    ShowFileUpload();
#else
        Debug.Log("File upload only works in WebGL builds.");
#endif

    }

    public void DownloadFileWithName(string fileName, string data)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        UI.DownloadFile(fileName, data);
#else
    //    string path = Path.Combine(Application.dataPath, fileName);
   //     File.WriteAllText(path, data);
        Debug.Log($"Not saved with WebGL");
#endif
    }
    // Called from JavaScript with the uploaded JSON
    public void OnJsonFileLoaded(string json)
    {
        MyData data = JsonUtility.FromJson<MyData>(json);
        // Save header data to DataPersistence
        DataPersistence.Instance.fileNameString = data.saveName;
        DataPersistence.Instance.dayCount = data.days;
        DataPersistence.Instance.hourCount = data.hours;
        DataPersistence.Instance.cargoDayPercent = data.cDay;
        DataPersistence.Instance.cargoNightPercent = data.cNight;
        DataPersistence.Instance.pirateDayPercent = data.piDay;
        DataPersistence.Instance.pirateNightPercent = data.piNight;
        DataPersistence.Instance.patrolDayPercent = data.paDay;
        DataPersistence.Instance.patrolNightPercent = data.paNight;
        DataPersistence.Instance.nightCaptureEnabled = data.pNightCap;
        DataPersistence.Instance.maxTick = data.mTick;
        // Save replay events
        DataPersistence.Instance.replayEvents = data.events;
        // Update UI
        string inputText = $"{data.saveName},{data.days},{data.hours},{data.pNightCap}," +
                           $"{data.cDay},{data.cNight},{data.piDay},{data.piNight},{data.paDay},{data.paNight}";
        string gridText = "this is just test\ndata\n\ntesting";

        string[] values = inputText.Split(',');

        TMP_Text titleText = LoadPanel.transform.Find("Save Name Text").GetComponent<TMP_Text>();
        TMP_Text runtimeText = LoadPanel.transform.Find("Runtime").GetComponent<TMP_Text>();
        TMP_Text captureAndShipPercentsText = LoadPanel.transform.Find("Ship Percents").GetComponent<TMP_Text>();
        TMP_Text gridPercentsText = scrollContent.GetComponentInChildren<TMP_Text>();

        titleText.text = values[0];
        runtimeText.text = "Days: " + values[1] + "\nHours: " + values[2];
        captureAndShipPercentsText.text = $"2x2 Pirate Night Capture: {values[3]}\n" +
                                        $"Cargo: {values[4]}% Day, {values[5]}% Night\n" +
                                        $"Patrol: {values[8]}% Day, {values[9]}% Night\n" +
                                        $"Pirate: {values[6]}% Day, {values[7]}% Night";
        gridPercentsText.text = gridText;
    }

    [System.Serializable]
    public class MyData
    {
        public string saveName;
        public int days;
        public int hours;
        public int cDay;
        public int cNight;
        public int piDay;
        public int piNight;
        public int paDay;
        public int paNight;
        public bool pNightCap;
        public int mTick;
        public List<ReplayEvent> events;
    }
}
