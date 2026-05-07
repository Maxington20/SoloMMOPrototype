using System.Collections.Generic;
using UnityEngine;

public static class InventoryTransactionService
{
    public static int GetTotalQuantityOfItem(IReadOnlyList<InventorySlotData> slots, ItemData item)
    {
        if (slots == null || item == null)
        {
            return 0;
        }

        int total = 0;

        for (int i = 0; i < slots.Count; i++)
        {
            InventorySlotData slot = slots[i];
            if (slot != null && !slot.IsEmpty && slot.Item == item)
            {
                total += slot.Quantity;
            }
        }

        return total;
    }

    public static bool CanAddItem(IReadOnlyList<InventorySlotData> slots, ItemData item, int quantity)
    {
        if (slots == null || item == null || quantity <= 0)
        {
            return false;
        }

        int remaining = quantity;
        int maxStack = item.IsStackable ? item.MaxStack : 1;

        if (item.IsStackable)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                InventorySlotData slot = slots[i];
                if (slot == null || !slot.CanStack(item))
                {
                    continue;
                }

                int freeSpace = maxStack - slot.Quantity;
                remaining -= freeSpace;

                if (remaining <= 0)
                {
                    return true;
                }
            }
        }

        for (int i = 0; i < slots.Count; i++)
        {
            InventorySlotData slot = slots[i];
            if (slot != null && slot.IsEmpty)
            {
                remaining -= maxStack;

                if (remaining <= 0)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static bool AddItem(IList<InventorySlotData> slots, ItemData item, int quantity)
    {
        if (slots == null || item == null || quantity <= 0)
        {
            return false;
        }

        if (!CanAddItem((IReadOnlyList<InventorySlotData>)slots, item, quantity))
        {
            return false;
        }

        int remaining = quantity;

        if (item.IsStackable)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                InventorySlotData slot = slots[i];
                if (slot == null || !slot.CanStack(item))
                {
                    continue;
                }

                int freeSpace = item.MaxStack - slot.Quantity;
                if (freeSpace <= 0)
                {
                    continue;
                }

                int amountToAdd = Mathf.Min(freeSpace, remaining);
                slot.Quantity += amountToAdd;
                remaining -= amountToAdd;

                if (remaining <= 0)
                {
                    break;
                }
            }
        }

        for (int i = 0; i < slots.Count; i++)
        {
            if (remaining <= 0)
            {
                break;
            }

            InventorySlotData slot = slots[i];
            if (slot == null || !slot.IsEmpty)
            {
                continue;
            }

            int stackSize = item.IsStackable ? Mathf.Min(item.MaxStack, remaining) : 1;
            slot.Set(item, stackSize);
            remaining -= stackSize;
        }

        return remaining <= 0;
    }

    public static bool CanRemoveItem(IReadOnlyList<InventorySlotData> slots, ItemData item, int quantity)
    {
        if (slots == null || item == null || quantity <= 0)
        {
            return false;
        }

        return GetTotalQuantityOfItem(slots, item) >= quantity;
    }

    public static bool RemoveItem(IList<InventorySlotData> slots, ItemData item, int quantity)
    {
        if (slots == null || item == null || quantity <= 0)
        {
            return false;
        }

        if (!CanRemoveItem((IReadOnlyList<InventorySlotData>)slots, item, quantity))
        {
            return false;
        }

        int remaining = quantity;

        for (int i = 0; i < slots.Count; i++)
        {
            InventorySlotData slot = slots[i];

            if (slot == null || slot.IsEmpty || slot.Item != item)
            {
                continue;
            }

            int amountToRemove = Mathf.Min(slot.Quantity, remaining);
            slot.Quantity -= amountToRemove;
            remaining -= amountToRemove;

            if (slot.Quantity <= 0)
            {
                slot.Clear();
            }

            if (remaining <= 0)
            {
                break;
            }
        }

        return remaining <= 0;
    }

    public static void RemoveFromSlot(InventorySlotData slot, int quantity)
    {
        if (slot == null || slot.IsEmpty || quantity <= 0)
        {
            return;
        }

        slot.Quantity -= quantity;

        if (slot.Quantity <= 0)
        {
            slot.Clear();
        }
    }

    public static bool MoveOrSwap(IList<InventorySlotData> slots, int sourceIndex, int targetIndex)
    {
        if (slots == null || sourceIndex < 0 || sourceIndex >= slots.Count || targetIndex < 0 || targetIndex >= slots.Count)
        {
            return false;
        }

        if (sourceIndex == targetIndex)
        {
            return false;
        }

        InventorySlotData sourceSlot = slots[sourceIndex];
        InventorySlotData targetSlot = slots[targetIndex];

        if (sourceSlot == null || targetSlot == null || sourceSlot.IsEmpty || sourceSlot.Item == null)
        {
            return false;
        }

        if (targetSlot.IsEmpty)
        {
            targetSlot.Set(sourceSlot.Item, sourceSlot.Quantity);
            sourceSlot.Clear();
            return true;
        }

        if (sourceSlot.Item == targetSlot.Item && sourceSlot.Item.IsStackable)
        {
            int maxStack = sourceSlot.Item.MaxStack;
            int freeSpace = maxStack - targetSlot.Quantity;

            if (freeSpace <= 0)
            {
                return false;
            }

            int amountToMove = Mathf.Min(freeSpace, sourceSlot.Quantity);
            targetSlot.Quantity += amountToMove;
            sourceSlot.Quantity -= amountToMove;

            if (sourceSlot.Quantity <= 0)
            {
                sourceSlot.Clear();
            }

            return amountToMove > 0;
        }

        ItemData sourceItem = sourceSlot.Item;
        int sourceQuantity = sourceSlot.Quantity;

        sourceSlot.Set(targetSlot.Item, targetSlot.Quantity);
        targetSlot.Set(sourceItem, sourceQuantity);

        return true;
    }
}