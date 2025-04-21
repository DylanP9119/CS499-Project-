using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
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

    public bool rawTextEditor;

    public Button saveFileName;
    public string fileNameString;

    //input fields
    private string gridDayString;
    private string gridNightString;

    public double[] cargoGridPercentsD;
    public double[] patrolGridPercentsD;
    public double[] pirateGridPercentsD;

    public double[] cargoGridPercentsN;
    public double[] patrolGridPercentsN;
    public double[] pirateGridPercentsN;

    public int cargoDayPercent;
    public int cargoNightPercent;
    public int pirateDayPercent;
    public int pirateNightPercent;
    public int patrolDayPercent;
    public int patrolNightPercent;

    public int dayCount;
    public int hourCount;
    public int minuteCount;
    private string path;
    private bool isParsed;
    
    public GameObject errorPanel;
    public TMP_Text errorPanelText;

    //SLIDERS FOR SHIP PERCENTAGES
    public Slider cargoDaySlider;
    public Slider pirateDaySlider;
    public Slider patrolDaySlider;

    public Slider cargoNightSlider;
    public Slider pirateNightSlider;
    public Slider patrolNightSlider;

    //TEXT FOR THE PERCENTAGES
    public TMP_Text cargoDayText;
    public TMP_Text pirateDayText;
    public TMP_Text patrolDayText;
    public TMP_Text cargoNightText;
    public TMP_Text pirateNightText;
    public TMP_Text patrolNightText;

    //VALUES FOR THE GRID PERCENTAGE ADDER
    public int dayTimeMinimumRange;
    public int dayTimeMaximumRange;
    public double dayTimeMultiplier;

    public int nightTimeMinimumRange;
    public int nightTimeMaximumRange;
    public double nightTimeMultiplier;

    public TMP_InputField dayGridPercentagesPanel;
    public TMP_InputField nightGridPercentagesPanel;

    public TMP_Dropdown dayShipSelection;
    public TMP_Dropdown nightShipSelection;

    public TMP_Dropdown removeOptionDay;
    public TMP_Dropdown removeOptionNight;

    public List<string> dayGridPercentList = new List<string>();
    public List<string> nightGridPercentList = new List<string>();

    public static UIControllerScript Instance;

    // THINGS TO DO ON PROGRAM START
    public void Start()
    {
        
    }

    public void Awake()
    {
        /*
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        */

        Instance = this;
        //DontDestroyOnLoad(gameObject);

        //if (Instance != null)
        //{
        //    Destroy(gameObject);
        //    return;
        //}

        //Instance = this;
        //DontDestroyOnLoad(gameObject);
        fileNameString = "Default";
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
        rawTextEditor = false;

        cargoNightSlider.interactable = false;
        patrolNightSlider.interactable = false;
        pirateNightSlider.interactable = false;

        errorPanel.SetActive(false);

        //fill grid spaces with default values

        cargoGridPercentsD = new double[101];
        patrolGridPercentsD = new double[101];
        pirateGridPercentsD = new double[401];

        cargoGridPercentsN = new double[101];
        patrolGridPercentsN = new double[101];
        pirateGridPercentsN = new double[401];
        ResetGrids();
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

        if (canStart) {
            InstantiateDayGridPercents();
            InstantiateNightGridPercents();
        }

        if (canStart) {
            Debug.Log("SUCCESS!");
            Save();
            DataPersistence.Instance.wasEnteredfromLoadScene = false;
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
        DownloadFile(path, json);
        DataPersistence.Instance.path = path;
        DataPersistence.Instance.fileNameString = path;
    }

    public void DownloadFile(string filename, string content)
    {
    #if UNITY_WEBGL && !UNITY_EDITOR
        DownloadFileWebGL(filename, content);
    #else
        // In Editor or standalone builds, save locally for testing
        path = Path.Combine(Application.persistentDataPath, DataPersistence.Instance.fileNameString + ".json");  
        DataPersistence.Instance.path = path;      
        System.IO.File.WriteAllText(path, content);
        Debug.Log("Saved locally: " + path);
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
        DataPersistence.Instance.fileNameString = fileNameString;
    }

    //GRID PROBABILITY HANDLING FUNCTIONS

    //Input Strings on top

    public void AddToBoxDay() {
        string shipText = dayShipSelection.options[dayShipSelection.value].text;
        int maxRange = 100;

        if (shipText == "pirate")
            maxRange = 400;

        if (dayTimeMaximumRange > maxRange || dayTimeMaximumRange < 0) {
            errorPanel.SetActive(true);
            errorPanelText.text = "Error Adding: Max range is invalid. It must be an integer between 0 and " + maxRange + ".";
        }
        else if (dayTimeMinimumRange > maxRange || dayTimeMinimumRange < 0) {
            errorPanel.SetActive(true);
            errorPanelText.text = "Error Adding: Min range is invalid. It must be an integer between 0 and " + maxRange + ".";
        }
        else if (dayTimeMinimumRange > dayTimeMaximumRange) {
            errorPanel.SetActive(true);
            errorPanelText.text = "Error Adding: Please make min range equal to or smaller than your max range.";
        }
        else if (dayTimeMultiplier < 0) {
            errorPanel.SetActive(true);
            errorPanelText.text = "Error Adding: Ensure your multipler is a decimal zero or greater.";
        }
        else {
            string lineText = shipText + "," + dayTimeMinimumRange + "," + dayTimeMaximumRange + "," + dayTimeMultiplier;
            dayGridPercentagesPanel.text = dayGridPercentagesPanel.text + lineText + "\n";
            dayGridPercentList.Add(lineText);

            TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData(lineText);
            removeOptionDay.options.Add(optionData);

            removeOptionDay.RefreshShownValue();
        }
    }

    public void RemoveFromDayBox() {
        string currentSelectedText = removeOptionDay.options[removeOptionDay.value].text;
        bool textFound = false;

        for (int i = 0; i < dayGridPercentList.Count; i++)
        {
            Debug.Log(dayGridPercentList[i]);
            Debug.Log(currentSelectedText);
            if (textFound) {
                dayGridPercentList[i - 1] = dayGridPercentList[i];
            }
            else if (currentSelectedText == dayGridPercentList[i]) {
                textFound = true;
            }
        }

        dayGridPercentList.RemoveAt(dayGridPercentList.Count - 1);
        
        dayGridPercentagesPanel.text = "";
        removeOptionDay.ClearOptions();

        for (int i = 0; i < dayGridPercentList.Count; i++) {
            dayGridPercentagesPanel.text = dayGridPercentagesPanel.text + dayGridPercentList[i] + "\n";

            TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData(dayGridPercentList[i]);
            removeOptionDay.options.Add(optionData);
        }

        removeOptionDay.RefreshShownValue();
    }

    public void AddToBoxNight() {
        string shipText = nightShipSelection.options[nightShipSelection.value].text;
        int maxRange = 100;

        if (shipText == "pirate")
            maxRange = 400;

        if (nightTimeMaximumRange > maxRange || nightTimeMaximumRange < 0) {
            errorPanel.SetActive(true);
            errorPanelText.text = "Error Adding: Max range is invalid. It must be an integer between 0 and 100.";
        }
        else if (nightTimeMinimumRange > maxRange || nightTimeMinimumRange < 0) {
            errorPanel.SetActive(true);
            errorPanelText.text = "Error Adding: Min range is invalid. It must be an integer between 0 and 100.";
        }
        else if (nightTimeMinimumRange > nightTimeMaximumRange) {
            errorPanel.SetActive(true);
            errorPanelText.text = "Error Adding: Please make min range equal to or smaller than your max range.";
        }
        else if (nightTimeMultiplier < 0) {
            errorPanel.SetActive(true);
            errorPanelText.text = "Error Adding: Ensure your multipler is a decimal zero or greater.";
        }
        else {
            string lineText = shipText + "," + nightTimeMinimumRange + "," + nightTimeMaximumRange + "," + nightTimeMultiplier;
            nightGridPercentagesPanel.text = nightGridPercentagesPanel.text + lineText + "\n";
            nightGridPercentList.Add(lineText);

            TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData(lineText);
            removeOptionNight.options.Add(optionData);

            removeOptionNight.RefreshShownValue();
        }
    }

    public void RemoveFromNightBox() {
        string currentSelectedText = removeOptionNight.options[removeOptionNight.value].text;
        bool textFound = false;

        for (int i = 0; i < nightGridPercentList.Count; i++)
        {
            if (textFound) {
                nightGridPercentList[i - 1] = nightGridPercentList[i];
            }
            else if (currentSelectedText == nightGridPercentList[i]) {
                textFound = true;
            }
        }

        nightGridPercentList.RemoveAt(nightGridPercentList.Count - 1);
        
        nightGridPercentagesPanel.text = "";
        removeOptionNight.ClearOptions();

        for (int i = 0; i < nightGridPercentList.Count; i++) {
            nightGridPercentagesPanel.text = nightGridPercentagesPanel.text + nightGridPercentList[i] + "\n";

            TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData(nightGridPercentList[i]);
            removeOptionNight.options.Add(optionData);
        }

        removeOptionDay.RefreshShownValue();
    }

    public void ReadMinimumStringDay(string s) {
        isParsed = Int32.TryParse(s, out dayTimeMinimumRange);

        if (!isParsed) {
            dayTimeMinimumRange = -1;
        }
    }

    public void ReadMaximumStringDay(string s) {
        isParsed = Int32.TryParse(s, out dayTimeMaximumRange);

        if (!isParsed) {
            isParsed = Int32.TryParse(s, out dayTimeMaximumRange);
        }
    }

    public void ReadMinimumStringNight(string s) {
        isParsed = Int32.TryParse(s, out nightTimeMinimumRange);

        if (!isParsed) {
            nightTimeMinimumRange = -1;
        }
    }

    public void ReadMaximumStringNight(string s) {
        isParsed = Int32.TryParse(s, out nightTimeMaximumRange);

        if (!isParsed) {
            nightTimeMaximumRange = -1;
        }
    }

    public void ReadMultiplierDay(string s) {
        isParsed = Double.TryParse(s, out dayTimeMultiplier);

        if (!isParsed) {
            dayTimeMultiplier = -1;
        }
    }

    public void ReadMultiplierNight(string s) {
        isParsed = Double.TryParse(s, out nightTimeMultiplier);

        if (!isParsed) {
            dayTimeMultiplier = -1;
        }
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

    public void InstantiateDayGridPercents() {
        foreach (string line in dayGridPercentList) {
            string[] values = line.Split(',');

            int gridMinimum = Int32.Parse(values[1]);
            int gridMaximum = Int32.Parse(values[2]);
            double multiplier = Double.Parse(values[3]);

            if (values[0] == "cargo") {
                for (int i = gridMinimum; i <= gridMaximum; i++) {
                    cargoGridPercentsD[i] = cargoGridPercentsD[i] * multiplier;
                    Debug.Log(cargoGridPercentsD[i] + " at " + i);
                }
            }
            else if (values[0] == "pirate") {
                for (int i = gridMinimum; i <= gridMaximum; i++) {
                    pirateGridPercentsD[i] = pirateGridPercentsD[i] * multiplier;
                }
            }
            else if (values[0] == "patrol") {
                for (int i = gridMinimum; i <= gridMaximum; i++) {
                    patrolGridPercentsD[i] = patrolGridPercentsD[i] * multiplier;
                }
            }
            else {
                errorPanel.SetActive(true);
                errorPanelText.text = "Error starting: A ship type in the Day Grid is not right!: \"" + values[0] + "\". Be careful using raw text editor!";
                canStart = false;
                break;
            }
        }
    }

    public void InstantiateNightGridPercents() {
        foreach (string line in nightGridPercentList) {
            string[] values = line.Split(',');

            int gridMinimum = Int32.Parse(values[1]);
            int gridMaximum = Int32.Parse(values[2]);
            double multiplier = Double.Parse(values[3]);

            if (values[0] == "cargo") {
                for (int i = gridMinimum; i <= gridMaximum; i++) {
                    cargoGridPercentsN[i] = cargoGridPercentsN[i] * multiplier;
                }
            }
            else if (values[0] == "pirate") {
                for (int i = gridMinimum; i <= gridMaximum; i++) {
                    pirateGridPercentsN[i] = pirateGridPercentsN[i] * multiplier;
                }
            }
            else if (values[0] == "patrol") {
                for (int i = gridMinimum; i <= gridMaximum; i++) {
                    patrolGridPercentsN[i] = patrolGridPercentsN[i] * multiplier;
                }
            }
            else {
                errorPanel.SetActive(true);
                errorPanelText.text = "Error starting: A ship type in the Night Grid is not right!: \"" + values[0] + "\". Be careful using raw text editor!";
                canStart = false;
                break;
            }
        }
    }

    public void ResetGrids() {

        for (int i = 0; i < cargoGridPercentsD.Length; i++)
        {   
            cargoGridPercentsD[i] = 1;
            patrolGridPercentsD[i] = 1;
            cargoGridPercentsN[i] = 1;
            patrolGridPercentsN[i] = 1;
        }

        for (int i = 0; i < pirateGridPercentsD.Length; i++)
        {
            pirateGridPercentsD[i] = 1;
            pirateGridPercentsN[i] = 1;
        }
    }
}
