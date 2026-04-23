using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyLoot : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyData enemyData;
    [SerializeField] private Collider lootClickCollider;
    [SerializeField] private GameObject lootIndicatorObject;

    [Header("Settings")]
    [SerializeField] private int defaultLootSlotCount = 6;

    private readonly List<InventorySlotData> lootSlots = new List<InventorySlotData>();
    private Health health;
    private int goldAmount;
    private bool lootGenerated;

    public event Action OnLootChanged;

    public IReadOnlyList<InventorySlotData> LootSlots => lootSlots;
    public Collider LootClickCollider => lootClickCollider;
    public int GoldAmount => goldAmount;
    public int SlotCount => lootSlots.Count;

    public bool CanBeLooted
    {
        get
        {
            return lootGenerated && health != null && health.IsDead && HasAnyLoot();
        }
    }

    private void Awake()
    {
        if (enemyData == null)
        {
            enemyData = GetComponent<EnemyData>();
        }

        health = GetComponent<Health>();

        BuildEmptyLootSlots();
        SetLootInteractable(false);
    }

    public void GenerateLoot()
    {
        if (enemyData == null)
        {
            Debug.LogWarning($"EnemyLoot on {name} is missing EnemyData.");
            return;
        }

        BuildEmptyLootSlots();

        goldAmount = UnityEngine.Random.Range(enemyData.MinGold, enemyData.MaxGold + 1);

        EnemyLootTableEntry[] lootTable = enemyData.LootTable;
        if (lootTable != null)
        {
            for (int i = 0; i < lootTable.Length; i++)
            {
                EnemyLootTableEntry entry = lootTable[i];

                if (entry == null || entry.Item == null)
                {
                    continue;
                }

                if (UnityEngine.Random.value > entry.DropChance)
                {
                    continue;
                }

                int quantity = UnityEngine.Random.Range(entry.MinQuantity, entry.MaxQuantity + 1);
                if (quantity <= 0)
                {
                    continue;
                }

                AddItemToLoot(entry.Item, quantity);
            }
        }

        lootGenerated = true;
        RefreshLootState();
    }

    public void ResetLoot()
    {
        goldAmount = 0;
        lootGenerated = false;

        BuildEmptyLootSlots();
        RefreshLootState();
    }

    public bool TryLootGold(PlayerInventory playerInventory)
    {
        if (playerInventory == null || !CanBeLooted || goldAmount <= 0)
        {
            return false;
        }

        int amountToLoot = goldAmount;
        goldAmount = 0;

        playerInventory.AddGold(amountToLoot, true);
        RefreshLootState();
        return true;
    }

    public bool TryLootItem(int slotIndex, PlayerInventory playerInventory)
    {
        if (playerInventory == null || !CanBeLooted)
        {
            return false;
        }

        if (slotIndex < 0 || slotIndex >= lootSlots.Count)
        {
            return false;
        }

        InventorySlotData slot = lootSlots[slotIndex];
        if (slot == null || slot.IsEmpty || slot.Item == null)
        {
            return false;
        }

        if (!playerInventory.CanAddItem(slot.Item, slot.Quantity))
        {
            if (ChatManager.Instance != null)
            {
                ChatManager.Instance.PostSystem("Inventory is full.");
            }

            return false;
        }

        bool added = playerInventory.AddItem(slot.Item, slot.Quantity);
        if (!added)
        {
            return false;
        }

        slot.Clear();
        RefreshLootState();
        return true;
    }

    private bool HasAnyLoot()
    {
        if (goldAmount > 0)
        {
            return true;
        }

        for (int i = 0; i < lootSlots.Count; i++)
        {
            InventorySlotData slot = lootSlots[i];
            if (slot != null && !slot.IsEmpty && slot.Item != null)
            {
                return true;
            }
        }

        return false;
    }

    private void AddItemToLoot(ItemData item, int quantity)
    {
        if (item == null || quantity <= 0)
        {
            return;
        }

        int remaining = quantity;

        if (item.IsStackable)
        {
            for (int i = 0; i < lootSlots.Count; i++)
            {
                InventorySlotData slot = lootSlots[i];
                if (!slot.CanStack(item))
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
                    return;
                }
            }
        }

        for (int i = 0; i < lootSlots.Count; i++)
        {
            if (remaining <= 0)
            {
                return;
            }

            InventorySlotData slot = lootSlots[i];
            if (slot == null || !slot.IsEmpty)
            {
                continue;
            }

            int amountToAdd = item.IsStackable ? Mathf.Min(item.MaxStack, remaining) : 1;
            slot.Set(item, amountToAdd);
            remaining -= amountToAdd;
        }

        if (remaining > 0)
        {
            Debug.LogWarning($"EnemyLoot on {name} ran out of loot slots while adding {item.DisplayName}.");
        }
    }

    private void BuildEmptyLootSlots()
    {
        lootSlots.Clear();

        int configuredCount = enemyData != null ? enemyData.LootSlotCount : 0;
        int tableCount = enemyData != null && enemyData.LootTable != null ? enemyData.LootTable.Length : 0;
        int finalCount = Mathf.Max(1, defaultLootSlotCount, configuredCount, tableCount);

        for (int i = 0; i < finalCount; i++)
        {
            lootSlots.Add(new InventorySlotData());
        }
    }

    private void RefreshLootState()
    {
        SetLootInteractable(CanBeLooted);
        OnLootChanged?.Invoke();
    }

    private void SetLootInteractable(bool isInteractable)
    {
        if (lootClickCollider != null)
        {
            lootClickCollider.enabled = isInteractable;
        }

        if (lootIndicatorObject != null)
        {
            lootIndicatorObject.SetActive(isInteractable);
        }
    }
}