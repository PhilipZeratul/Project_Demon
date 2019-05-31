using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(PhysicsObject))]
public class PlayerController : MonoBehaviour
{
    public FloatRange walkSpeed;
    public float accleration;

    private PhysicsObject physicsObject;


    private void Awake()
    {
        physicsObject = GetComponent<PhysicsObject>();
    }

    // Binded by Unity Event in the editor
    public void Movement(InputAction.CallbackContext ctx)
    {
        Vector2 input = ctx.ReadValue<Vector2>();

        float velocityX = input.x * walkSpeed.min;
        float velocityY = input.y * walkSpeed.min;

        physicsObject.Move(velocityX, velocityY);
    }
}
