using UnityEngine.InputSystem;
using UnityEngine;

public class PCInputHandler : MonoBehaviour
{
    public Vector2 MovementInput { get; private set; }
    public Vector2 MouseInput { get; private set; }
    public bool JumpInput { get; private set; }

    public bool SprintingInput { get; private set; }


    public void OnMove(InputValue value)
    {
        MovementInput = value.Get<Vector2>();
    }
    public void OnLook(InputValue value) { MouseInput = value.Get<Vector2>(); }

    public void OnJump(InputValue value) { JumpInput = value.Get<bool>(); }

    public void OnSprint(InputValue value) { SprintingInput = value.Get<bool>(); }



}