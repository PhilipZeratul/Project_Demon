using UnityEngine;


[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PhysicsObject : MonoBehaviour
{
    public float VelocityX { get; private set; }
    public float VelocityY { get; private set; }

    private Rigidbody2D rb;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        rb.velocity = new Vector2(VelocityX, VelocityY);
    }

    public void Move(float velocityX, float velocityY)
    {
        VelocityX = velocityX;
        VelocityY = velocityY;
    }
}
