using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class UIControllerScript : MonoBehaviour
{

    [SerializeField] private string mainMenu = "mainMenu";
    //buttons
    public Button nightCaptureButton;
    public TMP_Text buttonText;
    public bool nightCaptureEnabled = false;

    public Button beginButton;
    public bool canStart = false;

    //percent input fields
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
    
    public void Start() {

        //Buttons
        buttonText.text = "DISABLED";
        nightCaptureEnabled = false;
        Debug.Log(buttonText);
    }

    public void TextMeshUpdated(string text) {
        Debug.Log("STRING UPDATED: " + text);
    }

    public void BackButton() {
        SceneManager.LoadScene(mainMenu);
    }

    //button toggle
    public void NightCaptureToggle() {

        nightCaptureEnabled = !nightCaptureEnabled;

        if (nightCaptureEnabled) {
            buttonText.text = "ENABLED";
        }
        else {
            buttonText.text = "DISABLED";
        }
    }

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

        if (!canStart) {
            Debug.Log("Invalid text entry, please check again.");
        }
        else {
            //scene swap
            Debug.Log("SUCCESS!");
        }
        
    }

    //all cargo values
    public void ReadStringInputCARGODAY(string s) {
        isParsed = Int32.TryParse(s, out cargoDayPercent);

        if (isParsed)
            Debug.Log(cargoDayPercent);
        else
            cargoDayPercent = -1;
        
    }

    public void ReadStringInputCARGONIGHT(string s) {
        isParsed = Int32.TryParse(s, out cargoNightPercent);

        if (isParsed)
            Debug.Log(cargoNightPercent);
        else
            cargoNightPercent = -1;
        
    }

    public void ReadStringInputPIRATEDAY(string s) {
        isParsed = Int32.TryParse(s, out pirateDayPercent);

        if (isParsed)
            Debug.Log(pirateDayPercent);
        else
            pirateDayPercent = -1;
        
    }

    public void ReadStringInputPIRATENIGHT(string s) {
        isParsed = Int32.TryParse(s, out pirateNightPercent);

        if (isParsed)
            Debug.Log(pirateNightPercent);
        else
            pirateNightPercent = -1;
        
    }

    public void ReadStringInputPATROLDAY(string s) {
        isParsed = Int32.TryParse(s, out patrolDayPercent);

        if (isParsed)
            Debug.Log(patrolDayPercent);
        else
            patrolDayPercent = -1;
        
    }

    public void ReadStringInputPATROLNIGHT(string s) {
        isParsed = Int32.TryParse(s, out patrolNightPercent);

        if (isParsed)
            Debug.Log(patrolNightPercent);
        else
            patrolNightPercent = -1;
        
    }

    //time values
    public void ReadStringInputDAYS(string s) {
        isParsed = Int32.TryParse(s, out dayCount);

        if (isParsed)
            Debug.Log(dayCount);
        else
            dayCount = -1;
        
    }

    public void ReadStringInputHOURS(string s) {
        isParsed = Int32.TryParse(s, out hourCount);

        if (isParsed)
            Debug.Log(hourCount);
        else
            hourCount = -1;
        
    }
}
