using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] Transform target; // The transform to follow (e.g., the player)

    [Header("Camera Settings")]
    [SerializeField] float distance = 10f; // Distance from target
    // Min and max zoom distances
    [SerializeField] float minDistance = 3f;
    [SerializeField] float maxDistance = 20f;
    [SerializeField] float zoomSpeed = 0.01f; // The speed of zooming in/out
    [SerializeField] float smoothSpeed = 5f; // How smoothly the camera follows

    [Header("Camera Angle")]
    [SerializeField] float verticalAngle = 45f; // Vertical angle of camera (in degrees)

    [Header("Orbit Settings")]
    [SerializeField] float orbitSpeed = 0.1f; // How fast the camera orbits
    [SerializeField] bool invertOrbitHorizontal = false; // Inverts left/right orbit direction
    [SerializeField] bool invertOrbitVertical = false; // Inverts up/down orbit direction
    [SerializeField] float minVerticalAngle = 10f; // Minimum vertical angle (looking down)
    [SerializeField] float maxVerticalAngle = 80f; // Maximum vertical angle (looking up)

    Camera mainCamera;
    float scrollInput;
    float currentAngle = 0f; // Current horizontal orbit angle
    float currentVerticalAngle; // Current vertical orbit angle
    bool isOrbiting = false; // Is right mouse button held down?
    Vector2 mouseDelta;

    void Start()
    {
        mainCamera = GetComponent<Camera>(); // Gets the Camera component

        // Initialize vertical angle to the starting angle
        currentVerticalAngle = verticalAngle;

        // If no target is set, try to find the player
        if (target == null)
        {
            GameObject lookAtTarget = GameObject.FindGameObjectWithTag("CameraLookAtTarget");
            if (lookAtTarget != null)
            {
                // Assign the player's transform as the target
                target = lookAtTarget.transform;
            }
            else
            {
                // Log a warning if no player is found
                Debug.LogWarning("CameraController: No target assigned and no GameObject with 'CameraLookAtTarget' tag found.");
            }
        }
    }

    void LateUpdate()
    {
        if (target == null) return; // Exit if no target to follow

        // Handle zoom input
        HandleZoom();

        // Handle orbit input
        HandleOrbit();
    }

    private void HandleOrbit()
    {
        // Calculate orbit angles (horizontal and vertical)
        if (isOrbiting)
        {
            // Horizontal orbit
            if (mouseDelta.x != 0f)
            {
                float orbitDirection = invertOrbitHorizontal ? -1f : 1f; // Allows inversion of horizontal orbit based on user preference
                currentAngle += mouseDelta.x * orbitSpeed * orbitDirection;
            }

            // Vertical orbit
            if (mouseDelta.y != 0f)
            {
                float verticalDirection = invertOrbitVertical ? 1f : -1f;
                currentVerticalAngle += mouseDelta.y * orbitSpeed * verticalDirection; // Allows inversion of vertical orbit based on user preference
                currentVerticalAngle = Mathf.Clamp(currentVerticalAngle, minVerticalAngle, maxVerticalAngle);
            }
        }

        // Calculate camera position using spherical coordinates
        // This ensures zooming moves in a straight line towards/away from target
        Vector3 lookAtPoint = target.position;

        // Convert angles to radians
        float horizontalRad = currentAngle * Mathf.Deg2Rad;
        float verticalRad = currentVerticalAngle * Mathf.Deg2Rad;

        // Calculate direction (from 'lookAtPoint' to camera) using spherical coordinates
        Vector3 direction = new Vector3(
            Mathf.Sin(horizontalRad) * Mathf.Cos(verticalRad),
            Mathf.Sin(verticalRad),
            -Mathf.Cos(horizontalRad) * Mathf.Cos(verticalRad) // Negative Z as the camera is behind the target
        );

        // Position the camera at the distance along the direction calculated above
        Vector3 targetPosition = lookAtPoint + direction * distance;

        // While orbiting, move directly to maintain distance - otherwise use a smooth follow with lerp.
        if (isOrbiting)
        {
            transform.position = targetPosition;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
        }

        // Always look at the target
        transform.LookAt(lookAtPoint);
    }

    private void HandleZoom()
    {
        // Handle scroll wheel input
        if (scrollInput != 0f)
        {
            distance -= scrollInput * zoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }
    }

    #region InputSystemCallbacks

    // Input System callback for scroll wheel
    public void OnZoom(InputValue value)
    {
        scrollInput = value.Get<float>();
    }

    // Input System callback for right mouse button
    public void OnOrbit(InputValue value)
    {
        // For Button actions we need to check if its in a 'pressed' or 'released' state (a boolean)
        // value.isPressed returns true on press, false on release. Configured as an "axis" type in the Input System, as this returns 0 or a 1.
        isOrbiting = value.isPressed;
    }

    // Input System callback for mouse delta (movement)
    public void OnLook(InputValue value)
    {
        if (isOrbiting)
        {
            mouseDelta = value.Get<Vector2>();
        }
    }

    #endregion
}
