using UnityEngine;
using UnityEngine.UI;

public class TimeControl : MonoBehaviour
{
    public static TimeControl Instance { get; private set; }
    public Button playPauseButton;
    public Button btnIncreaseSpeed;
    public Button btnDecreaseSpeed;
    public float[] speeds = { 1.0f, 0.5f, 0.1f, 0.05f };
    private int currentSpeedIndex = 1;
    private float moveTimer = 0f;
    private bool movementPaused = false;
    private float moveSpeed = 1f;

    void Start()
    {
        Instance = this;
        movementPaused = true;

        if (playPauseButton != null)
            playPauseButton.onClick.AddListener(TogglePlayPause);
        if (btnIncreaseSpeed != null)
            btnIncreaseSpeed.onClick.AddListener(IncreaseSpeed);
        if (btnDecreaseSpeed != null)
            btnDecreaseSpeed.onClick.AddListener(DecreaseSpeed);
    }

    void Update() => UpdateMoveTimer();

    public void TogglePlayPause() => movementPaused = !movementPaused;

    void IncreaseSpeed()
    {
        if (currentSpeedIndex < speeds.Length - 1 && !ReplayManager.Instance.ReplayModeActive)
        {
            currentSpeedIndex++;
            moveSpeed = speeds[currentSpeedIndex];
        }
    }

    void DecreaseSpeed()
    {
        if (currentSpeedIndex > 0 && !ReplayManager.Instance.ReplayModeActive)
        {
            currentSpeedIndex--;
            moveSpeed = speeds[currentSpeedIndex];
        }
    }

    public float GetSpeed() => moveSpeed;
    void UpdateMoveTimer() { if (!movementPaused) moveTimer += Time.deltaTime; }
    public bool ShouldMove() => (moveTimer >= moveSpeed && !movementPaused);
    public void ResetMoveTimer() => moveTimer = 0f;
    public void ToggleMovement(bool pause) => movementPaused = pause;
    public bool IsPaused => movementPaused;
}