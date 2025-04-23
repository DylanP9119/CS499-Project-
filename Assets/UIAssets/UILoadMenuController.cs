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

    public GameObject LoadPanel;
    public GameObject scrollContent;

    public void BackButton()
    {
        SceneManager.LoadScene(mainMenu);
    }

    public void PlayButton()
    {
        SceneManager.LoadScene(simMenu);
        DataPersistence.Instance.wasEnteredfromLoadScene = true;
    }

    [DllImport("__Internal")]
    private static extern void ShowFileUpload();

    public void OpenFileUpload()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
    ShowFileUpload();
#else
        Debug.Log("File upload only works in WebGL builds.");
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
        public List<ReplayEvent> events;
    }


    /*
    public void DebuggerFunction() {

        TextAsset jsonFile = Resources.Load<TextAsset>("mydata");

        string json = jsonFile.text;
        MyData data = JsonUtility.FromJson<MyData>(json);

        string inputText = "test,04/15/2025,10:27," + data.days + "," + data.hours + "," + data.pNightCap + "," + data.cDay + "," + data.cNight + ","
                             + data.piDay + "," + data.piNight + ","  + data.paNight + ","  + data.paNight;
        string gridText = "this is just test\ndata\n\ntesting";

        string[] values = inputText.Split(',');
        
        TMP_Text titleText = LoadPanel.transform.Find("Save Name Text").GetComponent<TMP_Text>();
        TMP_Text fileDateText = LoadPanel.transform.Find("Time Made").GetComponent<TMP_Text>();
        TMP_Text runtimeText = LoadPanel.transform.Find("Runtime").GetComponent<TMP_Text>();
        TMP_Text captureAndShipPercentsText = LoadPanel.transform.Find("Ship Percents").GetComponent<TMP_Text>();
        TMP_Text gridPercentsText = scrollContent.GetComponentInChildren<TMP_Text>();

        titleText.text = values[0];
        fileDateText.text = "Created on:\n" + values[1] + ", " + values[2];
        runtimeText.text = "Days: " + values[3] + "\nHours: " + values[4];
        captureAndShipPercentsText.text = "2x2 Pirate Night Capture: " + values[5] + "\nCargo: " + values[6] + "% Day, " + values[7] + "% Night " + 
                                                                                     "\nPatrol: " + values[8] + "% Day, " + values[9] + "% Night " + 
                                                                                     "\nPirate: " + values[10] + "% Day, " + values[11] + "% Night ";
        
        gridPercentsText.text = gridText;
    }
    */
}
