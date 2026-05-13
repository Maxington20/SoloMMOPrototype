using System;
using System.Collections.Generic;
using UnityEngine;

public class WorldInteractable : MonoBehaviour, IInteractable, ILootContainer
{
    [Header("Data")]
    [SerializeField] private WorldInteractableData interactableData;

    [Header("Interaction")]
    [SerializeField] private float interactionRange = 4f;

    [Header("Loot Slots")]
    [SerializeField] private int defaultLootSlotCount = 6;

    [Header("Fallback")]
    [SerializeField] private string fallbackDisplayName = "Interactable";

    [TextArea(2, 5)]
    [SerializeField] private string fallbackMessage = "You interact with the object.";

    private readonly List<InventorySlotData> lootSlots = new List<InventorySlotData>();

    private bool hasBeenUsed;
    private bool lootGenerated;
    private int goldAmount;
    private DisplayName displayName;

    public event Action OnLootChanged;

    public string InteractionName => LootDisplayName;
    public string LootDisplayName => GetDisplayName();
    public IReadOnlyList<InventorySlotData> LootSlots => lootSlots;
    public int GoldAmount => goldAmount;
    public int SlotCount => lootSlots.Count;

    public bool CanBeLooted => lootGenerated && HasAnyLoot();

    private void Awake()
    {
        displayName = GetComponent<DisplayName>();
        BuildEmptyLootSlots();
        ApplyDisplayName();
    }

    private void OnValidate()
    {
        displayName = GetComponent<DisplayName>();
        ApplyDisplayName();
    }

    public bool CanInteract(Transform interactor)
    {
        if (interactor == null)
        {
            return false;
        }

        if (hasBeenUsed && ShouldOnlyBeUsedOnce() && !HasAnyLoot())
        {
            return false;
        }

        float distance = Vector3.Distance(transform.position, interactor.position);
        return distance <= interactionRange;
    }

    public void Interact(Transform interactor)
    {
        if (interactor == null)
        {
            return;
        }

        if (!CanInteract(interactor))
        {
            PostSystem($"{GetDisplayName()} is too far away or has already been used.");
            return;
        }

        if (!lootGenerated)
        {
            GenerateLootFromData();
        }

        if (HasAnyLoot())
        {
            if (LootWindowUI.Instance != null)
            {
                LootWindowUI.Instance.OpenLoot(this);
            }
            else
            {
                PostSystem("Loot window UI is missing.");
            }

            return;
        }

        PostSystem(GetInteractionMessage());
        hasBeenUsed = true;

        if (ShouldHideAfterUse())
        {
            gameObject.SetActive(false);
        }
    }

    public bool TryLootGold(PlayerInventory playerInventory)
    {
        if (playerInventory == null || goldAmount <= 0)
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
        if (playerInventory == null)
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
            PostSystem("Inventory is full.");
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

    public void ResetInteractable()
    {
        hasBeenUsed = false;
        lootGenerated = false;
        goldAmount = 0;

        BuildEmptyLootSlots();
        gameObject.SetActive(true);

        OnLootChanged?.Invoke();
    }

    private void GenerateLootFromData()
    {
        BuildEmptyLootSlots();
        goldAmount = 0;

        PostSystem(GetInteractionMessage());

        if (interactableData == null)
        {
            lootGenerated = true;
            RefreshLootState();
            return;
        }

        goldAmount = interactableData.GoldReward;

        if (interactableData.ItemRewards != null)
        {
            for (int i = 0; i < interactableData.ItemRewards.Count; i++)
            {
                WorldInteractableItemReward reward = interactableData.ItemRewards[i];

                if (reward == null || reward.Item == null)
                {
                    continue;
                }

                AddItemToLoot(reward.Item, reward.Quantity);
            }
        }

        lootGenerated = true;
        RefreshLootState();
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

            int amountToAdd = item.IsStackable
                ? Mathf.Min(item.MaxStack, remaining)
                : 1;

            slot.Set(item, amountToAdd);
            remaining -= amountToAdd;
        }

        if (remaining > 0)
        {
            Debug.LogWarning($"WorldInteractable on {name} ran out of loot slots while adding {item.DisplayName}.");
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

        int rewardCount = interactableData != null && interactableData.ItemRewards != null
            ? interactableData.ItemRewards.Count
            : 0;

        int finalCount = Mathf.Max(1, defaultLootSlotCount, rewardCount);

        for (int i = 0; i < finalCount; i++)
        {
            lootSlots.Add(new InventorySlotData());
        }
    }

    private void RefreshLootState()
    {
        if (!HasAnyLoot())
        {
            hasBeenUsed = true;

            if (ShouldHideAfterUse())
            {
                gameObject.SetActive(false);
            }
        }

        OnLootChanged?.Invoke();
    }

    private void ApplyDisplayName()
    {
        if (displayName == null)
        {
            return;
        }

        displayName.SetDisplayName(GetDisplayName());
    }

    private string GetDisplayName()
    {
        if (interactableData != null && !string.IsNullOrWhiteSpace(interactableData.DisplayName))
        {
            return interactableData.DisplayName;
        }

        return fallbackDisplayName;
    }

    private string GetInteractionMessage()
    {
        if (interactableData != null && !string.IsNullOrWhiteSpace(interactableData.InteractionMessage))
        {
            return interactableData.InteractionMessage;
        }

        return fallbackMessage;
    }

    private bool ShouldOnlyBeUsedOnce()
    {
        return interactableData == null || interactableData.CanOnlyBeUsedOnce;
    }

    private bool ShouldHideAfterUse()
    {
        return interactableData != null && interactableData.HideAfterUse;
    }

    private void PostSystem(string message)
    {
        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem(message);
        }
    }
}