using System;
using UnityEngine;

[Serializable]
public struct InventoryItem
{
    public ItemData data;
    public int count;

    public bool IsEmpty => data == null || count <= 0;

    public InventoryItem(ItemData data, int count)
    {
        this.data = data;
        this.count = count;
    }
}
