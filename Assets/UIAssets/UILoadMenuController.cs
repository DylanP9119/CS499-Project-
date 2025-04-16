using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class UILoadMenuController : MonoBehaviour
{
    [SerializeField] private string mainMenu = "mainMenu";

    public GameObject itemPrefab;
    public Transform contentParent; 

    public PanelScriptHandler panelScriptHandler;

    public void BackButton()
    {
        SceneManager.LoadScene(mainMenu);
    }

    //Deletes panel from view.
    public void DeleteButton()
    {
        Destroy(itemPrefab);
    }

    //Begins the simulation with prompted information.
    public void StartSim()
    {
        //hjere
    }

    public void AddItemButton()
    {
        Instantiate(itemPrefab, contentParent);
    }

    public void LoadText() {

    }
}
