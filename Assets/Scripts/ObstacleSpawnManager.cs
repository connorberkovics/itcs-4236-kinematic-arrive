using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ObstacleSpawnManager : MonoBehaviour
{
    public event Action Spawned;

    [Header("Input Settings")]
    [SerializeField] private InputAction spawnAction;

    [Header("Obstacle Prefab")]
    [SerializeField] private GameObject obstacle;

    [Header("World Grid")]
    [SerializeField] private WorldGrid grid;


    private void OnEnable()
    {
        spawnAction.Enable();
        spawnAction.performed += SpawnObstacle;
    }

    private void OnDisable()
    {
        spawnAction.performed -= SpawnObstacle;
        spawnAction.Disable();
    }


    private void SpawnObstacle(InputAction.CallbackContext context)
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.CompareTag("Ground"))
        {
            // Determine spawn position and rotation
            Vector3 spawnPosition = new Vector3(hit.point.x, 0f, hit.point.z);
            Quaternion spawnRotation = Quaternion.Euler(Vector3.zero);

            // Spawn obstacle and update world grid
            var instance = Instantiate(obstacle, spawnPosition, spawnRotation);
            GridNode node = grid.NodeFromWorld(instance.transform.position);
            node.walkable = grid.NodeIsWalkable(node);

            Spawned?.Invoke();
        }
    }
}
