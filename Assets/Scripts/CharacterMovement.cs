using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CharacterMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float maxSpeed = 25f;
    [SerializeField] private float turnSpeed = 20f;
    [SerializeField] private float timeToTarget = 0.5f;

    [Header("Target Settings")]
    [SerializeField] private float satisfactionRadius = 2f;
    [SerializeField] private Transform targetTransform;
    [SerializeField] private TargetController targetController;

    private CharacterController controller;
    private Vector3 movementVelocity;
    

    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }


    private void OnEnable()
    {
        // Notify the character whenever the target position changes
        targetController.OnTargetPositionChanged += CalculateVelocity;
    }


    private void OnDisable()
    {
        // Unsubscribe on disable to prevent an error
        targetController.OnTargetPositionChanged -= CalculateVelocity;
    }


    private void Update()
    {
        MoveAndTurn();
    }


    // Handles character movement and orientation
    private void MoveAndTurn()
    {
        // Calculate the vector pointing from character to target
        Vector3 towardsTarget = targetTransform.position - transform.position;

        // Move towards target if we're outside the radius of satisfaction
        if (towardsTarget.magnitude > satisfactionRadius)
        {
            // Get direction
            Vector3 direction = towardsTarget.normalized;

            // Rotate towards target
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

            // Move the character using calculated velocity
            controller.Move(movementVelocity * Time.deltaTime);
        }
    }


    private void CalculateVelocity(object sender, EventArgs e)
    {
        // Get the vector pointing from character towards target
        Vector3 towardsTarget = targetTransform.position - transform.position;

        // Character will move only if it's outside the satisfaction radius
        if (towardsTarget.magnitude < satisfactionRadius)
        {
            movementVelocity = Vector3.zero;
            return;
        }

        // Calculate velocity
        movementVelocity = towardsTarget / timeToTarget;

        // If the character is moving too fast, clamp its velocity
        if (movementVelocity.magnitude > maxSpeed)
        {
            movementVelocity = movementVelocity.normalized * maxSpeed;
        }
    }
}
