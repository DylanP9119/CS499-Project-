using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonControllerScript : MonoBehaviour
{
    [SerializeField] private string newSim = "simMenu";
    [SerializeField] private string loadSim = "loadSimMenu";
    [SerializeField] private string credits = "creditsMenu";

    public void NewSimButton() {
        SceneManager.LoadScene(newSim);
    }

    public void LoadSimButton() {
        SceneManager.LoadScene(loadSim);
    }

    public void CreditsButton() {
        SceneManager.LoadScene(credits);
    }
}
