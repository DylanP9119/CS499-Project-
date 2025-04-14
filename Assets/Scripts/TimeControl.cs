using UnityEngine;

public class TimeControl : MonoBehaviour
{    
    public float[] speedLevels = { 1.0f, 0.5f, 0.1f, 0.05f };
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
            movementPaused = !movementPaused;
            ToggleMovement(movementPaused);
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
            moveTimer = 0f;
            return true;
        }
        return false;
    }

    public void ToggleMovement(bool pause)
    {
        movementPaused = pause;
    }

    public void SetSimulationSpeed(int speedIndex)
    {
        currentSpeedIndex = Mathf.Clamp(speedIndex, 0, speedLevels.Length - 1);
    }
}