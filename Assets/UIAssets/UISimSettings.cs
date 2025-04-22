using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;

public class UISimSettings : MonoBehaviour
{
    [SerializeField] private string mainMenu = "mainMenu";

    public GameObject popupPanel;
    public GameObject infoPanel;
    public GameObject cubePrefab;
    public Material startingMaterial;

    public TMP_Text instantiatedStatsInfo;

    public Vector3 startPosition = new Vector3(25f, 0f, 25f);
    private List<GameObject> spawnedCubes = new List<GameObject>();

    public static DataPersistence Instance;

    void Awake() {
        //instantiate grid
        popupPanel.SetActive(false);
        infoPanel.SetActive(false);
        UpdateGrid(startingMaterial);
    }

    public void UpdateGrid(Material importedMaterial) {
        for (int x = 0; x < 8; x++) {
            for (int y = 0; y < 2; y++) {
                Vector3 updatedPosition = new Vector3(x * 50, 0, y * 50);
                Vector3 spawnPosition = startPosition + updatedPosition;
    
                GameObject newCube = Instantiate(cubePrefab, spawnPosition, Quaternion.identity);

                newCube.name = $"Cube_{x}_{y}";

                Renderer cubeRenderer = newCube.GetComponent<Renderer>();
                cubeRenderer.material = importedMaterial;
                spawnedCubes.Add(newCube);
            }
        }
    }

    public void DeleteGrid() {
        foreach (GameObject cube in spawnedCubes) {
            Destroy(cube);
        }
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

    public void InfoButton() {
        infoPanel.SetActive(true);

        instantiatedStatsInfo.text = "File Name:\n" + DataPersistence.Instance.fileNameString + "\n\nTotal Sim Time:\n" + DataPersistence.Instance.dayCount + " Days, " + DataPersistence.Instance.hourCount + " Hours\n\nNight Values: "
                                    + DataPersistence.Instance.nightCaptureEnabled + "\n\nCargo Percents:\n" + DataPersistence.Instance.cargoDayPercent + "% Day, " + DataPersistence.Instance.cargoNightPercent + "% Night\nPirate Percents:\n"
                                    + DataPersistence.Instance.pirateDayPercent + "% Day, " + DataPersistence.Instance.pirateNightPercent + "% Night\nPatrol Percents:\n" + DataPersistence.Instance.patrolDayPercent + "% Day, " 
                                    + DataPersistence.Instance.patrolNightPercent + "% Night";
    }

    public void ExitInfoButton() {
        infoPanel.SetActive(false);
    }
}
