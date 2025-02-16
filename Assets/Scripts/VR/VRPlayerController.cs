using Mirror;
using UnityEngine;

public class VRPlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform headset;
    [SerializeField] private Transform leftController;
    [SerializeField] private Transform rightController;
    [SerializeField] private CharacterController characterController;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float snapTurnAngle = 45f;

    private VRInputHandler inputHandler;
    private float verticalVelocity;

    private void Start()
    {

        inputHandler = GetComponent<VRInputHandler>();
        characterController = GetComponent<CharacterController>();
        if (!GetComponent<NetworkIdentity>().isLocalPlayer)
        {
            // Disable input handling for non-local players
            enabled = false;
            inputHandler.enabled = false;
            return;
        }
    }

    private void Update()
    {


        HandleMovement();
        ApplyGravity();
        UpdateBodyPosition();
    }

    private void HandleMovement()
    {
        Vector3 moveDirection = headset.TransformDirection(
            new Vector3(inputHandler.MovementInput.x, 0, inputHandler.MovementInput.y));
        moveDirection.y = 0;
        characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
    }

    private void ApplyGravity()
    {
        if (characterController.isGrounded)
        {
            verticalVelocity = -0.5f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
        characterController.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }

    private void UpdateBodyPosition()
    {
        // Keep the character controller centered under the headset
        Vector3 headsetPosition = headset.localPosition;
        characterController.center = new Vector3(headsetPosition.x, characterController.center.y, headsetPosition.z);
    }
}