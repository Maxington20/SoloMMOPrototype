using System.Collections.Generic;
using UnityEngine;

public static class QuestRewardService
{
    public static List<QuestItemReward> BuildFinalItemRewards(
        QuestDefinition definition,
        int selectedChoiceRewardIndex)
    {
        List<QuestItemReward> rewards = new List<QuestItemReward>();

        if (definition == null)
        {
            return rewards;
        }

        if (definition.guaranteedItemRewards != null)
        {
            rewards.AddRange(definition.guaranteedItemRewards);
        }

        if (definition.choiceItemRewards != null &&
            selectedChoiceRewardIndex >= 0 &&
            selectedChoiceRewardIndex < definition.choiceItemRewards.Count)
        {
            rewards.Add(definition.choiceItemRewards[selectedChoiceRewardIndex]);
        }

        return rewards;
    }

    public static bool CanFitRewardsAfterTurnIn(
        PlayerInventory inventory,
        List<QuestCollectionObjective> consumedItems,
        List<QuestItemReward> rewards)
    {
        if (inventory == null)
        {
            return rewards == null || rewards.Count == 0;
        }

        List<SimulatedInventorySlot> simulatedSlots = BuildSimulatedInventory(inventory);

        if (!RemoveCollectionItemsFromSimulation(simulatedSlots, consumedItems))
        {
            return false;
        }

        return AddRewardsToSimulation(simulatedSlots, rewards);
    }

    public static void ConsumeCollectionItems(
        PlayerInventory inventory,
        List<QuestCollectionObjective> objectives)
    {
        if (inventory == null || objectives == null)
        {
            return;
        }

        for (int i = 0; i < objectives.Count; i++)
        {
            QuestCollectionObjective objective = objectives[i];

            if (objective == null || objective.Item == null)
            {
                continue;
            }

            inventory.RemoveItem(objective.Item, objective.RequiredAmount, false);
        }
    }

    public static void GrantRewards(
        QuestDefinition completedQuest,
        List<QuestItemReward> finalItemRewards,
        PlayerInventory inventory,
        PlayerProgression progression)
    {
        if (completedQuest == null)
        {
            return;
        }

        if (progression != null && completedQuest.xpReward > 0)
        {
            progression.AddXp(completedQuest.xpReward);
        }

        if (inventory != null && completedQuest.goldReward > 0)
        {
            inventory.AddGold(completedQuest.goldReward, false);
            PostSystem($"You receive {completedQuest.goldReward} gold.");
        }

        if (inventory == null || finalItemRewards == null)
        {
            return;
        }

        for (int i = 0; i < finalItemRewards.Count; i++)
        {
            QuestItemReward reward = finalItemRewards[i];

            if (reward == null || reward.Item == null)
            {
                continue;
            }

            inventory.AddItem(reward.Item, reward.Quantity);
        }
    }

    private static List<SimulatedInventorySlot> BuildSimulatedInventory(PlayerInventory inventory)
    {
        List<SimulatedInventorySlot> simulatedSlots = new List<SimulatedInventorySlot>();

        foreach (InventorySlotData slot in inventory.Slots)
        {
            simulatedSlots.Add(new SimulatedInventorySlot
            {
                Item = slot != null && !slot.IsEmpty ? slot.Item : null,
                Quantity = slot != null && !slot.IsEmpty ? slot.Quantity : 0
            });
        }

        return simulatedSlots;
    }

    private static bool RemoveCollectionItemsFromSimulation(
        List<SimulatedInventorySlot> simulatedSlots,
        List<QuestCollectionObjective> consumedItems)
    {
        if (consumedItems == null)
        {
            return true;
        }

        for (int i = 0; i < consumedItems.Count; i++)
        {
            QuestCollectionObjective objective = consumedItems[i];

            if (objective == null || objective.Item == null)
            {
                continue;
            }

            if (!TryRemoveFromSimulatedSlots(
                    simulatedSlots,
                    objective.Item,
                    objective.RequiredAmount))
            {
                return false;
            }
        }

        return true;
    }

    private static bool AddRewardsToSimulation(
        List<SimulatedInventorySlot> simulatedSlots,
        List<QuestItemReward> rewards)
    {
        if (rewards == null)
        {
            return true;
        }

        for (int i = 0; i < rewards.Count; i++)
        {
            QuestItemReward reward = rewards[i];

            if (reward == null || reward.Item == null)
            {
                continue;
            }

            if (!TryAddToSimulatedSlots(
                    simulatedSlots,
                    reward.Item,
                    reward.Quantity))
            {
                return false;
            }
        }

        return true;
    }

    private static bool TryRemoveFromSimulatedSlots(
        List<SimulatedInventorySlot> slots,
        ItemData item,
        int quantity)
    {
        int remaining = quantity;

        for (int i = 0; i < slots.Count; i++)
        {
            SimulatedInventorySlot slot = slots[i];

            if (slot.Item != item)
            {
                continue;
            }

            int removed = Mathf.Min(slot.Quantity, remaining);
            slot.Quantity -= removed;

            if (slot.Quantity <= 0)
            {
                slot.Item = null;
                slot.Quantity = 0;
            }

            slots[i] = slot;
            remaining -= removed;

            if (remaining <= 0)
            {
                return true;
            }
        }

        return false;
    }

    private static bool TryAddToSimulatedSlots(
        List<SimulatedInventorySlot> slots,
        ItemData item,
        int quantity)
    {
        int remaining = quantity;

        if (item.IsStackable)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                SimulatedInventorySlot slot = slots[i];

                if (slot.Item != item)
                {
                    continue;
                }

                int freeSpace = item.MaxStack - slot.Quantity;

                if (freeSpace <= 0)
                {
                    continue;
                }

                int added = Mathf.Min(freeSpace, remaining);
                slot.Quantity += added;
                slots[i] = slot;

                remaining -= added;

                if (remaining <= 0)
                {
                    return true;
                }
            }
        }

        for (int i = 0; i < slots.Count; i++)
        {
            SimulatedInventorySlot slot = slots[i];

            if (slot.Item != null)
            {
                continue;
            }

            int added = item.IsStackable
                ? Mathf.Min(item.MaxStack, remaining)
                : 1;

            slot.Item = item;
            slot.Quantity = added;
            slots[i] = slot;

            remaining -= added;

            if (remaining <= 0)
            {
                return true;
            }
        }

        return false;
    }

    private static void PostSystem(string message)
    {
        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem(message);
        }
    }

    private struct SimulatedInventorySlot
    {
        public ItemData Item;
        public int Quantity;
    }
}