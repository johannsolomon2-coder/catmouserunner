using UnityEngine;

public class GroundMove : MonoBehaviour
{
    public float speed = 5f;

    void Update()
    {
        float moveSpeed = speed;
        if (GameManager.instance != null) moveSpeed = GameManager.instance.currentSpeed;
        
        transform.position += Vector3.left * moveSpeed * Time.deltaTime;

        if (transform.position.x <= -10f)
        {
            transform.position = new Vector3(transform.position.x + 20f, transform.position.y, transform.position.z);
        }
    }
}