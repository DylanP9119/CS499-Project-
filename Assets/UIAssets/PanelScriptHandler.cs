using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PanelScriptHandler : MonoBehaviour
{ 
    public GameObject scrollContent;

    //Deletes panel from view.
    public void DeleteButton()
    {
        Destroy(gameObject);
    }


    private void InstantiateTextData(string message) {
        string inputText = message + ",04/15/2025,10:27,15,12,ON,40,40,15,15,30,30";
        string gridText = "this is just test\ndata\n\ntesting";

        string[] values = inputText.Split(',');

        TMP_Text titleText = gameObject.transform.Find("Save Name Text").GetComponent<TMP_Text>();
        TMP_Text fileDateText = gameObject.transform.Find("Time Made").GetComponent<TMP_Text>();
        TMP_Text runtimeText = gameObject.transform.Find("Runtime").GetComponent<TMP_Text>();
        TMP_Text captureAndShipPercentsText = gameObject.transform.Find("Ship Percents").GetComponent<TMP_Text>();
        TMP_Text gridPercentsText = scrollContent.GetComponentInChildren<TMP_Text>();

        titleText.text = values[0];
        fileDateText.text = "Created on:\n" + values[1] + ", " + values[2];
        runtimeText.text = "Days: " + values[3] + "\nHours: " + values[4];
        captureAndShipPercentsText.text = "2x2 Pirate Night Capture: " + values[5] + "\nCargo: " + values[6] + "% Day, " + values[7] + "% Night " + 
                                                                                     "\nPatrol: " + values[8] + "% Day, " + values[9] + "% Night " + 
                                                                                     "\nPirate: " + values[10] + "% Day, " + values[11] + "% Night ";
        
        gridPercentsText.text = gridText;
    }
}
