using UnityEngine;
using UnityEngine.XR;
using UnityEngine.InputSystem;

public class VRInputHandler : MonoBehaviour
{
    [SerializeField] private InputActionAsset vrActionAsset;

    private InputAction moveAction;
    private InputAction gripLeftAction;
    private InputAction gripRightAction;
    private InputAction triggerLeftAction;
    private InputAction triggerRightAction;

    public Vector2 MovementInput { get; private set; }
    public float LeftGrip { get; private set; }
    public float RightGrip { get; private set; }
    public float LeftTrigger { get; private set; }
    public float RightTrigger { get; private set; }

    private void Awake()
    {
        var gameplayMap = vrActionAsset.FindActionMap("VRGameplay");
        moveAction = gameplayMap.FindAction("Move");
        gripLeftAction = gameplayMap.FindAction("GripLeft");
        gripRightAction = gameplayMap.FindAction("GripRight");
        triggerLeftAction = gameplayMap.FindAction("TriggerLeft");
        triggerRightAction = gameplayMap.FindAction("TriggerRight");
    }

    private void OnEnable()
    {
        moveAction.Enable();
        gripLeftAction.Enable();
        gripRightAction.Enable();
        triggerLeftAction.Enable();
        triggerRightAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        gripLeftAction.Disable();
        gripRightAction.Disable();
        triggerLeftAction.Disable();
        triggerRightAction.Disable();
    }

    private void Update()
    {
        MovementInput = moveAction.ReadValue<Vector2>();
        LeftGrip = gripLeftAction.ReadValue<float>();
        RightGrip = gripRightAction.ReadValue<float>();
        LeftTrigger = triggerLeftAction.ReadValue<float>();
        RightTrigger = triggerRightAction.ReadValue<float>();
    }
}