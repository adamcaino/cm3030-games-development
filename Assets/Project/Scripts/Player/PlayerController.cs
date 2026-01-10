using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private LayerMask groundLayer;

    [Header("VFX Settings")]
    [SerializeField] GameObject moveVFXPrefab; // Prefab for movement visual effects (e.g., when clicking to move)

    CharacterController characterController;
    Camera mainCamera;

    Vector2 moveInput;
    Vector3 targetPosition;
    bool isMovingToTarget;
    private float verticalVelocity;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        mainCamera = Camera.main;
    }

    void Update()
    {
        // Apply gravity
        verticalVelocity = characterController.isGrounded ? -2f : verticalVelocity + gravity * Time.deltaTime;

        // Handle movement logic
        if (isMovingToTarget) // Are we moving to a clicked target?
        {
            MoveToTarget();
        }
        else if (moveInput != Vector2.zero) // Are we using keyboard input?
        {
            MoveWithKeyboard();
        }
    }

    void SpawnMoveVFX(Vector3 position)
    {
        if (moveVFXPrefab != null) // Check if a prefab is assigned before trying to instantiate it
        {
            Instantiate(moveVFXPrefab, position, moveVFXPrefab.transform.rotation);
        }
        else
        {
            Debug.LogWarning("PlayerController: Move VFX Prefab is not assigned.");
        }
    }

    void MoveToTarget()
    {
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f; // Keep movement horizontal

        // Check if we've reached the target
        if (direction.magnitude < 0.1f)
        {
            // Stop moving
            isMovingToTarget = false;
            return;
        }

        // Normalize vector and apply movement
        direction.Normalize();
        Vector3 moveVector = direction * moveSpeed * Time.deltaTime;

        moveVector.y = verticalVelocity * Time.deltaTime; // Apply gravity to remain grounded

        characterController.Move(moveVector); // Move the player

        // Rotate to face direction of movement
        if (direction.magnitude > 0.5f)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(direction),
                Time.deltaTime * 10f
            );
        }
    }

    void MoveWithKeyboard()
    {
        // Transform direction relative to camera orientation
        // In other words, "forwards" is where the camera is facing rather than the player
        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;

        // Reset y components to zero to keep movement horizontal
        cameraForward.y = 0f;
        cameraRight.y = 0f;

        // Normalize vectors
        cameraForward.Normalize();
        cameraRight.Normalize();

        // Calculate the final move direction
        Vector3 moveDirection = cameraForward * moveInput.y + cameraRight * moveInput.x;

        // Apply movement with gravity
        Vector3 moveVector = moveDirection * moveSpeed * Time.deltaTime;
        moveVector.y = verticalVelocity * Time.deltaTime; // Apply gravity
        characterController.Move(moveVector);

        // Rotate player to face movement direction
        if (moveDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(moveDirection),
                Time.deltaTime * 10f
            );
        }
    }

    #region InputSystemCallbacks

    // Player Input Callbacks
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        isMovingToTarget = false; // This cancels mouse-based movement when using the keyboard
    }

    public void OnMouseClick(InputValue value)
    {
        if (value.isPressed)
        {
            Vector3 mousePos = Mouse.current.position.ReadValue();
            Ray ray = mainCamera.ScreenPointToRay(mousePos);

            // Raycast to find where the mouse clicked on the ground
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
            {
                // Set target position and spawn VFX at the hit point
                targetPosition = hit.point;
                isMovingToTarget = true;
                SpawnMoveVFX(hit.point);
            }
        }
    }

    #endregion
}
