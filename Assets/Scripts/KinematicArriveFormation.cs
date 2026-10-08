using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class KinematicArriveFormation : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float maxSpeed = 7f;
    [SerializeField] private float turnSpeed = 5f;
    [SerializeField] private float obstacleBumpSpeed = 0.1f;
    [SerializeField] private float timeToReachTarget = 0.5f;

    [Header("Target Settings")]
    [SerializeField] private float satisfactionRadius = 1f;
    [SerializeField] private Transform targetTransform;

    [Header("Formation Settings")]
    [SerializeField] private float distanceOffset;
    [SerializeField] private float angleOffset;

    private CharacterController controller;
    private Vector3 velocity;
    private Quaternion rotationOffset;
    private float gravity = -9.81f;


    private void Start()
    {
        controller = GetComponent<CharacterController>();

        // Angle offset to quaternion
        rotationOffset = Quaternion.Euler(0f, angleOffset, 0f);
    }


    private void Update()
    {
        KinematicArrive();
    }


    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Only register collisions with obstacles
        if (!hit.gameObject.CompareTag("Obstacle")) {
            return;
        }

        // Get vector from character to obstacle
        Vector3 toObstacle = hit.transform.position - transform.position;
        toObstacle.Normalize();
        toObstacle.y = 0f;

        // Dot product determines if the obstacle is left/right of the character
        float dotProd = Vector3.Dot(transform.right, toObstacle);
        Vector3 bump;

        // Dot prod < 0 -> obstacle on the left, push character right
        if (dotProd < 0f)
        {
            bump = transform.right * obstacleBumpSpeed;
        }

        // Dot prod > 0 -> obstacle on the right, push character left
        else
        {
            bump = -1f * obstacleBumpSpeed * transform.right;
        }

        // Nudge character
        controller.Move(bump);
    }


    /// Returns a Vector3 pointing from character's position to target's position.
    private Vector3 GetTowards()
    {
        Vector3 targetPosition = targetTransform.position;
        Vector3 forward = targetTransform.forward * distanceOffset;
        targetPosition += rotationOffset * forward;

        Vector3 towards = targetPosition - transform.position;
        towards.y = 0f;

        return towards;
    }


    private void KinematicArrive()
    {
        // Get the vector pointing from character to target
        Vector3 towards = GetTowards();

        // Turn to face target
        Quaternion targetRotation = Quaternion.LookRotation(towards);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

        // Stop moving if too close
        if (towards.magnitude < satisfactionRadius)
        {
            velocity = Vector3.zero;
            return;
        }

        // Otherwise, calculate velocity
        else
        {
            Vector3 potentialVelocity = towards / timeToReachTarget;
            velocity = (potentialVelocity.magnitude > maxSpeed) ? potentialVelocity.normalized * maxSpeed : potentialVelocity;
        }

        // Keep character grounded
        velocity.y = gravity;

        // Move to target
        controller.Move(velocity * Time.deltaTime);
    }
}
