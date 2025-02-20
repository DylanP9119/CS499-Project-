using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TimeControl : MonoBehaviour
{    
    public float[] speedLevels = { 1.0f, 0.5f, 0.1f, 0.05f }; // Time between moves (1s, 0.5s, 0.1s, 0.05s) -> 1x, 2x, 10x, 20x
    private int currentSpeedIndex = 0;
    private float moveTimer = 0f;
    private bool movementPaused = false;

    void Update()
    {
        HandleSpeedInput();
        UpdateMoveTimer();
    }

    void HandleSpeedInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CycleSpeedLevel();
        }
    }

    void UpdateMoveTimer()
    {
        if (!movementPaused)
        {
            moveTimer += Time.deltaTime;
        }
    }

    public bool ShouldMove()
    {
        if (moveTimer >= speedLevels[currentSpeedIndex] && !movementPaused)
        {
            return true;
        }
        return false;
    }



    public void ToggleMovement(bool pause)
    {
        movementPaused = pause;
    }

    public float GetCurrentInterval()
    {
        return speedLevels[currentSpeedIndex];
    }
}
