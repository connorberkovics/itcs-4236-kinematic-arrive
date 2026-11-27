using System.Collections.Generic;
using UnityEngine;

public class NodeHeap
{
    private readonly List<GridNode> items = new();

    public int Count => items.Count;


    public void Clear() => items.Clear();


    public void Add(GridNode node)
    {
        node.heapIndex = items.Count;
        items.Add(node);

        SiftUp(node.heapIndex);
    }


    public GridNode RemoveFirst()
    {
        GridNode first = items[0];
        int last = items.Count - 1;

        items[0] = items[last];
        items[0].heapIndex = 0;
        items.RemoveAt(last);

        if (items.Count > 0)
            SiftDown(0);

        return first;
    }


    public void Update(GridNode node) => SiftUp(node.heapIndex);


    private static bool Less(GridNode a, GridNode b) => a.LessThan(b);


    private void SiftUp(int index)
    {
        while (index > 0)
        {
            int parent = (index - 1) / 2;

            if (!Less(items[index], items[parent]))
                break;

            Swap(index, parent);
            index = parent;
        }
    }


    private void SiftDown(int index)
    {
        while (true)
        {
            int left = index * 2 + 1;
            int right = left + 1;
            int smallest = index;

            if (left < items.Count && Less(items[left], items[smallest]))
                smallest = left;
            if (right < items.Count && Less(items[right], items[smallest]))
                smallest = right;
            if (smallest == index)
                return;

            Swap(index, smallest);
            index = smallest;
        }
    }


    private void Swap(int a, int b)
    {
        GridNode temp = items[a];
        items[a] = items[b];
        items[b] = temp;

        items[a].heapIndex = a;
        items[b].heapIndex = b;
    }
}
