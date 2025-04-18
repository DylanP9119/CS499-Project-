using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

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

    [DllImport("__Internal")]
    private static extern void ShowFileUpload();

    public void OpenFileUpload()
    {
    #if UNITY_WEBGL && !UNITY_EDITOR
        ShowFileUpload();
    #else
        Debug.Log("File upload only works in WebGL builds.");
    #endif
    }

    // Called from JavaScript with the uploaded JSON
    public void OnJsonFileLoaded(string json)
    {
        Debug.Log("JSON Loaded: " + json);

        // Deserialize and use the data
        MyData data = JsonUtility.FromJson<MyData>(json);
        Debug.Log("Message from JSON: " + data.message);
    }

    [System.Serializable]
    public class MyData
    {
        public string message;
    }
}
