using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class TimeControl : MonoBehaviour
{ 
    public Button playPauseButton;
    public Button btnIncreaseSpeed;
    public Button btnDecreaseSpeed;
    public float[] speeds = { 1.0f, 0.5f, 0.1f, 0.05f }; // For simulation: 1x, 2x, 10x, 20x speeds    private int currentSpeedIndex = 0;
    private float moveTimer = 0f;
    private bool movementPaused = false;
    private float globalTime = 0f;
    public float replaySpeed = 1f;
    private float moveSpeed = 1f;
    private int currentSpeedIndex = 1; // Start at 1x.

    void Start()
    {
        if (playPauseButton != null)
            playPauseButton.onClick.AddListener(TogglePlayPause);
        if (btnIncreaseSpeed != null)
            btnIncreaseSpeed.onClick.AddListener(IncreaseSpeed);
        if (btnDecreaseSpeed != null)
            btnDecreaseSpeed.onClick.AddListener(DecreaseSpeed);
    }

    void Update()
    {
        UpdateMoveTimer();
    }
    void TogglePlayPause() => movementPaused = !movementPaused;

    void IncreaseSpeed()
    {
        if (currentSpeedIndex < speeds.Length - 1)
        {
            if(!ReplayManager.Instance.ReplayModeActive)
            {
            currentSpeedIndex++;
            moveSpeed = speeds[currentSpeedIndex];
            Debug.Log($"Replay speed set to {moveSpeed}x");
            }
        }
    }
    void DecreaseSpeed()
    {
        if (currentSpeedIndex > 0)
        {
            if(!ReplayManager.Instance.ReplayModeActive)
            {
            currentSpeedIndex--;
            moveSpeed = speeds[currentSpeedIndex];
            Debug.Log($"Replay speed set to {moveSpeed}x");
            }
        }
    }
    public float GetSpeed()
    {
        return(moveSpeed);
    }
    void UpdateMoveTimer()
    {
        if (!movementPaused)
            moveTimer += Time.deltaTime;
    }

    public bool ShouldMove()
    {
        if (moveTimer >= moveSpeed && !movementPaused)
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
