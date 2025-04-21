using UnityEngine;
using TMPro;

public class TextController : MonoBehaviour
{
    public TMP_Text cargoEnteredText;
    public TMP_Text cargoExitedText;
    public TMP_Text patrolEnteredText;
    public TMP_Text patrolExitedText;
    public TMP_Text pirateEnteredText;
    public TMP_Text pirateExitedText;
    public TMP_Text capturesText;
    public TMP_Text rescuesText;
    public TMP_Text pirateDestroyedText;
    public TMP_Text successfulEvasionText;
    public TMP_Text failedEvasionText;

    public int cargoEntered = 0, cargoExited = 0;
    public int patrolEntered = 0, patrolExited = 0;
    public int pirateEntered = 0, pirateExited = 0;
    public int captureCount = 0, rescueCount = 0;
    public int piratesDestroyed = 0;
    public int successfulEvasions = 0, failedEvasions = 0;

    void Start()
    {
        ResetCounters();
    }

    public void ResetCounters()
    {
        cargoEntered = cargoExited = patrolEntered = patrolExited = pirateEntered = pirateExited = 0;
        captureCount = rescueCount = piratesDestroyed = successfulEvasions = failedEvasions = 0;
        UpdateAllText();
    }

    void UpdateAllText()
    {
        cargoEnteredText.text = $"Cargos Entered: {cargoEntered}";
        cargoExitedText.text = $"Cargos Exited: {cargoExited}";
        patrolEnteredText.text = $"Patrols Entered: {patrolEntered}";
        patrolExitedText.text = $"Patrols Exited: {patrolExited}";
        pirateEnteredText.text = $"Pirates Entered: {pirateEntered}";
        pirateExitedText.text = $"Pirates Exited: {pirateExited}";
        capturesText.text = $"Captures: {captureCount}";
        rescuesText.text = $"Rescues: {rescueCount}";
        pirateDestroyedText.text = $"Pirates Destroyed: {piratesDestroyed}";
        successfulEvasionText.text = $"Successful Evasions: {successfulEvasions}";
        failedEvasionText.text = $"Failed Evasions: {failedEvasions}";
    }

    public void UpdateShipEnter(string type)
    {
        switch (type.ToLower())
        {
            case "cargo": cargoEntered++; cargoEnteredText.text = $"Cargos Entered: {cargoEntered}"; break;
            case "patrol": patrolEntered++; patrolEnteredText.text = $"Patrols Entered: {patrolEntered}"; break;
            case "pirate": pirateEntered++; pirateEnteredText.text = $"Pirates Entered: {pirateEntered}"; break;
        }
    }

    public void UpdateShipExit(string type)
    {
        switch (type.ToLower())
        {
            case "cargo": cargoExited++; cargoExitedText.text = $"Cargos Exited: {cargoExited}"; break;
            case "patrol": patrolExited++; patrolExitedText.text = $"Patrols Exited: {patrolExited}"; break;
            case "pirate": pirateExited++; pirateExitedText.text = $"Pirates Exited: {pirateExited}"; break;
        }
    }

    public void UpdateCaptures(bool captured)
    {
        if (captured)
        {
            captureCount++;
            capturesText.text = $"Captures: {captureCount}";
        }
        else
        {
            rescueCount++;
            rescuesText.text = $"Rescues: {rescueCount}";
        }
    }

    public void PirateDestroyed()
    {
        piratesDestroyed++;
        pirateDestroyedText.text = $"Pirates Destroyed: {piratesDestroyed}";
    }

    public void UpdateEvasion(bool success, bool finalUpdate)
    {
        if (success)
        {
            successfulEvasions++;
            successfulEvasionText.text = $"Successful Evasions: {successfulEvasions}";
        }
        else //if (finalUpdate)
        {
            failedEvasions++;
            failedEvasionText.text = $"Failed Evasions: {failedEvasions}";
        }
    }
public void ApplyCountersFromString(string counterString)
{
    string[] values = counterString.Split(',');
    if(values.Length == 11)
    {
        cargoEntered = int.Parse(values[0]);
        cargoExited = int.Parse(values[1]);
        patrolEntered = int.Parse(values[2]);
        patrolExited = int.Parse(values[3]);
        pirateEntered = int.Parse(values[4]);
        pirateExited = int.Parse(values[5]);
        captureCount = int.Parse(values[6]);
        rescueCount = int.Parse(values[7]);
        piratesDestroyed = int.Parse(values[8]);
        successfulEvasions = int.Parse(values[9]);
        failedEvasions = int.Parse(values[10]);

        Debug.Log("cargo entered" + cargoEntered);
        
        UpdateAllText();
    }
}
    // New helper methods for undoing interactions during reverse replay.
    public void UndoCapture()
    {
        captureCount = Mathf.Max(0, captureCount - 1);
        capturesText.text = $"Captures: {captureCount}";
    }

    public void UndoRescue()
    {
        rescueCount = Mathf.Max(0, rescueCount - 1);
        rescuesText.text = $"Rescues: {rescueCount}";
    }
}
