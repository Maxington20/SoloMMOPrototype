using System;

[Serializable]
public class InventorySlotData
{
    public ItemData Item;
    public int Quantity;

    public bool IsEmpty => Item == null || Quantity <= 0;

    public bool CanStack(ItemData item)
    {
        return !IsEmpty && Item == item && item != null && item.IsStackable;
    }

    public void Set(ItemData item, int quantity)
    {
        Item = item;
        Quantity = quantity;

        if (Item == null || Quantity <= 0)
        {
            Clear();
        }
    }

    public void Clear()
    {
        Item = null;
        Quantity = 0;
    }
}