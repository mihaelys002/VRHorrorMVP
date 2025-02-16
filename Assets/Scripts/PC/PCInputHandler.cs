using UnityEngine.InputSystem;
using UnityEngine;

public class PCInputHandler : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;

    public Vector2 MovementInput => moveAction.ReadValue<Vector2>();
    public Vector2 MouseInput => lookAction.ReadValue<Vector2>();
    public bool JumpInput => jumpAction.triggered;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        // Get references to actions
        moveAction = playerInput.actions["Move"];
        lookAction = playerInput.actions["Look"];
        jumpAction = playerInput.actions["Jump"];
    }
}