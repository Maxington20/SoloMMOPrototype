using System;
using System.Collections.Generic;

public interface ILootContainer
{
    event Action OnLootChanged;

    string LootDisplayName { get; }
    IReadOnlyList<InventorySlotData> LootSlots { get; }
    int GoldAmount { get; }
    int SlotCount { get; }

    bool CanBeLooted { get; }

    bool TryLootGold(PlayerInventory playerInventory);
    bool TryLootItem(int slotIndex, PlayerInventory playerInventory);
}