using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(PhysicsObject))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    public FloatRange walkSpeed;
    public float accleration;

    private PhysicsObject physicsObject;
    private Animator animator;

    private int IsRunningRight = Animator.StringToHash("IsRunningRight");


    private void Awake()
    {
        physicsObject = GetComponent<PhysicsObject>();
        animator = GetComponent<Animator>();
    }

    // Binded by Unity Event in the editor
    public void Movement(InputAction.CallbackContext ctx)
    {
        Vector2 input = ctx.ReadValue<Vector2>();

        float velocityX = input.x * walkSpeed.min;
        float velocityY = input.y * walkSpeed.min;

        if (!MathUtils.NearlyEqual(velocityX, 0f) || !MathUtils.NearlyEqual(velocityY, 0f))        
            animator.SetBool(IsRunningRight, true);       
        else
            animator.SetBool(IsRunningRight, false);

        physicsObject.Move(velocityX, velocityY);
    }
}
