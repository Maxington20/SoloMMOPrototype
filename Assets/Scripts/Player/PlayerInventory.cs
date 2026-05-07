using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerWallet))]
[RequireComponent(typeof(PlayerItemUseController))]
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
    private PlayerWallet playerWallet;
    private PlayerItemUseController itemUseController;

    public int Gold => playerWallet != null ? playerWallet.Gold : 0;
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
        playerWallet = GetComponent<PlayerWallet>();
        itemUseController = GetComponent<PlayerItemUseController>();

        if (playerWallet == null)
        {
            playerWallet = gameObject.AddComponent<PlayerWallet>();
        }

        if (itemUseController == null)
        {
            itemUseController = gameObject.AddComponent<PlayerItemUseController>();
        }

        playerWallet.OnGoldChanged += HandleWalletGoldChanged;
        playerWallet.Initialize(startingGold);

        InitializeSlots();
    }

    private void Start()
    {
        NotifyGoldChanged();
        NotifyInventoryChanged();
    }

    private void OnDestroy()
    {
        if (playerWallet != null)
        {
            playerWallet.OnGoldChanged -= HandleWalletGoldChanged;
        }

        if (Instance == this)
        {
            Instance = null;
        }
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
        if (amount <= 0 || playerWallet == null)
        {
            return;
        }

        playerWallet.AddGold(amount);

        if (postLootMessage)
        {
            PostSystem($"You loot {amount} gold.");
        }
    }

    public bool SpendGold(int amount)
    {
        if (playerWallet == null)
        {
            return false;
        }

        return playerWallet.SpendGold(amount);
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
        return InventoryTransactionService.GetTotalQuantityOfItem(slots, item);
    }

    public bool CanAddItem(ItemData item, int quantity = 1)
    {
        return InventoryTransactionService.CanAddItem(slots, item, quantity);
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
        NotifyInventoryChanged();

        PostSystem($"You buy {FormatItemQuantity(item, quantity)} for {totalCost} gold.");

        return true;
    }

    public bool TrySellItemFromSlot(int slotIndex, int quantity = 1)
    {
        InventorySlotData slot = GetSlot(slotIndex);
        if (slot == null || slot.IsEmpty || slot.Item == null || quantity <= 0)
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

        NotifyInventoryChanged();

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

        if (itemUseController == null || !itemUseController.TryUseItem(item))
        {
            return false;
        }

        RemoveFromSlotInternal(slotIndex, 1, false);
        NotifyInventoryChanged();

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
        bool changed = InventoryTransactionService.MoveOrSwap(slots, sourceIndex, targetIndex);

        if (changed)
        {
            NotifyInventoryChanged();
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
            NotifyInventoryChanged();
            return false;
        }

        if (replacedItem != null)
        {
            AddItemInternal(replacedItem, 1, false);
        }

        NotifyInventoryChanged();

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

        if (!item.IsEquippable || !playerEquipment.CanEquipItemInSlot(item, targetSlotType))
        {
            string reason = playerEquipment.GetCannotEquipReason(item, targetSlotType);
            PostSystem(string.IsNullOrWhiteSpace(reason) ? $"{item.DisplayName} cannot be equipped there." : reason);
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

        bool equipped = playerEquipment.Equip(item, targetSlotType, out ItemData replacedItem);

        if (!equipped)
        {
            AddItemInternal(item, 1, false);
            NotifyInventoryChanged();
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

        NotifyInventoryChanged();

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
        NotifyInventoryChanged();

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
        NotifyInventoryChanged();

        PostSystem($"You unequip {removedItem.DisplayName}.");

        return true;
    }

    public bool CanRemoveItem(ItemData item, int quantity)
    {
        return InventoryTransactionService.CanRemoveItem(slots, item, quantity);
    }

    public bool RemoveItem(ItemData item, int quantity, bool notify = true)
    {
        bool removed = InventoryTransactionService.RemoveItem(slots, item, quantity);

        if (removed && notify)
        {
            NotifyInventoryChanged();
        }
        else if (removed)
        {
            NotifyInventoryChanged();
        }

        return removed;
    }

    private bool AddItemInternal(ItemData item, int quantity, bool notify)
    {
        bool added = InventoryTransactionService.AddItem(slots, item, quantity);

        if (!added)
        {
            if (notify)
            {
                PostSystem("Inventory is full.");
            }

            return false;
        }

        if (notify)
        {
            NotifyInventoryChanged();
            PostSystem($"You receive {FormatItemQuantity(item, quantity)}.");
        }

        return true;
    }

    private void RemoveFromSlotInternal(int slotIndex, int quantity, bool notify)
    {
        InventorySlotData slot = GetSlot(slotIndex);
        InventoryTransactionService.RemoveFromSlot(slot, quantity);

        if (notify)
        {
            NotifyInventoryChanged();
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

    private void HandleWalletGoldChanged(int updatedGold)
    {
        OnGoldChanged?.Invoke(updatedGold);
    }

    private void NotifyGoldChanged()
    {
        OnGoldChanged?.Invoke(Gold);
    }

    private void NotifyInventoryChanged()
    {
        OnInventoryChanged?.Invoke();
    }

    private void PostSystem(string message)
    {
        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem(message);
        }
    }
}