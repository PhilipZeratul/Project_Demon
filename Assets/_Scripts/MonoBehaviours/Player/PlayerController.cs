using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(PhysicsObject))]
public class PlayerController : MonoBehaviour
{
    public FloatRange walkSpeed;
    public float accleration;


    private PhysicsObject physicsObject;

    PlayerControls controls;

    private void Awake()
    {
        //controls.Player.Move.performed += ctx => Movement(ctx.ReadValue<Vector2>());
        physicsObject = GetComponent<PhysicsObject>();
    }

    public void Movement(InputAction.CallbackContext ctx)
    {
        Vector2 input = ctx.ReadValue<Vector2>();
        physicsObject.Move(input.x, input.y);
    }
}
