using UnityEngine;

public class CraneHolder : MonoBehaviour
{
    Rigidbody2D rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocityY = -5.0f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
