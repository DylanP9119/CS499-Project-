using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;
using System;
using System.Linq;

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

    public TMP_Text cargoDayPercentList;
    public TMP_Text pirateDayPercentList;
    public TMP_Text patrolDayPercentList;

    public TMP_Text cargoNightPercentList;
    public TMP_Text pirateNightPercentList;
    public TMP_Text patrolNightPercentList;
    

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
        ReplayManager.Instance.SaveReplayToFile();
        SceneManager.LoadScene(mainMenu);
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
        
        cargoDayPercentList.text = "Cargo Day %s:\n";
        pirateDayPercentList.text = "Pirate Day %s:\n";
        patrolDayPercentList.text = "Patrol Day %s:\n";

        cargoNightPercentList.text = "Cargo Night %s:\n";
        pirateNightPercentList.text = "Pirate Night %s:\n";
        patrolNightPercentList.text = "Patrol Night %s:\n";

        List<string> values1 = ReturnGridPercents(DataPersistence.Instance.cargoGridPercentsD);
        List<string> values2 = ReturnGridPercents(DataPersistence.Instance.pirateGridPercentsD);
        List<string> values3 = ReturnGridPercents(DataPersistence.Instance.patrolGridPercentsD);
        List<string> values4 = ReturnGridPercents(DataPersistence.Instance.cargoGridPercentsN);
        List<string> values5 = ReturnGridPercents(DataPersistence.Instance.pirateGridPercentsN);
        List<string> values6 = ReturnGridPercents(DataPersistence.Instance.patrolGridPercentsN);

        for (int i = 0; i < values2.Count; i++) {
            if (i < values1.Count) {
                cargoDayPercentList.text = cargoDayPercentList.text + values1[i];
                patrolDayPercentList.text = patrolDayPercentList.text + values3[i];

                cargoNightPercentList.text = cargoNightPercentList.text + values4[i];
                patrolNightPercentList.text = patrolNightPercentList.text + values6[i];
            }
            pirateDayPercentList.text = pirateDayPercentList.text + values2[i];
            pirateNightPercentList.text = pirateNightPercentList.text + values5[i];
        }


    }
    public List<string> ReturnGridPercents(double[] values)
    {
        double sumOfValues = 0.0;
        List<string> stringList = new List<string>();

        for (int i = 0; i < values.Length; i++)
        {
            sumOfValues = sumOfValues + values[i];
        }

        for (int i = 0; i < values.Length; i++)
        {
            double roundedPercent = Math.Round(((values[i] / sumOfValues) * 100), 3);
            stringList.Add("Space " + i + ": " + roundedPercent + "%\n");
        }

        return stringList;
    }

    public void ExitInfoButton() {
        infoPanel.SetActive(false);
    }
}
