using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WorldGrid))]
public class AStarPathfinder : MonoBehaviour
{
    private const int STRAIGHT_COST = 10;
    private const int DIAGONAL_COST = 14;
    private const int MIN_PATH_LENGTH = 3;

    [Tooltip("Remove intermediate points along straight segments.")]
    [SerializeField] private bool simplifyPath = true;

    private WorldGrid grid;
    private readonly NodeHeap open = new();
    private int currentSearch;


    private void Awake()
    {
        grid = GetComponent<WorldGrid>();
    }


    public List<Vector3> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        GridNode start = grid.NodeFromWorld(startPos);
        GridNode target = grid.NodeFromWorld(targetPos);

        // Start and target must be walkable
        if (!start.walkable || !target.walkable) return null;

        currentSearch++;
        open.Clear();

        // Initialize start node
        Init(start);
        start.gCost = 0;
        start.hCost = Heuristic(start, target);
        open.Add(start);

        // A* algorithm
        while (open.Count > 0)
        {
            // Pop frontmost node
            GridNode current = open.RemoveFirst();
            current.closed = true;

            // Have we found the target yet?
            if (current == target)
            {
                // Target found -> generate path
                List<Vector3> path = Retrace(start, target, startPos, targetPos);
                grid.SetDebugPath(path);
                return path;
            }

            // Target not found -> add neighbors to open list
            foreach (GridNode neighbor in grid.GetNeighbors(current))
            {
                Init(neighbor);
                if (neighbor.closed) continue;

                // Calculate G-cost
                int dx = neighbor.x - current.x, dz = neighbor.z - current.z;
                bool diagonal = dx != 0 && dz != 0;
                int newG = current.gCost + (diagonal ? DIAGONAL_COST : STRAIGHT_COST);
                bool inOpen = neighbor.heapIndex >= 0;

                // Update open list if neighbor hasn't been added or a better G-cost is found
                if (!inOpen || newG < neighbor.gCost)
                {
                    neighbor.gCost = newG;
                    neighbor.hCost = Heuristic(neighbor, target);
                    neighbor.parent = current;
                    if (inOpen) open.Update(neighbor);
                    else open.Add(neighbor);
                }
            }
        }

        // Path not found
        grid.SetDebugPath(null);
        return null;
    }


    private void Init(GridNode node)
    {
        if (node.searchId == currentSearch) return;

        node.searchId = currentSearch;
        node.gCost = int.MaxValue;
        node.hCost = 0;
        node.parent = null;
        node.closed = false;
        node.heapIndex = -1;
    }


    private static int Heuristic(GridNode a, GridNode b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dz = Mathf.Abs(a.z - b.z);

        return DIAGONAL_COST * Mathf.Min(dx, dz)
            + STRAIGHT_COST * Mathf.Abs(dx - dz);
    }


    private List<Vector3> Retrace(GridNode start, GridNode end, Vector3 startPos, Vector3 targetPos)
    {
        List<GridNode> nodes = new();

        // Starting at the end node, iterate backwards
        for (GridNode node = end; node != start; node = node.parent)
        {
            nodes.Add(node);
        }

        // Add start node and reverse list (i.e., list now goes start -> ... -> end)
        nodes.Add(start);
        nodes.Reverse();

        // Initialize list of path nodes' positions
        List<Vector3> path = new(nodes.Count);
        float y = startPos.y;

        // Add nodes to path
        foreach (GridNode node in nodes)
        {
            path.Add(new Vector3(node.worldPosition.x, y, node.worldPosition.z));
        }

        // Land on requested target position instead of node center
        path[path.Count - 1] = new Vector3(targetPos.x, y, targetPos.z);

        return simplifyPath ? Simplify(path) : path;
    }


    private static List<Vector3> Simplify(List<Vector3> path)
    {
        // Paths with < 3 nodes cannot be simplified
        if (path.Count < MIN_PATH_LENGTH) return path;

        List<Vector3> result = new() { path[0] };
        Vector2 lastDir = Vector2.zero;

        for (int i = 1; i < path.Count; i++)
        {
            Vector2 dir = new Vector2(
                path[i].x - path[i - 1].x,
                path[i].z - path[i - 1].z)
                .normalized;

            if (i > 1 && (dir - lastDir).sqrMagnitude > 0.0001f)
                result.Add(path[i - 1]);
            lastDir = dir;
        }
        result.Add(path[path.Count - 1]);
        return result;
    }
}
