using UnityEngine;

public class ShipController : MonoBehaviour
{
    public GameObject ship;

    public float spawnChance;
    private float chance = 0.0f;
    float moveTimer = 0.0f;

    public GameObject cargoPrefab;
    public GameObject patrolPrefab;
    public GameObject piratePrefab;

    private TimeControl isPaused;

    
    void Start()
    {
        isPaused = FindFirstObjectByType<TimeControl>();
    }
    void Update()
    {
        if(isPaused.ShouldMove())
        {
            moveTimer += Time.deltaTime;    
            if(moveTimer >= 1)   
            {
                SpawnShip();
                moveTimer = 0;
            }
        }
    }

    void SpawnShip()
    {
        chance = Random.Range(0.1f,1.0f);                            
        if(chance <= spawnChance)
        {
            Debug.Log(chance);
            if(chance <= 0.50f)
                {
                    Instantiate(cargoPrefab, GetSpawnPosition(), Quaternion.identity);
                    Debug.Log("Cargo Ship Spawned");
                }
            else if(chance <= 0.50f + .25f )
                {
                    Instantiate(patrolPrefab, GetSpawnPosition(), Quaternion.identity);
                    Debug.Log("Patrol Ship Spawned");   
                }
            else
                {
                    Instantiate(piratePrefab, GetSpawnPosition(), Quaternion.identity);
                    Debug.Log("Pirate Ship Spawned");     
                }
            }
    }
    Vector3 GetSpawnPosition()
    {
        // Change for Grid Size
        float x = Random.Range(-30f, 30f);
        float y = Random.Range(-30f, 30f);
        return new Vector3(x, y, 0);
    }
}
