using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class UIControllerScript : MonoBehaviour
{

    [SerializeField] private string mainMenu = "mainMenu";
    [SerializeField] private string startSim = "simulationRun";
    //buttons
    public Button nightCaptureButton;
    public TMP_Text buttonText;
    public bool nightCaptureEnabled = false;

    public Button beginButton;
    public bool canStart = false;

    //input fields
    private string gridDayString;
    private string gridNightString;

    private double[] cargoGridPercentsD = new double[100];
    private double[] patrolGridPercentsD = new double[100];
    private double[] pirateGridPercentsD = new double[400];

    private double[] cargoGridPercentsN = new double[100];
    private double[] patrolGridPercentsN = new double[100];
    private double[] pirateGridPercentsN = new double[400];

    double multiplier = 0;
    int gridLocation = 0;

    private int cargoDayPercent = -1;
    private int cargoNightPercent = -1;
    private int pirateDayPercent = -1;
    private int pirateNightPercent = -1;
    private int patrolDayPercent = -1;
    private int patrolNightPercent = -1;

    private int dayCount = -1;
    private int hourCount = -1;
    private int minuteCount = 0;

    private bool isParsed;
    
    // THINGS TO DO ON PROGRAM START
    public void Start() {

        //Buttons
        buttonText.text = "DISABLED";
        nightCaptureEnabled = false;

        //fill grid spaces with default values
        for (int gridSpace = 1; gridSpace < cargoGridPercentsD.Length; gridSpace++) {
            cargoGridPercentsD[gridSpace] = 1;
            patrolGridPercentsD[gridSpace] = 1;
            cargoGridPercentsN[gridSpace] = 1;
            patrolGridPercentsN[gridSpace] = 1;
        }
        for (int gridSpace = 1; gridSpace < pirateGridPercentsD.Length; gridSpace++) {
            pirateGridPercentsD[gridSpace] = 1;
            pirateGridPercentsN[gridSpace] = 1;
        }
    }


    //back button
    public void BackButton() {
        SceneManager.LoadScene(mainMenu);
    }

    // TOGGLE DECREASED NIGHT TIME CAPTURE, ONLY WHEN CERTAIN BUTTON IS PRESSED.
    public void NightCaptureToggle() {

        nightCaptureEnabled = !nightCaptureEnabled;

        if (nightCaptureEnabled) {
            buttonText.text = "ENABLED";
        }
        else {
            buttonText.text = "DISABLED";
        }
    }

    // START THE PROGRAM, TOGGLES WHEN "BEGIN" BUTTON IS PRESSED"
    public void BeginToggle() {
        canStart = true;
        
        minuteCount = (hourCount * 60) + (dayCount * 24 * 60);

        if (cargoDayPercent <= 1 || cargoDayPercent > 100) 
            canStart = false;
        else if (cargoNightPercent <= 1 || cargoNightPercent > 100) 
            canStart = false;
        else if (pirateDayPercent <= 1 || pirateDayPercent > 100) 
            canStart = false;
        else if (pirateNightPercent <= 1 || pirateNightPercent > 100) 
            canStart = false;
        else if (patrolDayPercent <= 1 || patrolDayPercent > 100) 
            canStart = false;
        else if (patrolNightPercent <= 1 || patrolNightPercent > 100) 
            canStart = false;
        else if ((dayCount < 0 || hourCount < 0) || (minuteCount < 720 || minuteCount > 43200)) 
            canStart = false;

        /*
        Grid Syntax Checking goes here
        */
        //read from textboxes
        string[] gridLinesDay = gridDayString.Split(new[] {"\r\n", "\n", "\r"}, StringSplitOptions.RemoveEmptyEntries);
        string[] gridLinesNight = gridNightString.Split(new[] {"\r\n", "\n", "\r"}, StringSplitOptions.RemoveEmptyEntries);
        
        //declare separated values
        //For day values...
        string[] values = new string[3];
        foreach (string line in gridLinesDay) {
            values = line.Split(',');

            gridLocation = Int32.Parse(values[1]);
            multiplier = Convert.ToDouble(values[2]);

            switch (values[0]) {
                case "cargo":
                    cargoGridPercentsD[gridLocation] = cargoGridPercentsD[gridLocation] * multiplier;
                    break;
                case "patrol":
                    patrolGridPercentsD[gridLocation] = patrolGridPercentsD[gridLocation] * multiplier;
                    break;
                case "pirate":
                    pirateGridPercentsD[gridLocation] = pirateGridPercentsD[gridLocation] * multiplier;
                    break;
                default:
                    Debug.Log("Invalid Entry for ship type in line: " + line);
                    break;
            }
            Debug.Log(values[0] + " " + values[1] + " " + values[2]);
        }
        //For night values...
        foreach (string line in gridLinesNight) {
            values = line.Split(',');
            switch (values[0]) {
                case "cargo":
                    cargoGridPercentsN[gridLocation] = cargoGridPercentsN[gridLocation] * multiplier;
                    break;
                case "patrol":
                    patrolGridPercentsN[gridLocation] = patrolGridPercentsN[gridLocation] * multiplier;
                    break;
                case "pirate":
                    pirateGridPercentsN[gridLocation] = pirateGridPercentsN[gridLocation] * multiplier;
                    break;
                default: 
                    Debug.Log("Invalid Entry for ship type in line: " + line);
                    break;
            }
        }

        if (!canStart) {
            Debug.Log("Invalid text entry, please check again.");
        }
        else {
            //scene swap
            Debug.Log("SUCCESS!");
           // SceneManager.LoadScene(startSim);
           // SEND ALL VALUES OVER, THOSE VALUES BEING:
           // Night Capture Boolean (nightCaptureEnabled)
           // (Cargo/Patrol/Pirate)(Day/Night)Percents
           // Minute Count (or day/hour count if desired.)
           // (Cargo/Patrol/Pirate)GridPercents(D/N)
        }
        
    }

    //GRID PROBABILITY HANDLING FUNCTIONS

    public void ReadStringInputGRIDDAYPERCENT(string s) {
        gridDayString = s;
    }

    public void ReadStringInputGRIDNIGHTPERCENT(string s) {
        gridNightString = s;
    }

    //ALL CARGO SET VALUES

    public void ReadStringInputCARGODAY(string s) {
        isParsed = Int32.TryParse(s, out cargoDayPercent);

        //invalid entry
        if (!isParsed)
            cargoDayPercent = -1;
        
    }

    public void ReadStringInputCARGONIGHT(string s) {
        isParsed = Int32.TryParse(s, out cargoNightPercent);

        //invalid entry
        if (!isParsed)
            cargoNightPercent = -1;
        
    }

    public void ReadStringInputPIRATEDAY(string s) {
        isParsed = Int32.TryParse(s, out pirateDayPercent);

        //invalid entry
        if (!isParsed)
            pirateDayPercent = -1;
        
    }

    public void ReadStringInputPIRATENIGHT(string s) {
        isParsed = Int32.TryParse(s, out pirateNightPercent);

        //invalid entry
        if (!isParsed)
            pirateNightPercent = -1;
        
    }

    public void ReadStringInputPATROLDAY(string s) {
        isParsed = Int32.TryParse(s, out patrolDayPercent);

        //invalid entry
        if (!isParsed)
            patrolDayPercent = -1;
        
    }

    public void ReadStringInputPATROLNIGHT(string s) {
        isParsed = Int32.TryParse(s, out patrolNightPercent);

        //invalid entry
        if (!isParsed)
            patrolNightPercent = -1;
        
    }

    //ALL TIME VALUES
    public void ReadStringInputDAYS(string s) {
        isParsed = Int32.TryParse(s, out dayCount);

        //invalid entry
        if (!isParsed)
            dayCount = -1;
        
    }

    public void ReadStringInputHOURS(string s) {
        isParsed = Int32.TryParse(s, out hourCount);

        //invalid entry
        if (!isParsed)
            hourCount = -1;
        
    }
}
