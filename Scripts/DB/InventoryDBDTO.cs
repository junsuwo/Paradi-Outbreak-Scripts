using System;
using System.Collections.Generic;

[Serializable]
public class InventoryItemDTO
{
    public string item_code;
    public int slot_index;
    public int item_count;
}

[Serializable]
public class InventoryLoadResponse
{
    public bool success;
    public InventoryItemDTO[] items;  
}

[Serializable]
public class InventorySavePayload
{
    public int userId;
    public InventoryItemDTO[] items;  
}
