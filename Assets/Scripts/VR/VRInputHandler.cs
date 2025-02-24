using UnityEngine;

using UnityEngine.XR;

using UnityEngine.InputSystem;
using System;

public class VRInputHandler : MonoBehaviour

{
    [SerializeField] public InputActionReference MoveAction;

    public Vector2 MovementInput { get; private set; }

    public float LeftGrip { get; private set; }

    public float RightGrip { get; private set; }

    public float LeftTrigger { get; private set; }

    public float RightTrigger { get; private set; }



    private void Awake()

    {
        MoveAction.action.performed += OnMove;

        MoveAction.action.canceled += OnMove;

        MoveAction.action.Enable();

    }

    private void OnMove(InputAction.CallbackContext context)
    {
        Debug.LogError("Mooooving");
        if (context.phase == InputActionPhase.Performed)
        {
            MovementInput = context.ReadValue<Vector2>();
        }
        else
        {
            MovementInput = Vector2.zero;
        }
    }

    private void OnEnable()

    {


    }



    private void OnDisable()

    {


    }



    private void Update()

    { 

    }

}