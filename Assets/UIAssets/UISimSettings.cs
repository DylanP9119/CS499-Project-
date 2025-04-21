using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UISimSettings : MonoBehaviour
{
    [SerializeField] private string mainMenu = "mainMenu";

    public GameObject popupPanel;

    void Start() {
        popupPanel.SetActive(false);
    }

    public void BackButton()
    {
        popupPanel.SetActive(true);
        //Pause simulation?
    }

    public void YesButton()
    {
        popupPanel.SetActive(false);
        SaveSimluation();
    }

    public void NoButton()
    {
        SceneManager.LoadScene(mainMenu);
    }

    public void CancelButton()
    {
        popupPanel.SetActive(false);
        //Play simluation
    }

    public void SaveSimluation() {
        //Save info goes here!
    }
}
