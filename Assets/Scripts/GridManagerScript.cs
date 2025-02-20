using UnityEngine;
using System.Collections.Generic;

// Main grid class
public class GridManagerScript : MonoBehaviour
{
    // Declare width, height, and dictionary for grid layout
    private int gridWidth = 400;
    private int gridHeight = 100;
    private Dictionary<Vector2Int, TileSelector> grid = new Dictionary<Vector2Int, TileSelector>();

    //On start...
    void Start() { 

        //Create 2D array that loops through and creates a grid cell for each coordinate.
        for (int x = 1; x <= gridWidth; x++) {
            for (int y = 1; y <= gridHeight; y++) {
                Vector2Int pos = new Vector2Int(x, y);
                grid[pos] = new TileSelector(x, y);
            }
        }
    }
}


public class TileSelector
{
    public int xCoord, yCoord;
    public GameObject tile;

    public TileSelector(int x, int y)
    {
        xCoord = x;
        yCoord = y;
    }
}