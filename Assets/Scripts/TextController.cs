using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextController : MonoBehaviour
{
    public TMP_Text clockText;

    public TMP_Text cargosEntered;
    public TMP_Text cargosExited;

    public TMP_Text patrolsEntered;
    public TMP_Text patrolsExited;

    public TMP_Text piratesEntered;
    public TMP_Text piratesExited;
    public TMP_Text piratesDestroyed;

    public TMP_Text cargosCaptured;
    public TMP_Text cargosRescued;
    public TMP_Text cargosSucceededEvasion;
    public TMP_Text cargosFailedEvasion;

    private int clockTime = 0;

    private int cargoEnterCount = 0;
    private int cargoExitCount = 0;

    private int patrolEnterCount = 0;
    private int patrolExitCount = 0;

    private int pirateEnterCount = 0;
    private int pirateExitCount = 0;
    private int pirateDestroyCount = 0;

    private int cargoCaptureCount = 0;
    private int cargoRescueCount = 0;
    private int cargoSuccessfulEvasionCount = 0;
    private int cargoFailedEvasionCount = 0;

    void Start() {
        //clockText.text = "Cargos entered: " + clockTime;

        cargosEntered.text = "Cargos entered: " + cargoEnterCount;
        cargosExited.text = "Cargos exited: " + cargoExitCount;
        
        patrolsEntered.text = "Patrols entered: " + patrolEnterCount;
        patrolsExited.text = "Patrols exited: " + patrolExitCount;

        piratesEntered.text = "Pirates entered: " + pirateEnterCount;
        piratesExited.text = "Pirates exited: " + pirateExitCount;
        piratesDestroyed.text = "Pirates destroyed: " + pirateDestroyCount;

        cargosCaptured.text = "Cargos captured: " + cargoCaptureCount;
        cargosRescued.text = "Cargos rescued: " + cargoRescueCount;
        cargosSucceededEvasion.text = "Cargos escape succeeded: " + cargoSuccessfulEvasionCount;
        cargosFailedEvasion.text = "Cargos escape failed : " + cargoFailedEvasionCount;
    }

    void ClockUpdate() {
        //Clock update here
    }

    public void UpdateShipEnter(string shipType) {
        if (shipType == "cargo") {
            cargoEnterCount++;
            cargosEntered.text = "Cargos entered: " + cargoEnterCount;
        }
        else if (shipType == "patrol") {
            patrolEnterCount++;
            patrolsEntered.text = "Patrols entered: " + patrolEnterCount;
        }
        else if (shipType == "pirate") {
            pirateEnterCount++;
            piratesEntered.text = "Pirates entered: " + pirateEnterCount;
        }
        else {
            //fallback error
        }
    }

    public void UpdateShipExit(string shipType) {
        if (shipType == "cargo") {
            cargoExitCount++;
            cargosExited.text = "Cargos exited: " + cargoExitCount;
        }
        else if (shipType == "patrol") {
            patrolExitCount++;
            patrolsExited.text = "Patrols exited: " + patrolExitCount;
        }
        else if (shipType == "pirate") {
            pirateExitCount++;
            piratesExited.text = "Pirates exited: " + pirateExitCount;
        }
        else {
            //fallback error
        }
    }

    public void UpdateEvasion(bool attemptedEvasion, bool successfulEvasion) {
        if (!attemptedEvasion) {
            Debug.Log("No evasion.");
        }
        else if (!successfulEvasion) {
            cargoFailedEvasionCount++;
            cargosFailedEvasion.text = "Cargos escape failed : " + cargoFailedEvasionCount;
        }
        else {
            cargoSuccessfulEvasionCount++;
            cargosSucceededEvasion.text = "Cargos escape succeeded: " + cargoSuccessfulEvasionCount;
        }
    }

    public void PirateDestroyed() {
        pirateDestroyCount++;
        piratesDestroyed.text = "Pirates destroyed: " + pirateDestroyCount;
    }

    public void UpdateCaptures(bool isCaptured) {
        if (isCaptured) {
            cargoCaptureCount++;
            cargosCaptured.text = "Cargos captured: " + cargoCaptureCount;
        }
        else {
            cargoRescueCount++;
            cargosRescued.text = "Cargos rescued: " + cargoRescueCount;
        }
    }
}
