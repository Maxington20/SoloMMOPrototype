using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    public event Action<int> OnGoldChanged;
    public event Action OnInventoryChanged;

    [Header("Gold")]
    [SerializeField] private int startingGold = 0;

    [Header("Inventory")]
    [SerializeField] private int slotCount = 20;

    private readonly List<InventorySlotData> slots = new List<InventorySlotData>();
    private PlayerEquipment playerEquipment;
    private Health playerHealth;

    public int Gold { get; private set; }
    public int SlotCount => slotCount;
    public IReadOnlyList<InventorySlotData> Slots => slots;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        playerEquipment = GetComponent<PlayerEquipment>();
        playerHealth = GetComponent<Health>();

        Gold = Mathf.Max(0, startingGold);
        InitializeSlots();
    }

    private void Start()
    {
        OnGoldChanged?.Invoke(Gold);
        OnInventoryChanged?.Invoke();
    }

    private void InitializeSlots()
    {
        slots.Clear();

        int finalSlotCount = Mathf.Max(1, slotCount);

        for (int i = 0; i < finalSlotCount; i++)
        {
            slots.Add(new InventorySlotData());
        }
    }

    public void AddGold(int amount)
    {
        AddGold(amount, true);
    }

    public void AddGold(int amount, bool postLootMessage)
    {
        if (amount <= 0)
        {
            return;
        }

        Gold += amount;
        OnGoldChanged?.Invoke(Gold);

        if (postLootMessage)
        {
            PostSystem($"You loot {amount} gold.");
        }
    }

    public bool SpendGold(int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        if (Gold < amount)
        {
            return false;
        }

        Gold -= amount;
        OnGoldChanged?.Invoke(Gold);
        return true;
    }

    public InventorySlotData GetSlot(int index)
    {
        if (index < 0 || index >= slots.Count)
        {
            return null;
        }

        return slots[index];
    }

    public int GetTotalQuantityOfItem(ItemData item)
    {
        if (item == null)
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

    public bool CanAddItem(ItemData item, int quantity = 1)
    {
        if (item == null || quantity <= 0)
        {
            return false;
        }

        int remaining = quantity;
        int maxStack = item.IsStackable ? item.MaxStack : 1;

        if (item.IsStackable)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].CanStack(item))
                {
                    int freeSpace = maxStack - slots[i].Quantity;
                    remaining -= freeSpace;

                    if (remaining <= 0)
                    {
                        return true;
                    }
                }
            }
        }

        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].IsEmpty)
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

    public bool AddItem(ItemData item, int quantity = 1)
    {
        return AddItemInternal(item, quantity, true);
    }

    public bool TryBuyItem(ItemData item, int quantity = 1)
    {
        if (item == null || quantity <= 0)
        {
            return false;
        }

        int totalCost = item.BuyValue * quantity;

        if (totalCost <= 0)
        {
            PostSystem($"{item.DisplayName} cannot be purchased.");
            return false;
        }

        if (!CanAddItem(item, quantity))
        {
            PostSystem("Inventory is full.");
            return false;
        }

        if (!SpendGold(totalCost))
        {
            PostSystem("You do not have enough gold.");
            return false;
        }

        AddItemInternal(item, quantity, false);
        OnInventoryChanged?.Invoke();

        PostSystem($"You buy {FormatItemQuantity(item, quantity)} for {totalCost} gold.");

        return true;
    }

    public bool TrySellItemFromSlot(int slotIndex, int quantity = 1)
    {
        InventorySlotData slot = GetSlot(slotIndex);
        if (slot == null || slot.IsEmpty || slot.Item == null)
        {
            return false;
        }

        if (quantity <= 0)
        {
            return false;
        }

        int quantityToSell = Mathf.Min(quantity, slot.Quantity);
        ItemData item = slot.Item;

        if (item.SellValue <= 0)
        {
            PostSystem($"{item.DisplayName} cannot be sold.");
            return false;
        }

        RemoveFromSlotInternal(slotIndex, quantityToSell, false);

        int totalGold = item.SellValue * quantityToSell;
        AddGold(totalGold, false);

        OnInventoryChanged?.Invoke();

        PostSystem($"You sell {FormatItemQuantity(item, quantityToSell)} for {totalGold} gold.");

        return true;
    }

    public bool TryUseItemFromSlot(int slotIndex)
    {
        InventorySlotData slot = GetSlot(slotIndex);
        if (slot == null || slot.IsEmpty || slot.Item == null)
        {
            return false;
        }

        ItemData item = slot.Item;
        if (!item.IsUsable)
        {
            PostSystem($"{item.DisplayName} cannot be used.");
            return false;
        }

        bool used = ApplyItemUse(item);
        if (!used)
        {
            return false;
        }

        RemoveFromSlotInternal(slotIndex, 1, false);
        OnInventoryChanged?.Invoke();

        PostSystem($"You use {item.DisplayName}.");

        return true;
    }

    public bool TryUseFirstMatchingItem(ItemData item)
    {
        if (item == null)
        {
            return false;
        }

        for (int i = 0; i < slots.Count; i++)
        {
            InventorySlotData slot = slots[i];
            if (slot == null || slot.IsEmpty || slot.Item != item)
            {
                continue;
            }

            return TryUseItemFromSlot(i);
        }

        PostSystem($"You do not have any {item.DisplayName}.");
        return false;
    }

    public bool TryMoveOrSwapSlot(int sourceIndex, int targetIndex)
    {
        if (sourceIndex < 0 || sourceIndex >= slots.Count || targetIndex < 0 || targetIndex >= slots.Count)
        {
            return false;
        }

        if (sourceIndex == targetIndex)
        {
            return false;
        }

        InventorySlotData sourceSlot = slots[sourceIndex];
        InventorySlotData targetSlot = slots[targetIndex];

        if (sourceSlot == null || sourceSlot.IsEmpty || sourceSlot.Item == null)
        {
            return false;
        }

        bool changed = false;

        if (targetSlot.IsEmpty)
        {
            targetSlot.Set(sourceSlot.Item, sourceSlot.Quantity);
            sourceSlot.Clear();
            changed = true;
        }
        else if (sourceSlot.Item == targetSlot.Item && sourceSlot.Item.IsStackable)
        {
            int maxStack = sourceSlot.Item.MaxStack;
            int freeSpace = maxStack - targetSlot.Quantity;

            if (freeSpace > 0)
            {
                int amountToMove = Mathf.Min(freeSpace, sourceSlot.Quantity);
                targetSlot.Quantity += amountToMove;
                sourceSlot.Quantity -= amountToMove;

                if (sourceSlot.Quantity <= 0)
                {
                    sourceSlot.Clear();
                }

                changed = amountToMove > 0;
            }
        }
        else
        {
            ItemData sourceItem = sourceSlot.Item;
            int sourceQuantity = sourceSlot.Quantity;

            sourceSlot.Set(targetSlot.Item, targetSlot.Quantity);
            targetSlot.Set(sourceItem, sourceQuantity);

            changed = true;
        }

        if (changed)
        {
            OnInventoryChanged?.Invoke();
        }

        return changed;
    }

    public bool TryEquipFromSlot(int slotIndex)
    {
        if (playerEquipment == null)
        {
            Debug.LogWarning("PlayerInventory: PlayerEquipment component not found.");
            return false;
        }

        InventorySlotData slot = GetSlot(slotIndex);
        if (slot == null || slot.IsEmpty || slot.Item == null)
        {
            return false;
        }

        ItemData item = slot.Item;

        if (!item.IsEquippable || item.EquipmentSlot == EquipmentSlotType.None)
        {
            PostSystem($"{item.DisplayName} cannot be equipped.");
            return false;
        }

        ItemData currentlyEquipped = playerEquipment.GetEquippedItem(item.EquipmentSlot);

        if (currentlyEquipped != null && !CanAddItem(currentlyEquipped, 1))
        {
            PostSystem("Inventory is full.");
            return false;
        }

        RemoveFromSlotInternal(slotIndex, 1, false);

        bool equipped = playerEquipment.Equip(item, out ItemData replacedItem);

        if (!equipped)
        {
            AddItemInternal(item, 1, false);
            OnInventoryChanged?.Invoke();
            return false;
        }

        if (replacedItem != null)
        {
            AddItemInternal(replacedItem, 1, false);
        }

        OnInventoryChanged?.Invoke();

        PostSystem($"You equip {item.DisplayName}.");

        return true;
    }

    public bool TryEquipFromSlotToEquipmentSlot(int slotIndex, EquipmentSlotType targetSlotType)
    {
        if (playerEquipment == null)
        {
            Debug.LogWarning("PlayerInventory: PlayerEquipment component not found.");
            return false;
        }

        InventorySlotData sourceSlot = GetSlot(slotIndex);
        if (sourceSlot == null || sourceSlot.IsEmpty || sourceSlot.Item == null)
        {
            return false;
        }

        ItemData item = sourceSlot.Item;

        if (!item.IsEquippable || item.EquipmentSlot != targetSlotType)
        {
            PostSystem($"{item.DisplayName} cannot be equipped there.");
            return false;
        }

        ItemData currentlyEquipped = playerEquipment.GetEquippedItem(targetSlotType);

        bool sourceSlotWillBeEmptyAfterRemove = sourceSlot.Quantity <= 1;

        if (currentlyEquipped != null && !sourceSlotWillBeEmptyAfterRemove && !CanAddItem(currentlyEquipped, 1))
        {
            PostSystem("Inventory is full.");
            return false;
        }

        RemoveFromSlotInternal(slotIndex, 1, false);

        bool equipped = playerEquipment.Equip(item, out ItemData replacedItem);

        if (!equipped)
        {
            AddItemInternal(item, 1, false);
            OnInventoryChanged?.Invoke();
            return false;
        }

        if (replacedItem != null)
        {
            InventorySlotData updatedSourceSlot = GetSlot(slotIndex);

            if (updatedSourceSlot != null && updatedSourceSlot.IsEmpty)
            {
                updatedSourceSlot.Set(replacedItem, 1);
            }
            else
            {
                AddItemInternal(replacedItem, 1, false);
            }
        }

        OnInventoryChanged?.Invoke();

        PostSystem($"You equip {item.DisplayName}.");

        return true;
    }

    public bool TryMoveEquippedItemToInventorySlot(EquipmentSlotType equipmentSlotType, int targetSlotIndex)
    {
        if (playerEquipment == null)
        {
            Debug.LogWarning("PlayerInventory: PlayerEquipment component not found.");
            return false;
        }

        InventorySlotData targetSlot = GetSlot(targetSlotIndex);
        if (targetSlot == null || !targetSlot.IsEmpty)
        {
            return false;
        }

        ItemData equippedItem = playerEquipment.GetEquippedItem(equipmentSlotType);
        if (equippedItem == null)
        {
            return false;
        }

        bool unequipped = playerEquipment.Unequip(equipmentSlotType, out ItemData removedItem);
        if (!unequipped || removedItem == null)
        {
            return false;
        }

        targetSlot.Set(removedItem, 1);
        OnInventoryChanged?.Invoke();

        PostSystem($"You unequip {removedItem.DisplayName}.");

        return true;
    }

    public bool TryUnequip(EquipmentSlotType slotType)
    {
        if (playerEquipment == null)
        {
            Debug.LogWarning("PlayerInventory: PlayerEquipment component not found.");
            return false;
        }

        ItemData equippedItem = playerEquipment.GetEquippedItem(slotType);
        if (equippedItem == null)
        {
            return false;
        }

        if (!CanAddItem(equippedItem, 1))
        {
            PostSystem("Inventory is full.");
            return false;
        }

        bool unequipped = playerEquipment.Unequip(slotType, out ItemData removedItem);
        if (!unequipped || removedItem == null)
        {
            return false;
        }

        AddItemInternal(removedItem, 1, false);
        OnInventoryChanged?.Invoke();

        PostSystem($"You unequip {removedItem.DisplayName}.");

        return true;
    }

    private bool ApplyItemUse(ItemData item)
    {
        if (item == null || !item.IsUsable)
        {
            return false;
        }

        bool usedSomething = false;

        if (item.HealthRestoreAmount > 0)
        {
            if (playerHealth == null)
            {
                return false;
            }

            if (playerHealth.IsDead)
            {
                PostSystem("You cannot use that while dead.");
                return false;
            }

            if (playerHealth.CurrentHealth >= playerHealth.MaxHealth)
            {
                PostSystem("You are already at full health.");
                return false;
            }

            int restored = playerHealth.RestoreHealth(item.HealthRestoreAmount);
            if (restored > 0)
            {
                usedSomething = true;
                PostSystem($"{item.DisplayName} restores {restored} health.");
            }
        }

        return usedSomething;
    }

    private bool AddItemInternal(ItemData item, int quantity, bool notify)
    {
        if (item == null || quantity <= 0)
        {
            return false;
        }

        if (!CanAddItem(item, quantity))
        {
            if (notify)
            {
                PostSystem("Inventory is full.");
            }

            return false;
        }

        int remaining = quantity;

        if (item.IsStackable)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (!slots[i].CanStack(item))
                {
                    continue;
                }

                int freeSpace = item.MaxStack - slots[i].Quantity;
                if (freeSpace <= 0)
                {
                    continue;
                }

                int toAdd = Mathf.Min(freeSpace, remaining);
                slots[i].Quantity += toAdd;
                remaining -= toAdd;

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

            if (!slots[i].IsEmpty)
            {
                continue;
            }

            int stackSize = item.IsStackable ? Mathf.Min(item.MaxStack, remaining) : 1;
            slots[i].Set(item, stackSize);
            remaining -= stackSize;
        }

        if (notify)
        {
            OnInventoryChanged?.Invoke();
            PostSystem($"You receive {FormatItemQuantity(item, quantity)}.");
        }

        return true;
    }

    private void RemoveFromSlotInternal(int slotIndex, int quantity, bool notify)
    {
        InventorySlotData slot = GetSlot(slotIndex);
        if (slot == null || slot.IsEmpty || quantity <= 0)
        {
            return;
        }

        slot.Quantity -= quantity;

        if (slot.Quantity <= 0)
        {
            slot.Clear();
        }

        if (notify)
        {
            OnInventoryChanged?.Invoke();
        }
    }

    private string FormatItemQuantity(ItemData item, int quantity)
    {
        if (item == null)
        {
            return "item";
        }

        if (quantity <= 1)
        {
            return item.DisplayName;
        }

        return $"{item.DisplayName} x{quantity}";
    }

    private void PostSystem(string message)
    {
        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem(message);
        }
    }

        public bool CanRemoveItem(ItemData item, int quantity)
    {
        if (item == null || quantity <= 0)
        {
            return false;
        }

        return GetTotalQuantityOfItem(item) >= quantity;
    }

    public bool RemoveItem(ItemData item, int quantity, bool notify = true)
    {
        if (item == null || quantity <= 0)
        {
            return false;
        }

        if (!CanRemoveItem(item, quantity))
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

        if (notify)
        {
            OnInventoryChanged?.Invoke();
        }
        else
        {
            OnInventoryChanged?.Invoke();
        }

        return true;
    }
}