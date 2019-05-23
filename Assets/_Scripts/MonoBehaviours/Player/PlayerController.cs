using UnityEngine;


public class PlayerController : MonoBehaviour
{
    public FloatRange walkSpeed;
    public float accleration;


    private PlayerControls controls;


    private void Awake()
    {
        controls.PlayerActionMap.Movement.performed += ctx => Movement(ctx.ReadValue<Vector2>());
    }

    private void Movement(Vector2 input)
    {

    }
}
