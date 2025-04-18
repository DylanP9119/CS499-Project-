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
    }

    public void YesButton()
    {
        popupPanel.SetActive(false);
        SceneManager.LoadScene(mainMenu);
    }

    public void NoButton()
    {
        popupPanel.SetActive(false);
    }
}
