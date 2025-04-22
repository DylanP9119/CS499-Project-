using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class UISimSettings : MonoBehaviour
{
    [SerializeField] private string mainMenu = "mainMenu";

    public GameObject popupPanel;
    public GameObject cubePrefab;
    public Material startingMaterial;

    public Vector3 startPosition = new Vector3(25f, 0f, 25f);
    private List<GameObject> spawnedCubes = new List<GameObject>();

    void Start() {
        popupPanel.SetActive(false);
    }

    void Awake() {
        //instantiate grid
        popupPanel.SetActive(false);
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
}
