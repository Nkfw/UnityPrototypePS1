using UnityEngine;
using UnityEngine.InputSystem;

// This script goes on the Player
// Handles movement using the new Input System
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float rotationSpeed = 10f; // How fast player rotates to face movement direction
    [SerializeField] private float crouchSpeedMultiplier = 0.5f; // Speed reduction when crouching
    [SerializeField] private float runSpeedMultiplier = 2.2f;
    [SerializeField] private float carrySpeedMultiplier = 0.7f; // Speed reduction when carrying sheep
    [SerializeField] private Animator playerAnimator;

    // Current input from WASD or gamepad
    public Vector2 moveInput;

    // Movement states
    private bool isCrouching = false;
    private bool isRunning = false;
    private bool isCarryingSheep = false;


    // Reference to the CharacterController component
    private CharacterController characterController;

    // Vertical velocity for gravity and jumping
    private Vector3 velocity;

    // Cache main camera transform
    private Transform cameraTransform;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();

        playerAnimator = GetComponentInChildren<Animator>();

        // Cache the main camera transform for camera-relative movement
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
        else
        {
            Debug.LogWarning("No main camera found! Movement will use world space.");
        }
    }

    // Called automatically by the Input System when player presses WASD
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    // Called automatically by the Input System when player presses Jump
    public void OnJump(InputAction.CallbackContext context)
    {
        // Only jump when the button is pressed (not on release)
        if (context.performed && characterController.isGrounded)
        {
            // Calculate jump velocity using: v = sqrt(2 * jumpHeight * gravity)
            velocity.y = Mathf.Sqrt(jumpHeight * 2f * -gravity);

            // Trigger jump animation
            if (playerAnimator != null)
            {
                playerAnimator.SetBool("IsJumping", true);
            }
        }
    }

    // Called automatically by the Input System when player holds/releases Crouch
    public void OnCrouch(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isCrouching = true;
        }
        else if (context.canceled)
        {
            isCrouching = false;
        }
    }

    // Called automatically by the Input System when player holds/releases Run
    public void OnRun(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isRunning = true;
        }
        else if (context.canceled)
        {
            isRunning = false;
        }
    }


    private void Update()
    {
        // animation for walking and running
        float animationSpeed = 0f;

        if (moveInput.magnitude > 0.1f)
        {
            if (isRunning)
            {
                animationSpeed = 1f;
            }
            else
            {
                animationSpeed = 0.3f;
            } 
        }

        // jumping animation

        

        if (playerAnimator != null)
        {
            playerAnimator.SetFloat("Speed", animationSpeed);
        }

        MovePlayer();

        RotatePlayer();

        ApplyGravity();
    }

    private float GetCurrentSpeed()
    {
        float currentSpeed = speed;

        if (isCrouching)
        {
            currentSpeed *= crouchSpeedMultiplier;
        }

        if (isRunning)
        {
            currentSpeed *= runSpeedMultiplier;
        }

        if (isCarryingSheep)
        {
            currentSpeed *= carrySpeedMultiplier;
        }

        return currentSpeed;
    }

    // Called by PlayerInteraction when pickup/drop state changes
    public void SetCarryingState(bool carrying)
    {
        isCarryingSheep = carrying;
    }

    private void MovePlayer()
    {
        // No input = no movement
        if (moveInput == Vector2.zero)
        {
            return;
        }

        // Calculate camera-relative movement direction
        Vector3 moveDirection = GetCameraRelativeMovement();

        // Get current speed (includes crouch modifier and future modifiers)
        float currentSpeed = GetCurrentSpeed();

        // Apply movement (horizontal only)
        characterController.Move(moveDirection * currentSpeed * Time.deltaTime);
    }


    private Vector3 GetCameraRelativeMovement()
    {
        // If no camera, fall back to world-space movement
        if (cameraTransform == null)
        {
            return new Vector3(moveInput.x, 0f, moveInput.y);
        }

        // Get camera forward and right vectors
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;

        // Keep movement horizontal (ignore camera pitch)
        cameraForward.y = 0f;
        cameraRight.y = 0f;

        // Normalize to prevent speed changes
        cameraForward.Normalize();
        cameraRight.Normalize();

        // Calculate movement direction relative to camera
        // moveInput.y = forward/back (W/S), moveInput.x = left/right (A/D)
        Vector3 moveDirection = cameraForward * moveInput.y + cameraRight * moveInput.x;

        return moveDirection;
    }

    private void ApplyGravity()
    {
        // Apply small downward force when grounded to keep player "stuck" to ground
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;

            // Reset jump animation when landed
            if (playerAnimator != null)
            {
                bool wasJumping = playerAnimator.GetBool("IsJumping");
                if (wasJumping)
                {
                    playerAnimator.SetBool("IsJumping", false);
                }
            }
        }

        // Apply gravity acceleration
        velocity.y += gravity * Time.deltaTime;

        // Apply vertical movement
        characterController.Move(velocity * Time.deltaTime);
    }

    private void RotatePlayer()
    {
        // Only rotate if we're actually moving
        if (moveInput == Vector2.zero)
        {
            return;
        }

        // Get camera-relative movement direction
        Vector3 moveDirection = GetCameraRelativeMovement();

        // Only rotate if there's actual movement direction
        if (moveDirection.magnitude < 0.1f)
        {
            return;
        }

        // Calculate rotation to face movement direction
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

        // Smoothly rotate toward target rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}