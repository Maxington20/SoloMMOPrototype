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

        if (postLootMessage && ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem($"You loot {amount} gold.");
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
            if (ChatManager.Instance != null)
            {
                ChatManager.Instance.PostSystem($"{item.DisplayName} cannot be purchased.");
            }

            return false;
        }

        if (!CanAddItem(item, quantity))
        {
            if (ChatManager.Instance != null)
            {
                ChatManager.Instance.PostSystem("Inventory is full.");
            }

            return false;
        }

        if (!SpendGold(totalCost))
        {
            if (ChatManager.Instance != null)
            {
                ChatManager.Instance.PostSystem("You do not have enough gold.");
            }

            return false;
        }

        AddItemInternal(item, quantity, false);
        OnInventoryChanged?.Invoke();

        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem($"You buy {item.DisplayName} for {totalCost} gold.");
        }

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
            if (ChatManager.Instance != null)
            {
                ChatManager.Instance.PostSystem($"{item.DisplayName} cannot be sold.");
            }

            return false;
        }

        RemoveFromSlotInternal(slotIndex, quantityToSell, false);

        int totalGold = item.SellValue * quantityToSell;
        AddGold(totalGold, false);

        OnInventoryChanged?.Invoke();

        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem($"You sell {item.DisplayName} for {totalGold} gold.");
        }

        return true;
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
            return false;
        }

        ItemData currentlyEquipped = playerEquipment.GetEquippedItem(item.EquipmentSlot);

        if (currentlyEquipped != null && !CanAddItem(currentlyEquipped, 1))
        {
            if (ChatManager.Instance != null)
            {
                ChatManager.Instance.PostSystem("Inventory is full.");
            }

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

        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem($"You equip {item.DisplayName}.");
        }

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
            if (ChatManager.Instance != null)
            {
                ChatManager.Instance.PostSystem("Inventory is full.");
            }

            return false;
        }

        bool unequipped = playerEquipment.Unequip(slotType, out ItemData removedItem);
        if (!unequipped || removedItem == null)
        {
            return false;
        }

        AddItemInternal(removedItem, 1, false);
        OnInventoryChanged?.Invoke();

        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem($"You unequip {removedItem.DisplayName}.");
        }

        return true;
    }

    private bool AddItemInternal(ItemData item, int quantity, bool notify)
    {
        if (item == null || quantity <= 0)
        {
            return false;
        }

        if (!CanAddItem(item, quantity))
        {
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

            if (ChatManager.Instance != null)
            {
                ChatManager.Instance.PostSystem($"You receive {item.DisplayName}.");
            }
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
}