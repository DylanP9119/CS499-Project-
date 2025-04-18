using UnityEngine;
using TMPro;

public class ShipUICounter : MonoBehaviour
{
    public TMP_Text shipEnteredText;
    public TMP_Text shipExitedText;
    public TMP_Text shipOnscreenText;

    private int enterCount = 0;
    private int exitCount = 0;
    private int onscreenCount = 0;

    void Start() {

        for (int i = 0; i < 100; i++) {
            UpdateShipEnter();
            UpdateShipEnter();
            UpdateShipExit();
        }

    }

    void UpdateShipEnter() {
        enterCount++;
        shipEnteredText.text = "Ships entered: " + enterCount;
        UpdateShipOnscreen(1);
    }

    void UpdateShipExit() {
        exitCount++;
        shipExitedText.text = "Ships exited: " + exitCount;
        UpdateShipOnscreen(-1);
    }

    void UpdateShipOnscreen(int increment) {
        onscreenCount = onscreenCount + increment;
        shipOnscreenText.text = "Ships on-screen: " + onscreenCount;
    }
}
