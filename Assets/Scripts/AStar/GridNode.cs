using UnityEngine;

public class GridNode
{
    // Grid Data
    public int x;
    public int z;
    public Vector3 worldPosition;
    public bool walkable;

    // A* Data
    public int gCost;
    public int hCost;
    public GridNode parent;
    public int heapIndex;
    public int searchId;
    public bool closed;

    public int FCost => gCost + hCost;


    public GridNode(int x, int z, Vector3 worldPosition, bool walkable)
    {
        this.x = x;
        this.z = z;
        this.worldPosition = worldPosition;
        this.walkable = walkable;

        // Initialize A* Data
        gCost = int.MaxValue;
        hCost = 0;
        parent = null;
        closed = false;
        heapIndex = -1;
    }


    /// <summary>
    /// Compares two GridNode objects based on their F-costs.
    /// If there is a tie, compares H-costs instead.
    /// </summary>
    /// <param name="other">Node to compare.</param>
    /// <returns>True if this node's F-cost is less than 'other', or there
    /// is a tie and this node's G-cost is less than 'other'.</returns>
    public bool LessThan(GridNode other)
    {
        // Tie-breaker (G-cost comparison)
        if (this.FCost == other.FCost)
            return this.hCost < other.hCost;

        // F-cost comparison
        return this.FCost < other.FCost;
    }


    /// <summary>
    /// Compares two GridNode objects based on their x and z indices.
    /// </summary>
    /// <param name="other">Node to compare.</param>
    /// <returns>True if x and z values are the same; false otherwise.</returns>
    public bool IndicesMatch(GridNode other) => this.x == other.x && this.z == other.z;
}
