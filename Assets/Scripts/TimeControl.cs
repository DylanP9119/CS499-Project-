using UnityEngine;

public class TimeControl : MonoBehaviour
{    
    public static TimeControl Instance { get; private set; }
    
    private float moveTimer = 0f;
    private bool movementPaused = false;
    public float spawnChance = 0.0f;
    private float chance = 0.0f;

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
            chance = Random.Range(0,100);                            
            if(chance <= spawnChance)
            {
            chance = Random.Range(0,100);
            if(chance >= 0.0)
                {
                     // CHANGE LATER                                    
                }
            }
            return true;
        }
        return false;
    }

    public void ToggleMovement(bool pause)
    {
        movementPaused = pause;
    }
}
