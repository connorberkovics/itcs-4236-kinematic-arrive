using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class TargetController : MonoBehaviour
{
    public event EventHandler<PositionChangedArgs> TargetPositionChanged;

    [Header("Input Settings")]
    [SerializeField] private InputAction moveAction;


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
            Vector3 oldPos = transform.position;
            Vector3 newPos = new Vector3(hit.point.x, 0f, hit.point.z);
            transform.position = newPos;
            TargetPositionChanged?.Invoke(this, new PositionChangedArgs(oldPos, newPos));
        }
    }


    /// <summary>
    /// Event arguments for TargetPositionChanged event.
    /// </summary>
    public class PositionChangedArgs : EventArgs
    {
        public readonly Vector3 oldPos;
        public readonly Vector3 newPos;


        public PositionChangedArgs(Vector3 oldPos, Vector3 newPos)
        {
            this.oldPos = oldPos;
            this.newPos = newPos;
        }
    }
}