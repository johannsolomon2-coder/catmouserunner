using UnityEngine;

public class ObstacleMove : MonoBehaviour
{
    public float speed = 5f;

    void Update()
    {
        float moveSpeed = speed;
        if (GameManager.instance != null) moveSpeed = GameManager.instance.currentSpeed;
        
        transform.position += Vector3.left * moveSpeed * Time.deltaTime;

        if (transform.position.x < -12f)
        {
            Destroy(gameObject);
        }
    }
}