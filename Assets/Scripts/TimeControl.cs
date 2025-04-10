using UnityEngine;

public class TimeControl : MonoBehaviour
{    
    // These thresholds may be used for tick-based triggers.
    public float[] speedLevels = { 1.0f, 0.5f, 0.1f, 0.05f };
    private int currentSpeedIndex = 0;
    
    // This timer resets after each tick (if needed for spawning)
    private float moveTimer = 0f;
    // This global time increases continuously over the simulation.
    private float globalTime = 0f;
    private bool movementPaused = false;

    // Expose the short-term timer (if needed) and global simulation time.
    public float CurrentTime => moveTimer;
    public float GlobalTime => globalTime;
    public bool IsPaused => movementPaused;

    void Update()
    {
        if (!movementPaused)
        {
            float delta = Time.deltaTime;
            moveTimer += delta;
            globalTime += delta;
        }
    }

    // Returns true when it's time to trigger a tick based on speedLevels.
    public bool ShouldMove()
    {
        return moveTimer >= speedLevels[currentSpeedIndex] && !movementPaused;
    }

    // Toggle the simulation on or off.
    public void ToggleMovement(bool pause)
    {
        movementPaused = pause;
        Debug.Log(pause ? "Game Paused" : "Game Resumed");
    }

    // Reset the tick timer (globalTime is never reset).
    public void ResetTimer()
    {
        moveTimer = 0f;
    }
}
