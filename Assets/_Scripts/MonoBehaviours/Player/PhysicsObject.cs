using UnityEngine;
using System.Collections.Generic;


[RequireComponent(typeof(Collider2D))]
public class PhysicsObject : MonoBehaviour
{
    public float VelocityX { get; private set; }
    public float VelocityY { get; private set; }

    private Collider2D collider2d;
    private ContactFilter2D filter;
    private readonly List<Collider2D> contactColliderList = new List<Collider2D>();


    private void Awake()
    {
        collider2d = GetComponent<Collider2D>();

        filter.useTriggers = false;
        filter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        filter.useLayerMask = true;
    }

    private void FixedUpdate()
    {
        float fixedDeltaTime = Time.fixedDeltaTime;
        float deltaX = VelocityX * fixedDeltaTime;
        float deltaY = VelocityY * fixedDeltaTime;




        ResolveCollision();

        transform.Translate(deltaX, deltaY, 0f);
    }

    public void Move(float velocityX, float velocityY)
    {
        VelocityX = velocityX;
        VelocityY = velocityY;
    }

    //~TODO:
    private void ResolveCollision()
    {
        int contactNum = Physics2D.OverlapCollider(collider2d, filter, contactColliderList);
        if (contactNum != 0)
            Debug.LogFormat("Collided with {0} objects", contactNum);
    }
}
