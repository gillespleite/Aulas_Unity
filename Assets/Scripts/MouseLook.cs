using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLookDirect : MonoBehaviour
{
    [Header("Settings")]
    public float lookSensitivity = 0.1f;
    public Transform cameraTransform;

    private float pitch = 0f;

    void Start()
    {
        // Lock and hide the cursor for the FPS experience
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Auto-assign camera if left empty in Inspector
        if (cameraTransform == null && GetComponentInChildren<Camera>() != null)
        {
            cameraTransform = GetComponentInChildren<Camera>().transform;
        }
    }

    void Update()
    {
        // 1. Get raw pixel delta from the New Input System
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        // 2. Horizontal Rotation (Yaw) 
        // We rotate the ENTIRE player object around the Y-axis (Up)
        float yaw = mouseDelta.x * lookSensitivity;
        transform.Rotate(Vector3.up * yaw);

        // 3. Vertical Rotation (Pitch)
        // We only rotate the CAMERA object around its local X-axis
        pitch -= mouseDelta.y * lookSensitivity;
        pitch = Mathf.Clamp(pitch, -89f, 89f); // Prevent the player from doing a backflip

        cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}