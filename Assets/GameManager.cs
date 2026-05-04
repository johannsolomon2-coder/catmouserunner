using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public float score;
    public bool isGameOver = false;
    
    public float baseSpeed = 8f;
    public float currentSpeed;
    
    public int obstacleHits = 0;
    public float timeSinceLastHit = 0f;
    public float recoveryTime = 4f; // Time without hitting to recover
    
    public Transform cat;
    public Transform mouse;
    private Vector3 catFarPosition;
    private Vector3 catClosePosition;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void AutoCreateGameManager()
    {
        if (instance == null)
        {
            GameObject go = new GameObject("GameManager");
            go.AddComponent<GameManager>();
            DontDestroyOnLoad(go);
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        currentSpeed = baseSpeed;
    }

    void Start()
    {
        // Auto-find references if not assigned
        if (mouse == null)
        {
            // Search for Mouse even if inactive
            Transform[] allTransforms = Resources.FindObjectsOfTypeAll<Transform>();
            foreach (Transform t in allTransforms)
            {
                if (t.name == "Mouse" && t.gameObject.scene.isLoaded)
                {
                    mouse = t;
                    break;
                }
            }
            
            // If still null, create it dynamically
            if (mouse == null)
            {
                GameObject m = new GameObject("Mouse");
                mouse = m.transform;
                mouse.position = new Vector3(0, 1f, 0); // Start higher up so it falls cleanly onto the ground
                
                SpriteRenderer msr = m.AddComponent<SpriteRenderer>();
                msr.sortingOrder = 15; 
                
                // Load sprite FIRST so the collider knows how big it needs to be!
                Sprite s = LoadSpriteFromFile("MouseSprite.png");
                if (s != null) { msr.sprite = s; msr.color = Color.white; }
                
                BoxCollider2D col = m.AddComponent<BoxCollider2D>();
                Rigidbody2D rb = m.AddComponent<Rigidbody2D>();
                rb.freezeRotation = true;
                
                m.AddComponent<PlayerJump>();
                m.AddComponent<PlayerCollision>();
            }
        }

        if (mouse != null)
        {
            mouse.gameObject.SetActive(true); 
            mouse.localScale = new Vector3(0.8f, 0.8f, 1f); 
            
            // Just in case it was pre-existing and needs the sprite
            SpriteRenderer sr = mouse.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite == null)
            {
                sr.sortingOrder = 15;
                Sprite s = LoadSpriteFromFile("MouseSprite.png");
                if (s != null) { sr.sprite = s; sr.color = Color.white; }
            }
        }

        if (cat == null)
        {
            GameObject c = GameObject.Find("Cat");
            if (c == null) c = GameObject.Find("PlayerCat"); // Find PlayerCat since that's what it's named in the hierarchy
            if (c != null) cat = c.transform;
        }

        if (cat != null)
        {
            if (cat.CompareTag("Player"))
            {
                cat.tag = "Untagged"; // Ensure Cat is NOT the player
            }
            cat.localScale = new Vector3(0.6f, 0.6f, 1f); // Scale down the cat
            
            // Disable Animator so it doesn't override our new sprite!
            Animator anim = cat.GetComponent<Animator>();
            if (anim != null) anim.enabled = false;

            SpriteRenderer sr = cat.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Sprite s = LoadSpriteFromFile("CatSprite.png");
                if (s != null) { sr.sprite = s; sr.color = Color.white; sr.sortingOrder = 16; }
            }
        }
        
        if (cat != null && mouse != null)
        {
            // Cat is behind the mouse, but keep it on-screen!
            catFarPosition = mouse.position + new Vector3(-5f, 0, 0);
            catClosePosition = mouse.position + new Vector3(-2f, 0, 0);
            cat.position = catFarPosition;
        }
    }

    void Update()
    {
        if (isGameOver)
        {
            currentSpeed = 0f;
        }
        else
        {
            // Increase score as we "move forward"
            score += currentSpeed * Time.deltaTime * 2f;
            
            if (currentSpeed < baseSpeed)
            {
                timeSinceLastHit += Time.deltaTime;
                if (timeSinceLastHit >= recoveryTime)
                {
                    // Recovered from the stumble!
                    obstacleHits = 0;
                    timeSinceLastHit = 0f;
                    currentSpeed = baseSpeed; 
                }
            }
        }
        
        // Update Cat Position smoothly for the chase effect
        if (cat != null && mouse != null)
        {
            Vector3 targetCatPos = catFarPosition;
            if (obstacleHits == 1)
            {
                targetCatPos = catClosePosition; // Cat gets close!
            }
            else if (obstacleHits >= 2 || isGameOver)
            {
                // Cat catches the mouse! Offset it slightly so they don't perfectly overlap
                targetCatPos = mouse.position + new Vector3(-0.4f, 0, 0); 
            }
            
            // Keep cat's local Y/Z or match mouse Y if preferred. Usually, just keep X updating.
            targetCatPos.y = cat.position.y;
            targetCatPos.z = cat.position.z;
            
            // Move faster when catching!
            float chaseSpeed = isGameOver ? 15f : 5f;
            cat.position = Vector3.Lerp(cat.position, targetCatPos, Time.deltaTime * chaseSpeed);
        }
    }

    public void HitObstacle()
    {
        if (isGameOver) return;
        
        obstacleHits++;
        timeSinceLastHit = 0f;
        
        if (obstacleHits == 1)
        {
            // Stuck (slow down) for a short time
            currentSpeed = baseSpeed * 0.4f; 
        }
        else if (obstacleHits >= 2)
        {
            // Game Over
            isGameOver = true;
            currentSpeed = 0f;
        }
    }

    public void HitPoison()
    {
        if (isGameOver) return;
        
        timeSinceLastHit = 0f;
        currentSpeed = baseSpeed * 0.6f; 
        if (obstacleHits == 0) obstacleHits = 1; // Make the cat get closer
    }

    public void FallInPit()
    {
        if (isGameOver) return;
        
        HitObstacle(); // Just take 1 hit instead of instant game over
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 40;
        style.normal.textColor = Color.black;
        style.fontStyle = FontStyle.Bold;
        
        if (!isGameOver)
        {
            GUI.Label(new Rect(20, 20, 300, 50), "Score: " + Mathf.FloorToInt(score).ToString(), style);
        }
        else
        {
            bool catReached = true;
            if (mouse != null && mouse.gameObject.activeInHierarchy && cat != null)
            {
                if (Vector3.Distance(new Vector3(cat.position.x, 0, 0), new Vector3(mouse.position.x, 0, 0)) > 1.5f)
                {
                    catReached = false;
                }
            }

            if (catReached)
            {
                GUIStyle goStyle = new GUIStyle();
            goStyle.fontSize = 60;
            goStyle.normal.textColor = Color.red;
            goStyle.alignment = TextAnchor.MiddleCenter;
            goStyle.fontStyle = FontStyle.Bold;
            
            GUI.Label(new Rect(Screen.width / 2 - 200, Screen.height / 2 - 100, 400, 100), "CAT CAUGHT YOU!", goStyle);
            
            GUIStyle scoreStyle = new GUIStyle();
            scoreStyle.fontSize = 40;
            scoreStyle.normal.textColor = Color.black;
            scoreStyle.alignment = TextAnchor.MiddleCenter;
            
            GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height / 2, 300, 50), "Final Score: " + Mathf.FloorToInt(score).ToString(), scoreStyle);
            
            if (GUI.Button(new Rect(Screen.width / 2 - 75, Screen.height / 2 + 80, 150, 50), "Restart Game"))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            }
        }
    }

    public static Sprite LoadSpriteFromFile(string fileName)
    {
        string filePath = System.IO.Path.Combine(Application.dataPath, "Resources", fileName);
        if (System.IO.File.Exists(filePath))
        {
            byte[] fileData = System.IO.File.ReadAllBytes(filePath);
            Texture2D tex = new Texture2D(2, 2);
            if (tex.LoadImage(fileData))
            {
                // Force texture to have an alpha channel by creating a new RGBA32 texture
                Texture2D texWithAlpha = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false);
                
                // Remove white background to make it look like a standalone object
                Color[] pixels = tex.GetPixels();
                for (int i = 0; i < pixels.Length; i++)
                {
                    // If the pixel is mostly white, make it transparent
                    if (pixels[i].r > 0.85f && pixels[i].g > 0.85f && pixels[i].b > 0.85f)
                    {
                        pixels[i] = new Color(0, 0, 0, 0); 
                    }
                }
                texWithAlpha.SetPixels(pixels);
                texWithAlpha.Apply();

                // Increase Pixels Per Unit from 100 to 300 to make the base sprite smaller
                return Sprite.Create(texWithAlpha, new Rect(0, 0, texWithAlpha.width, texWithAlpha.height), new Vector2(0.5f, 0.5f), 300f);
            }
        }
        return null;
    }
}
