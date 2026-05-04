using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public GameObject obstaclePrefab;
    public GameObject poisonPrefab;
    public GameObject pitPrefab;
    public GameObject birdPrefab;
    
    public float spawnTime = 2f;

    public float minY = -1.5f;
    public float maxY = -0.5f;

    void Start()
    {
        InvokeRepeating("SpawnObstacle", 1f, spawnTime);
    }

    void SpawnObstacle()
    {
        if (GameManager.instance != null && GameManager.instance.isGameOver) return;
        
        int obstacleType = Random.Range(1, 4); // Skip 0 (Standard brown box)
        GameObject prefabToSpawn = obstaclePrefab;
        Vector3 spawnPos = transform.position;

        if (obstacleType == 0) // Standard Obstacle
        {
            prefabToSpawn = obstaclePrefab;
            spawnPos.y = Random.Range(minY, maxY);
        }
        else if (obstacleType == 1) // Poison Cheese
        {
            prefabToSpawn = poisonPrefab != null ? poisonPrefab : obstaclePrefab;
            spawnPos.y = Random.Range(minY, maxY);
        }
        else if (obstacleType == 2) // Pit
        {
            prefabToSpawn = pitPrefab != null ? pitPrefab : obstaclePrefab;
            spawnPos.y = -2.5f; // Place pit lower
        }
        else if (obstacleType == 3) // Bird
        {
            prefabToSpawn = birdPrefab != null ? birdPrefab : obstaclePrefab;
            spawnPos.y = Random.Range(maxY + 1f, maxY + 2.5f); // Place bird higher
        }

        GameObject spawned = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

        // Fallbacks to set up tags/colors if prefabs aren't properly assigned
        if (obstacleType == 1 && poisonPrefab == null)
        {
            spawned.name = "PoisonCheese";
            SpriteRenderer sr = spawned.GetComponent<SpriteRenderer>();
            Sprite s = GameManager.LoadSpriteFromFile("PoisonCheeseSprite.png");
            if (s != null && sr != null) { sr.sprite = s; sr.color = Color.white; spawned.transform.localScale = new Vector3(0.3f, 0.3f, 1f); sr.sortingOrder = 5; }
            else if (sr != null) sr.color = new Color(1f, 0.6f, 0f); // Cheese orange/yellow
        }
        else if (obstacleType == 2 && pitPrefab == null)
        {
            spawned.name = "Pit";
            SpriteRenderer sr = spawned.GetComponent<SpriteRenderer>();
            Sprite s = GameManager.LoadSpriteFromFile("PitSprite.png");
            if (s != null && sr != null) { sr.sprite = s; sr.color = Color.white; spawned.transform.localScale = new Vector3(0.4f, 0.4f, 1f); sr.sortingOrder = 5; }
            else if (sr != null) sr.color = Color.black;
            Collider2D col = spawned.GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
            if (s == null) spawned.transform.localScale = new Vector3(2f, 0.5f, 1f);
        }
        else if (obstacleType == 3 && birdPrefab == null)
        {
            spawned.name = "Bird";
            SpriteRenderer sr = spawned.GetComponent<SpriteRenderer>();
            Sprite s = GameManager.LoadSpriteFromFile("BirdSprite.png");
            if (s != null && sr != null) { sr.sprite = s; sr.color = Color.white; spawned.transform.localScale = new Vector3(0.3f, 0.3f, 1f); sr.sortingOrder = 5; }
            else if (sr != null) sr.color = Color.red;
            if (s == null) spawned.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        }
    }
}