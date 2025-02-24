using UnityEngine.Playables;
using UnityEngine;
using Mirror;

public class PCPlayerMovementController : MonoBehaviour
{
    [SerializeField] private Settings settings;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Mouse Look Settings")]
    [SerializeField] private float maxLookAngle = 80f;


    [SerializeField] private float coyoteTime = 0.2f; // Grace period after falling
    [SerializeField] private float jumpBufferTime = 0.2f; // Allow jump before landing
    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    private PCInputHandler inputHandler;
    private CharacterController characterController;
    private Camera playerCamera;
    private float verticalVelocity;
    private float cameraPitch;

    private void Start()
    {


        inputHandler = GetComponent<PCInputHandler>();
        characterController = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();

        // Add check for network authority
        if (!GetComponent<NetworkIdentity>().isLocalPlayer)
        {
            // Disable input handling for non-local players
            enabled = false;
            inputHandler.enabled = false;
            return;
        }

        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleMovement();
        HandleMouseLook();
        HandleJump();
    }

    private void HandleMovement()
    {
        Vector3 moveDirection = transform.right * inputHandler.MovementInput.x +
                              transform.forward * inputHandler.MovementInput.y;

        float currentSpeed = inputHandler.SprintingInput ? sprintSpeed : moveSpeed;
        characterController.Move(moveDirection * currentSpeed * Time.deltaTime);
    }

    private void HandleMouseLook()
    {
        // Horizontal rotation
        transform.Rotate(Vector3.up * inputHandler.MouseInput.x * settings.Sensitivity);

        // Vertical rotation (camera pitch)
        cameraPitch -= inputHandler.MouseInput.y * settings.Sensitivity;
        cameraPitch = Mathf.Clamp(cameraPitch, -maxLookAngle, maxLookAngle);
        playerCamera.transform.localRotation = Quaternion.Euler(cameraPitch, 0, 0);
    }



    private void HandleJump()
    {
        if (inputHandler.JumpInput)        
            jumpBufferCounter = jumpBufferTime;        
        else
            jumpBufferCounter -= Time.deltaTime;

        characterController.Move(Vector3.up * verticalVelocity * Time.deltaTime);
        if (characterController.isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            verticalVelocity = -0.5f; // Small downward force when grounded

        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
            verticalVelocity += gravity * Time.deltaTime;
        }

        if (coyoteTimeCounter > 0 && jumpBufferCounter > 0)
        {
            verticalVelocity = jumpForce;
            jumpBufferCounter = 0;
            coyoteTimeCounter = 0;
        }
    }
}