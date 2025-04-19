using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class UIControllerScript : MonoBehaviour
{

    [SerializeField] private string mainMenu = "mainMenu";
    [SerializeField] private string startSim = "MainScene";
    //buttons
    public Button nightCaptureButton;
    public TMP_Text buttonText;
    public bool nightCaptureEnabled = false;

    public Button beginButton;
    public bool canStart = false;

    public Button saveFileName;
    public string fileNameString;

    //input fields
    private string gridDayString;
    private string gridNightString;

    public double[] cargoGridPercentsD = new double[100];
    public double[] patrolGridPercentsD = new double[100];
    public double[] pirateGridPercentsD = new double[400];

    public double[] cargoGridPercentsN = new double[100];
    public double[] patrolGridPercentsN = new double[100];
    public double[] pirateGridPercentsN = new double[400];

    double multiplier = 0;
    int gridMinimum = 0;
    int gridMaximum = 0;

    public int cargoDayPercent;
    public int cargoNightPercent;
    public int pirateDayPercent;
    public int pirateNightPercent;
    public int patrolDayPercent;
    public int patrolNightPercent;

    public int dayCount;
    public int hourCount;
    public int minuteCount;

    private bool isParsed;
    
    public GameObject errorPanel;
    public TMP_Text errorPanelText;

    public Slider cargoDaySlider;
    public Slider pirateDaySlider;
    public Slider patrolDaySlider;

    public Slider cargoNightSlider;
    public Slider pirateNightSlider;
    public Slider patrolNightSlider;

    public TMP_Text cargoDayText;
    public TMP_Text pirateDayText;
    public TMP_Text patrolDayText;
    public TMP_Text cargoNightText;
    public TMP_Text pirateNightText;
    public TMP_Text patrolNightText;

    public static UIControllerScript Instance;

    // THINGS TO DO ON PROGRAM START
    public void Start()
    {
        
    }

    public void Awake()
    {

        Instance = this;

        //Defaults
        fileNameString = "MySimulation";

        cargoDayPercent = 50;
        cargoNightPercent = 50;
        pirateDayPercent = 25;
        pirateNightPercent = 25;
        patrolDayPercent = 40;
        patrolNightPercent = 40;

        dayCount = 0;
        hourCount = 24;
        minuteCount = 0;

        canStart = false;

        //Buttons
        buttonText.text = "DISABLED";
        nightCaptureEnabled = false;

        cargoNightSlider.interactable = false;
        patrolNightSlider.interactable = false;
        pirateNightSlider.interactable = false;

        errorPanel.SetActive(false);

        //fill grid spaces with default values
        for (int gridSpace = 0; gridSpace < cargoGridPercentsD.Length; gridSpace++)
        {
            cargoGridPercentsD[gridSpace] = 1;
            patrolGridPercentsD[gridSpace] = 1;
            cargoGridPercentsN[gridSpace] = 1;
            patrolGridPercentsN[gridSpace] = 1;
        }
        for (int gridSpace = 0; gridSpace < pirateGridPercentsD.Length; gridSpace++)
        {
            pirateGridPercentsD[gridSpace] = 1;
            pirateGridPercentsN[gridSpace] = 1;
        }
    }

    //back button
    public void BackButton()
    {
        SceneManager.LoadScene(mainMenu);
    }

    public void ErrorOKButton() {
        errorPanel.SetActive(false);
    }

    // TOGGLE DECREASED NIGHT TIME CAPTURE, ONLY WHEN CERTAIN BUTTON IS PRESSED.
    public void NightCaptureToggle()
    {

        nightCaptureEnabled = !nightCaptureEnabled;

        if (nightCaptureEnabled)
        {
            buttonText.text = "ENABLED";
        }
        else
        {
            buttonText.text = "DISABLED";
        }

        cargoNightSlider.interactable = nightCaptureEnabled;
        pirateNightSlider.interactable = nightCaptureEnabled;
        patrolNightSlider.interactable = nightCaptureEnabled;
    }

    // START THE PROGRAM, TOGGLES WHEN "BEGIN" BUTTON IS PRESSED"
    public void BeginToggle()
    {
        canStart = true;

        Debug.Log("TEST : " + canStart);
        minuteCount = (hourCount * 60) + (dayCount * 24 * 60);

        if ((dayCount < 0 || hourCount < 0) || (minuteCount < 720 || minuteCount > 43200)) {
            canStart = false;
            errorPanel.SetActive(true);
            errorPanelText.text = "Error Starting: Day and/or hour count is invalid. Ensure your parameters are between 12 hours and 30 days.";
        }

        if (fileNameString == "") {
            canStart = false;
            errorPanel.SetActive(true);
            errorPanelText.text = "Error Starting: File name is blank. Please enter a value for your file name.";
        }

        /*
        Grid Syntax Checking goes here
        */
        //read from textboxes
        string[] values = new string[4];
        Debug.Log("TEST 2: " + canStart);
        //declare separated values
        //For day values...
        if (gridDayString != null && canStart) {
            Debug.Log("Grid Day String Loop is Running");
            string[] gridLinesDay = gridDayString.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in gridLinesDay)
            {


                if (!canStart)
                    break;

                Debug.Log("Entered Day Loop Successfully");
                values = line.Split(',');

                Debug.Log(values[0] + " " + values[1] + " " + values[2] + " " + values[3]);

                canStart = GridRangeCheck(values[1], values[2], values[3]);

                if (!canStart) {
                    break;
                }
                else {
                    gridMinimum = Int32.Parse(values[1]);
                    gridMaximum = Int32.Parse(values[2]);
                    multiplier = Int32.Parse(values[3]);
                }

                switch (values[0])
                {
                    case "cargo":
                        if (gridMinimum < 1 || gridMaximum > 100) {
                            canStart = false;
                            Debug.Log("Invalid range of numbers for cargo day.");
                            break;
                        }

                        for (int i = gridMinimum - 1; i < gridMaximum; i++) {
                            cargoGridPercentsD[i] = cargoGridPercentsD[i] * multiplier;
                            Debug.Log(cargoGridPercentsD[i]);
                        }
                    break;
                    case "patrol":
                        if (gridMinimum < 1 || gridMaximum > 100) {
                            canStart = false;
                            Debug.Log("Invalid range of numbers for patrol day.");
                            break;
                        }

                        for (int i = gridMinimum - 1; i < gridMaximum; i++) {
                            patrolGridPercentsD[i] = patrolGridPercentsD[i] * multiplier;
                        }
                        break;
                    case "pirate":
                        if (gridMinimum < 1 || gridMaximum > 400) {
                            canStart = false;
                            Debug.Log("Invalid range of numbers for pirate day.");
                            break;
                        }

                        for (int i = gridMinimum - 1; i < gridMaximum; i++) {
                            pirateGridPercentsD[i] = pirateGridPercentsD[i] * multiplier;
                        }
                        break;
                    default:
                        Debug.Log("Invalid Entry for ship type in line: " + values[0]);
                        canStart = false;
                        break;
                }
                Debug.Log(values[0] + " " + values[1] + " " + values[2] + " " + values[3]);

            }
        }

        Debug.Log("CAN START: " + canStart);
        //For night values...
        if (gridNightString != null && canStart) {
            Debug.Log("Grid Night String Loop is Running");
            string[] gridLinesNight = gridNightString.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

            Debug.Log(gridNightString);
            foreach (string line in gridLinesNight)
            {
                
                Debug.Log("Entered Night Loop Successfully");
                values = line.Split(',');

                Debug.Log(values[0] + " " + values[1] + " " + values[2] + " " + values[3]);

                canStart = GridRangeCheck(values[1], values[2], values[3]);

                if (!canStart) {
                    break;
                }
                else {
                    gridMinimum = Int32.Parse(values[1]);
                    gridMaximum = Int32.Parse(values[2]);
                    multiplier = Int32.Parse(values[3]);
                }

                switch (values[0])
                {
                    case "cargo":
                        if (gridMinimum < 1 || gridMaximum > 100) {
                            canStart = false;
                            Debug.Log("Invalid range of numbers for patrol night.");
                            break;
                        }

                        for (int i = gridMinimum - 1; i < gridMaximum; i++) {
                            cargoGridPercentsN[gridMinimum] = cargoGridPercentsN[gridMinimum] * multiplier;
                        }
                        break;
                    case "patrol":
                        if (gridMinimum < 1 || gridMaximum > 100) {
                            canStart = false;
                            Debug.Log("Invalid range of numbers for patrol night.");
                            break;
                        }

                        for (int i = gridMinimum - 1; i < gridMaximum; i++) {
                            patrolGridPercentsN[gridMinimum] = patrolGridPercentsN[gridMinimum] * multiplier;
                        }
                        break;
                    case "pirate":
                        if (gridMinimum < 1 || gridMaximum > 400) {
                            canStart = false;
                            Debug.Log("Invalid range of numbers for pirate night.");
                            break;
                        }

                        for (int i = gridMinimum - 1; i < gridMaximum; i++) {
                            pirateGridPercentsN[gridMinimum] = pirateGridPercentsN[gridMinimum] * multiplier;
                        }
                        break;
                    default:
                        Debug.Log("Invalid Entry for ship type in line: " + values[0]);
                        canStart = false;
                        break;
                }
                Debug.Log(values[0] + " " + values[1] + " " + values[2]);
            }
        }
        
        if (!canStart)
        {
            Debug.Log("Invalid text entry, please check again.");
        }
        else
        {
            Debug.Log("SUCCESS!");
            Save();
            SceneManager.LoadScene(startSim);
        }

    }

    public class MyData
    {
        public string saveName;
        public int days, hours, cDay, cNight, piDay, piNight, paDay, paNight;
        public bool pNightCap;
    }

    public void Save()
    {
        MyData data = new MyData { 
            saveName = fileNameString,
            days = dayCount,
            hours = hourCount,
            cDay = cargoDayPercent,
            cNight = cargoNightPercent,
            piDay = pirateDayPercent,
            piNight = pirateNightPercent,
            paDay = patrolDayPercent,
            paNight = patrolNightPercent,
            pNightCap = nightCaptureEnabled,
            };
        
        string json = JsonUtility.ToJson(data, true);
        string path = fileNameString + ".json";
        DownloadFile(path, json);
    }

    public void DownloadFile(string filename, string content)
    {
    #if UNITY_WEBGL && !UNITY_EDITOR
        DownloadFileWebGL(filename, content);
    #else
        // In Editor or standalone builds, save locally for testing
        System.IO.File.WriteAllText(Application.dataPath + "/" + filename, content);
        Debug.Log("Saved locally: " + filename);
    #endif
    }

    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void DownloadFileWebGL(string filename, string content);

    public bool GridRangeCheck(string gridMin, string gridMax, string mult) {
        if (!(Int32.TryParse(gridMin, out int gridLow))) {
            Debug.Log("LOW CANT PARSE");
            return false;
        }
        
        else if(!(Int32.TryParse(gridMax, out int gridHigh))) {
            Debug.Log("HIGH CANT PARSE");
            return false;
        }
        
        else if (gridHigh < gridLow) {
            Debug.Log("MAX UNDER MIN CANT PARSE");
            return false;
        }

        else if (!(Double.TryParse(mult, out double probMult))) {
            Debug.Log("MULT UNDER MIN CANT PARSE");
            return false;
        }
	
	    return true;
    }
    
    //FILE HANDLER

    public void SaveFileName(string s) {
        fileNameString = s;
    }

    //GRID PROBABILITY HANDLING FUNCTIONS

    public void ReadStringInputGRIDDAYPERCENT(string s)
    {
        gridDayString = s;
        Debug.Log(gridDayString);
    }

    public void ReadStringInputGRIDNIGHTPERCENT(string s)
    {
        gridNightString = s;
        Debug.Log(gridNightString);
    }

    //ALL CARGO SET VALUES

    public void CargoDayPercentSlider(System.Single value)
    {
        cargoDayPercent = (int) value;
        cargoDayText.text = "Day: " + value + "%";

    }

    public void CargoNightPercentSlider(System.Single value)
    {
        cargoNightPercent = (int) value;
        cargoNightText.text = "Night: " + value + "%";

    }

    public void PirateDayPercentSlider(System.Single value)
    {
        pirateDayPercent = (int) value;
        pirateDayText.text = "Day: " + value + "%";

    }

    public void PirateNightPercentSlider(System.Single value)
    {
        pirateNightPercent = (int) value;
        pirateNightText.text = "Night: " + value + "%";

    }

    public void PatrolDayPercentSlider(System.Single value)
    {
        patrolDayPercent = (int) value;
        patrolDayText.text = "Day: " + value + "%";

    }

    public void PatrolNightPercentSlider(System.Single value)
    {
        patrolNightPercent = (int) value;
        patrolNightText.text = "Night: " + value + "%";

    }

    //ALL TIME VALUES
    public void ReadStringInputDAYS(string s)
    {
        isParsed = Int32.TryParse(s, out dayCount);

        //invalid entry
        if (!isParsed)
            dayCount = -1;

    }

    public void ReadStringInputHOURS(string s)
    {
        isParsed = Int32.TryParse(s, out hourCount);

        //invalid entry
        if (!isParsed)
            hourCount = -1;
    }
}
