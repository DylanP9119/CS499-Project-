using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class TimeControl : MonoBehaviour
{
    public Button forwardButton;
    public float[] speedLevels = { 1.0f, 0.5f, 0.1f, 0.05f }; // 1x, 2x, 10x, 20x for simulation
    private int currentSpeedIndex = 0;
    private float moveTimer = 0f;
    private bool movementPaused = false;
    private float globalTime = 0f;

    void Start()
    {
        if (forwardButton != null)
            forwardButton.onClick.AddListener(CycleSpeedUp);
    }
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
    void CycleSpeedUp()
    {
        Debug.Log("Cycle speed button pressed in simulation mode.");
        currentSpeedIndex = (currentSpeedIndex + 1) % speedLevels.Length;
        Debug.Log($"Simulation speed set to {speedLevels[currentSpeedIndex]} seconds between moves.");
    }
    void UpdateMoveTimer()
    {
        if (!movementPaused)
            moveTimer += Time.deltaTime;
    }

    public bool ShouldMove()
    {
        if (moveTimer >= speedLevels[currentSpeedIndex] && !movementPaused)
        {
            return true;
        }
        return false;
    }

    public void ResetMoveTimer()
    {
        moveTimer = 0f;
    }

    public void ToggleMovement(bool pause)
    {
        movementPaused = pause;
    }

    // Expose pause state for other systems.
    public bool IsPaused => movementPaused;
}
