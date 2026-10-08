using UnityEngine;
using UnityEngine.InputSystem;

public class ObstacleSpawnManager : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private InputAction spawnAction;

    [Header("Obstacle Prefab")]
    [SerializeField] private GameObject obstacle;


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

            // Instantiate obstacle prefab
            Instantiate(obstacle, spawnPosition, spawnRotation);
        }
    }
}
