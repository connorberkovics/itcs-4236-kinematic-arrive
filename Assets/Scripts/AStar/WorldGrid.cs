using System.Collections.Generic;
using UnityEngine;

public class WorldGrid : MonoBehaviour
{
    [Header("Grid Settings")]
    [Tooltip("Grid size in world units (x = width on X axis, y = depth on Z axis.")]
    [SerializeField] private Vector2 worldSize = new(50, 50);

    [Header("Node Settings")]
    [SerializeField] private float nodeSize = 2f;
    [Tooltip("Extra clearance so the character doesn't clip through walls.")]
    [SerializeField][Range(0.1f, 1f)] private float clearance = 0.9f;

    [Header("Obstacle Layer Mask")]
    [Tooltip("Layers that block movement.")]
    [SerializeField] private LayerMask obstacleMask;

    [Header("Debug Settings")]
    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private float perimeterCubeHeight = 0.1f;
    [SerializeField] private float nodeCubeHeight = 0.05f;
    [SerializeField] private float pathHeight = 0.2f;
    [SerializeField] private Color pathColor;
    [SerializeField] private Color blockColor;

    private GridNode[,] nodes;
    private List<Vector3> debugPath;

    public int SizeX { get; private set; }
    public int SizeZ { get; private set; }


    private void Awake()
    {
        Build();
    }


    private void OnDrawGizmos()
    {
        // Draw grid perimeter
        Gizmos.color = Color.yellow;
        Vector3 perimeterDimensions = new(worldSize.x, perimeterCubeHeight, worldSize.y);
        Gizmos.DrawWireCube(transform.position, perimeterDimensions);

        if (!drawGizmos)
            return;

        // Draw individual nodes
        if (nodes != null)
        {
            foreach (GridNode node in nodes)
            {
                // Cube color and size
                Gizmos.color = node.walkable ? pathColor : blockColor;
                Vector3 cubeSize = new Vector3(1, nodeCubeHeight, 1) * (nodeSize * clearance);

                Gizmos.DrawCube(node.worldPosition, cubeSize);
            }
        }

        if (debugPath != null)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < debugPath.Count - 1; i++)
            {
                Gizmos.DrawLine(
                    debugPath[i] + Vector3.up * pathHeight,
                    debugPath[i + 1] + Vector3.up * pathHeight);
            }
        }
    }


    public void Build()
    {
        SizeX = Mathf.Max(1, Mathf.RoundToInt(worldSize.x / nodeSize));
        SizeZ = Mathf.Max(1, Mathf.RoundToInt(worldSize.y / nodeSize));
        nodes = new GridNode[SizeX, SizeZ];

        Vector3 bottomLeft = transform.position
            - Vector3.right * worldSize.x / 2f
            - Vector3.forward * worldSize.y / 2f;
        float radius = nodeSize * 0.5f * clearance;

        for (int x = 0; x < SizeX; x++)
        {
            for (int z = 0; z < SizeZ; z++)
            {
                Vector3 pos = bottomLeft
                    + Vector3.right * (x * nodeSize + nodeSize * 0.5f)
                    + Vector3.forward * (z * nodeSize + nodeSize * 0.5f);
                bool walkable = !Physics.CheckSphere(pos, radius, obstacleMask);

                nodes[x, z] = new GridNode(x, z, pos, walkable);
            }
        }
    }


    public GridNode NodeFromWorld(Vector3 position)
    {
        float xPos = (position.x - (transform.position.x - worldSize.x / 2f)) / worldSize.x;
        float zPos = (position.z - (transform.position.z - worldSize.y / 2f)) / worldSize.y;
        int x = Mathf.Clamp(Mathf.FloorToInt(xPos * SizeX), 0, SizeX - 1);
        int z = Mathf.Clamp(Mathf.FloorToInt(zPos * SizeZ), 0, SizeZ - 1);

        return nodes[x, z];
    }


    public bool InBounds(int x, int z) => x >= 0 && z >= 0 && x < SizeX && z < SizeZ;


    public void SetDebugPath(List<Vector3> path) => debugPath = path;


    public List<GridNode> GetNeighbors(GridNode node)
    {
        List<GridNode> neighbors = new();

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dz = -1; dz <= 1; dz++)
            {
                // 'node' parameter has coords [dx,dz], so skip it
                if (dx == 0 && dz == 0) continue;
                int neighX = node.x + dx, neighZ = node.z + dz;

                // Neighbors must be in-bounds
                if (!InBounds(neighX, neighZ)) continue;

                GridNode neighbor = nodes[neighX, neighZ];

                // Neighbors must be walkable
                if (!neighbor.walkable) continue;

                bool diagonal = dx != 0 && dz != 0;

                // For diagonal neighbors, both adjacent orthagonal cells must be free
                if (diagonal &&
                    (!nodes[node.x + dx, node.z].walkable ||
                    !nodes[node.x, node.z + dz].walkable))
                    continue;

                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }


    public bool NodeIsWalkable(GridNode node)
    {
        float radius = nodeSize * 0.5f * clearance;
        return !Physics.CheckSphere(node.worldPosition, radius, obstacleMask);
    }
}
