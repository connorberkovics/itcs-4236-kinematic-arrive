using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class AStarMovement : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private TargetController targetController;
    [SerializeField] private Transform target;

    [Header("References")]
    [SerializeField] private AStarPathfinder pathfinder;
    [SerializeField] private WorldGrid grid;
    [SerializeField] private ObstacleSpawnManager obstacleSpawner;
    
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float turnSpeed = 540f;
    [SerializeField] private float waypointTolerance = 0.25f;
    [SerializeField] private float satisfactionRadius = 1.5f;

    private CharacterController controller;
    private List<Vector3> path;
    private int index;
    private float verticalVelocity;
    private readonly float gravity = -9.81f;
    private readonly float groundForce = -2f;


    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }


    private void OnEnable()
    {
        targetController.TargetPositionChanged += OnTargetPositionChanged;
        obstacleSpawner.Spawned += OnObstacleSpawned;
    }


    private void OnDisable()
    {
        targetController.TargetPositionChanged -= OnTargetPositionChanged;
        obstacleSpawner.Spawned -= OnObstacleSpawned;
    }


    private void Update()
    {
        if (target == null || pathfinder == null) return;

        ApplyMove(ComputeHorizontalMove());
    }


    private void FindNewPath()
    {
        // Generate and set path
        List<Vector3> newPath = pathfinder.FindPath(transform.position, target.position);
        if (newPath != null)
        {
            path = newPath;
            index = Mathf.Min(1, path.Count - 1); // 0 is leader's current node
        }
    }


    private Vector3 ComputeHorizontalMove()
    {
        if (path == null || index >= path.Count) return Vector3.zero;

        // Stop and face target if we're w/i radius of satisfaction
        Vector3 toTarget = Flat(target.position - transform.position);
        if (toTarget.magnitude <= satisfactionRadius)
        {
            Face(toTarget);
            return Vector3.zero;
        }

        // Move past reached waypoints
        while (index < path.Count && Flat(path[index] - transform.position).magnitude <= waypointTolerance)
            index++;

        if (index >= path.Count) return Vector3.zero;

        Vector3 to = Flat(path[index] - transform.position);
        Vector3 dir = to.normalized;
        Face(dir);

        return dir * moveSpeed;
    }


    private void ApplyMove(Vector3 horizontalVelocity)
    {
        // Keep character on the ground
        verticalVelocity = (controller.isGrounded && verticalVelocity < 0f)
            ? groundForce : verticalVelocity + gravity * Time.deltaTime;

        Vector3 velocity = horizontalVelocity;
        velocity.y = verticalVelocity;
        controller.Move(velocity * Time.deltaTime);
    }


    private void Face(Vector3 dir)
    {
        if (dir.sqrMagnitude < 0.0001f) return;

        // Rotate to face given direction
        Quaternion look = Quaternion.LookRotation(dir.normalized, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, look, turnSpeed * Time.deltaTime);
    }


    private static Vector3 Flat(Vector3 v) => new(v.x, 0f, v.z);


    private void OnTargetPositionChanged(object sender, TargetController.PositionChangedArgs e)
    {
        // Has the target moved to a new node?
        GridNode prevNode = grid.NodeFromWorld(e.oldPos);
        GridNode newNode = grid.NodeFromWorld(e.newPos);
        if (prevNode.IndicesMatch(newNode)) return;

        // Repath
        FindNewPath();
    }


    private void OnObstacleSpawned() => FindNewPath();
}
