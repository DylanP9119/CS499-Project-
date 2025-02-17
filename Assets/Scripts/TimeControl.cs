using UnityEngine;

public class TimeControl : MonoBehaviour
{    
    public static TimeControl Instance { get; private set; }
    
    [Header("Speed Settings")]
    public float[] speedLevels = { 1.0f, 0.5f, 0.1f, 0.05f }; // Time between moves (1s, 0.5s, 0.1s, 0.05s) -> 1x, 2x, 10x, 20x
    private int currentSpeedIndex = 0;
    private float moveTimer = 0f;
    private bool movementPaused = false;

    void Awake()
    {
        // Singleton pattern implementation
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
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

    void UpdateMoveTimer()
    {
        if (!movementPaused)
        {
            moveTimer += Time.deltaTime;
        }
    }

    public bool ShouldMove()
    {
        if (moveTimer >= speedLevels[currentSpeedIndex])
        {
            moveTimer = 0f;
            return true;
        }
        return false;
    }

   void CycleSpeedLevel()
    {
        currentSpeedIndex = (currentSpeedIndex + 1) % speedLevels.Length;
        float[] speedMultipliers = {1f, 2f, 10f, 20f};
        Debug.Log($"Speed changed to: {speedMultipliers[currentSpeedIndex]}x");
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
