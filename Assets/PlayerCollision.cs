using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (gameObject.name != "Mouse") return;
        HandleCollision(collision.gameObject);
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (gameObject.name != "Mouse") return;
        HandleCollision(collider.gameObject);
    }

    void HandleCollision(GameObject obj)
    {
        string objName = obj.name;
        if (obj.CompareTag("Obstacle") || objName.Contains("Bird"))
        {
            if (GameManager.instance != null) GameManager.instance.HitObstacle();
            Destroy(obj);
        }
        else if (objName.Contains("PoisonCheese"))
        {
            if (GameManager.instance != null) GameManager.instance.HitPoison();
            Destroy(obj);
        }
        else if (objName.Contains("Pit"))
        {
            if (GameManager.instance != null) GameManager.instance.FallInPit();
        }
    }
}