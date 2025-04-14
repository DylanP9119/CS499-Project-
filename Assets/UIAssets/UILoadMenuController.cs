using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UILoadMenuController : MonoBehaviour
{
    [SerializeField] private string mainMenu = "mainMenu";

    public GameObject itemPrefab;
    public Transform contentParent; 

    public void BackButton()
    {
        SceneManager.LoadScene(mainMenu);
    }

    public void AddItem()
    {
        Instantiate(itemPrefab, contentParent);
    }
}
