using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyLoot : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyData enemyData;
    [SerializeField] private Collider lootClickCollider;

    [Header("Loot Indicator")]
    [SerializeField] private GameObject lootIndicatorPrefab;
    [SerializeField] private Vector3 lootIndicatorLocalPosition = new Vector3(0f, 2.4f, 0f);

    [Header("Settings")]
    [SerializeField] private int defaultLootSlotCount = 6;

    private readonly List<InventorySlotData> lootSlots = new List<InventorySlotData>();
    private readonly HashSet<ItemData> uniqueItemsAlreadyDropped = new HashSet<ItemData>();

    private Health health;
    private GameObject spawnedLootIndicator;
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
        CreateLootIndicatorIfNeeded();
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
        uniqueItemsAlreadyDropped.Clear();

        RollGold();
        RollItems();

        lootGenerated = true;
        RefreshLootState();
    }

    public void ResetLoot()
    {
        goldAmount = 0;
        lootGenerated = false;
        uniqueItemsAlreadyDropped.Clear();

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

    private void RollGold()
    {
        goldAmount = 0;

        if (UnityEngine.Random.value > enemyData.GoldDropChance)
        {
            return;
        }

        if (enemyData.MaxGold <= 0)
        {
            return;
        }

        goldAmount = UnityEngine.Random.Range(enemyData.MinGold, enemyData.MaxGold + 1);
    }

    private void RollItems()
    {
        EnemyLootTableEntry[] lootTable = enemyData.LootTable;
        if (lootTable == null || lootTable.Length == 0)
        {
            return;
        }

        for (int roll = 0; roll < enemyData.ItemDropRolls; roll++)
        {
            TryRollOneItem(lootTable);
        }
    }

    private void TryRollOneItem(EnemyLootTableEntry[] lootTable)
    {
        List<EnemyLootTableEntry> successfulDrops = new List<EnemyLootTableEntry>();

        for (int i = 0; i < lootTable.Length; i++)
        {
            EnemyLootTableEntry entry = lootTable[i];

            if (entry == null || entry.Item == null)
            {
                continue;
            }

            if (entry.UniquePerCorpse && uniqueItemsAlreadyDropped.Contains(entry.Item))
            {
                continue;
            }

            if (UnityEngine.Random.value <= entry.DropChance)
            {
                successfulDrops.Add(entry);
            }
        }

        if (successfulDrops.Count == 0)
        {
            return;
        }

        EnemyLootTableEntry selectedEntry = successfulDrops[UnityEngine.Random.Range(0, successfulDrops.Count)];
        int quantity = UnityEngine.Random.Range(selectedEntry.MinQuantity, selectedEntry.MaxQuantity + 1);

        bool added = AddItemToLoot(selectedEntry.Item, quantity);
        if (added && selectedEntry.UniquePerCorpse)
        {
            uniqueItemsAlreadyDropped.Add(selectedEntry.Item);
        }
    }

    private bool AddItemToLoot(ItemData item, int quantity)
    {
        if (item == null || quantity <= 0)
        {
            return false;
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
                    return true;
                }
            }
        }

        for (int i = 0; i < lootSlots.Count; i++)
        {
            if (remaining <= 0)
            {
                return true;
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

        return remaining < quantity;
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

    private void CreateLootIndicatorIfNeeded()
    {
        if (lootIndicatorPrefab == null || spawnedLootIndicator != null)
        {
            return;
        }

        spawnedLootIndicator = Instantiate(lootIndicatorPrefab, transform);
        spawnedLootIndicator.name = "LootIndicator";
        spawnedLootIndicator.transform.localPosition = lootIndicatorLocalPosition;
        spawnedLootIndicator.transform.localRotation = Quaternion.identity;
        spawnedLootIndicator.transform.localScale = Vector3.one;
        spawnedLootIndicator.SetActive(false);
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

        if (spawnedLootIndicator != null)
        {
            spawnedLootIndicator.SetActive(isInteractable);
        }
    }
}