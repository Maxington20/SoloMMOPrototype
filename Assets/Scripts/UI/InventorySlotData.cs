using System;

[Serializable]
public class InventorySlotData
{
    public bool IsEmpty = true;

    // Placeholder for future item support
    public string ItemId = string.Empty;
    public int Quantity = 0;

    public void Clear()
    {
        IsEmpty = true;
        ItemId = string.Empty;
        Quantity = 0;
    }
}