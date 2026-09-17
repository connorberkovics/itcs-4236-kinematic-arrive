using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class TargetController : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private InputAction moveAction;

    // This event fires whenever the target's position updates
    public event EventHandler OnTargetPositionChanged;


    private void OnEnable()
    {
        // Enable input actions and target movement
        moveAction.Enable();
        moveAction.performed += UpdateTargetPosition;
    }


    private void OnDisable()
    {
        // Disable target movement and input actions
        moveAction.performed -= UpdateTargetPosition;
        moveAction.Disable();
    }


    // Clicking on the plane moves the target
    private void UpdateTargetPosition(InputAction.CallbackContext context)
    {
        // Draw a ray pointing from the camera towards the mouse position
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        // If the ray hits the ground...
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.CompareTag("Ground"))
        {
            // Update target's position and notify the character
            transform.position = new Vector3(hit.point.x, transform.position.y, hit.point.z);
            OnTargetPositionChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}