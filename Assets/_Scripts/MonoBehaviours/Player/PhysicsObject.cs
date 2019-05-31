using UnityEngine;
using System.Collections.Generic;


[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PhysicsObject : MonoBehaviour
{
    public float VelocityX { get; private set; }
    public float VelocityY { get; private set; }

    [SerializeField]
    private float pushMultiplier = 0.1f;

    private Rigidbody2D rigidbody2d;
    private Collider2D collider2d;
    private ContactFilter2D filter;
    private readonly List<Collider2D> contactColliderList = new List<Collider2D>();
    RaycastHit2D[] hits = new RaycastHit2D[5];


    private void Awake()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
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


        rigidbody2d.velocity = new Vector2(VelocityX, VelocityY);

        //ResolveCollision(ref deltaX, ref deltaY);

        //transform.Translate(deltaX, deltaY, 0f);
    }

    public void Move(float velocityX, float velocityY)
    {
        VelocityX = velocityX;
        VelocityY = velocityY;
    }

    //~TODO:
    private void ResolveCollision(ref float deltaX, ref float deltaY)
    {
        Vector2 move = new Vector2(deltaX, deltaY);
        Vector2 direction = move.normalized;
        float distance = move.magnitude;

        int hitNum = collider2d.Cast(direction, filter, hits, distance);

        for (int i = 0; i < hitNum; i++)
        {
            if (hits[i].transform.CompareTag(Constants.TagName.Wall))
            {
                Debug.LogFormat("Collided with: {0}, distance = {1}, normal = {2}", hits[i].transform.name, hits[i].distance, hits[i].normal);

                if (Mathf.Abs(hits[i].normal.x) > 0.35f)
                {
                    if (Mathf.Sign(direction.x * hits[i].normal.x) < 0)
                    {
                        Debug.LogFormat("Translate: {0}", hits[i].distance * direction.x);


                        transform.Translate(hits[i].distance * direction.x, 0f, 0f);
                        deltaX = 0f;
                    }
                    else if (Mathf.Abs(hits[i].normal.x) < 1f)
                        deltaX = hits[i].normal.x * pushMultiplier;
                }
                if (Mathf.Abs(hits[i].normal.y) > 0.35f)
                {
                    if (Mathf.Sign(direction.y * hits[i].normal.y) < 0)
                    {
                        Debug.LogFormat("Translate: {0}", hits[i].distance * direction.y);


                        transform.Translate(0f, hits[i].distance * direction.y, 0f);
                        deltaY = 0f;
                    }
                    else if (Mathf.Abs(hits[i].normal.y) < 1f)
                        deltaY = hits[i].normal.y * pushMultiplier;
                }
            }
        }
    }
}
