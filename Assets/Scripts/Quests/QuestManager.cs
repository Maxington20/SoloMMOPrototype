using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("Quest List")]
    [SerializeField] private List<QuestDefinition> quests = new List<QuestDefinition>();

    [Header("Settings")]
    [SerializeField] private int maxActiveQuests = 10;

    private readonly List<ActiveQuest> activeQuests = new List<ActiveQuest>();
    private readonly List<QuestDefinition> completedQuests = new List<QuestDefinition>();

    private ActiveQuest pendingTurnInQuest;

    public IReadOnlyList<ActiveQuest> ActiveQuests => activeQuests;
    public IReadOnlyList<QuestDefinition> AllQuests => quests;

    public bool HasActiveQuest => activeQuests.Count > 0;
    public bool HasCompletableQuest => GetFirstCompletableQuest() != null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public string GetQuestOfferText()
    {
        ActiveQuest completableQuest = GetFirstCompletableQuest();
        if (completableQuest != null)
        {
            return $"Press F to speak with the quest giver.";
        }

        QuestDefinition availableQuest = GetFirstAvailableQuest();
        if (availableQuest != null)
        {
            return $"Press F to speak with the quest giver.";
        }

        if (activeQuests.Count > 0)
        {
            return $"Press F to review your quests.";
        }

        return "No quests available.";
    }

    public void InteractWithQuestGiver()
    {
        if (QuestGiverUI.Instance != null)
        {
            QuestGiverUI.Instance.Open();
            return;
        }

        PostSystem("Quest giver UI is missing.");
    }

    public List<QuestDefinition> GetAvailableQuests()
    {
        List<QuestDefinition> available = new List<QuestDefinition>();

        if (quests == null)
        {
            return available;
        }

        for (int i = 0; i < quests.Count; i++)
        {
            QuestDefinition quest = quests[i];

            if (quest == null)
            {
                continue;
            }

            if (IsQuestAlreadyActive(quest))
            {
                continue;
            }

            if (IsQuestCompleted(quest))
            {
                continue;
            }

            available.Add(quest);
        }

        return available;
    }

    public List<ActiveQuest> GetCompletableQuests()
    {
        List<ActiveQuest> completable = new List<ActiveQuest>();
        PlayerInventory inventory = GetPlayerInventory();

        for (int i = 0; i < activeQuests.Count; i++)
        {
            ActiveQuest quest = activeQuests[i];

            if (quest != null && quest.IsComplete(inventory))
            {
                completable.Add(quest);
            }
        }

        return completable;
    }

    public List<ActiveQuest> GetInProgressQuests()
    {
        List<ActiveQuest> inProgress = new List<ActiveQuest>();
        PlayerInventory inventory = GetPlayerInventory();

        for (int i = 0; i < activeQuests.Count; i++)
        {
            ActiveQuest quest = activeQuests[i];

            if (quest != null && !quest.IsComplete(inventory))
            {
                inProgress.Add(quest);
            }
        }

        return inProgress;
    }

    public void AcceptQuest(QuestDefinition definition)
    {
        if (definition == null)
        {
            return;
        }

        if (activeQuests.Count >= maxActiveQuests)
        {
            PostSystem("Your quest log is full.");
            return;
        }

        if (IsQuestAlreadyActive(definition))
        {
            PostSystem($"You already have {definition.title}.");
            return;
        }

        if (IsQuestCompleted(definition))
        {
            PostSystem($"You have already completed {definition.title}.");
            return;
        }

        activeQuests.Add(new ActiveQuest(definition));
        PostSystem($"Quest accepted: {definition.title}.");
    }

    public void TurnInQuest(ActiveQuest quest)
    {
        if (quest == null || !activeQuests.Contains(quest))
        {
            return;
        }

        PlayerInventory inventory = GetPlayerInventory();

        if (!quest.IsComplete(inventory))
        {
            PostSystem($"You have not completed {quest.definition.title} yet.");
            return;
        }

        if (quest.definition.HasChoiceRewards)
        {
            if (QuestRewardChoiceUI.Instance == null)
            {
                PostSystem("Reward choice UI is missing.");
                return;
            }

            pendingTurnInQuest = quest;
            QuestRewardChoiceUI.Instance.Open(quest.definition, CompletePendingQuestTurnIn);
            return;
        }

        CompleteQuestTurnIn(quest, -1);
    }

    public void CompletePendingQuestTurnIn(int selectedChoiceRewardIndex)
    {
        if (pendingTurnInQuest == null)
        {
            return;
        }

        ActiveQuest questToComplete = pendingTurnInQuest;
        pendingTurnInQuest = null;

        CompleteQuestTurnIn(questToComplete, selectedChoiceRewardIndex);
    }

    public void CompleteQuestTurnIn(ActiveQuest quest, int selectedChoiceRewardIndex)
    {
        if (quest == null || !activeQuests.Contains(quest))
        {
            return;
        }

        QuestDefinition completedQuest = quest.definition;
        PlayerInventory inventory = GetPlayerInventory();
        PlayerProgression progression = GetPlayerProgression();

        if (completedQuest == null || !quest.IsComplete(inventory))
        {
            return;
        }

        List<QuestItemReward> finalItemRewards = BuildFinalItemRewards(completedQuest, selectedChoiceRewardIndex);

        if (!CanFitRewardsAfterTurnIn(inventory, completedQuest.collectionObjectives, finalItemRewards))
        {
            PostSystem("Inventory is full. Make space before turning in this quest.");
            return;
        }

        ConsumeCollectionItems(inventory, completedQuest.collectionObjectives);

        if (progression != null && completedQuest.xpReward > 0)
        {
            progression.AddXp(completedQuest.xpReward);
        }

        if (inventory != null && completedQuest.goldReward > 0)
        {
            inventory.AddGold(completedQuest.goldReward, false);
            PostSystem($"You receive {completedQuest.goldReward} gold.");
        }

        if (inventory != null)
        {
            foreach (QuestItemReward reward in finalItemRewards)
            {
                if (reward != null && reward.Item != null)
                {
                    inventory.AddItem(reward.Item, reward.Quantity);
                }
            }
        }

        activeQuests.Remove(quest);

        if (!completedQuests.Contains(completedQuest))
        {
            completedQuests.Add(completedQuest);
        }

        PostSystem($"Quest completed: {completedQuest.title}.");

        if (QuestGiverUI.Instance != null && QuestGiverUI.Instance.IsOpen)
        {
            QuestGiverUI.Instance.Refresh();
        }
    }

    public void RegisterEnemyKilled(GameObject enemyObject, GameObject killer)
    {
        if (enemyObject == null || killer == null || !killer.CompareTag("Player"))
        {
            return;
        }

        EnemyData enemyData = enemyObject.GetComponent<EnemyData>();
        if (enemyData == null)
        {
            return;
        }

        bool progressedAnyQuest = false;
        PlayerInventory inventory = GetPlayerInventory();

        foreach (ActiveQuest quest in activeQuests)
        {
            if (quest == null || quest.definition == null)
            {
                continue;
            }

            if (quest.IsComplete(inventory))
            {
                continue;
            }

            if (!QuestNeedsEnemyType(quest, enemyData.EnemyType))
            {
                continue;
            }

            quest.RegisterKill(enemyData.EnemyType);
            progressedAnyQuest = true;

            if (quest.IsComplete(inventory))
            {
                PostSystem($"Return to the quest giver to turn in {quest.definition.title}.");
            }
        }

        if (progressedAnyQuest)
        {
            PostSystem("Quest progress updated.");
        }
    }

    private QuestDefinition GetFirstAvailableQuest()
    {
        List<QuestDefinition> available = GetAvailableQuests();
        return available.Count > 0 ? available[0] : null;
    }

    private ActiveQuest GetFirstCompletableQuest()
    {
        PlayerInventory inventory = GetPlayerInventory();

        foreach (ActiveQuest quest in activeQuests)
        {
            if (quest != null && quest.IsComplete(inventory))
            {
                return quest;
            }
        }

        return null;
    }

    private bool IsQuestAlreadyActive(QuestDefinition definition)
    {
        foreach (ActiveQuest quest in activeQuests)
        {
            if (quest != null && quest.definition == definition)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsQuestCompleted(QuestDefinition definition)
    {
        return completedQuests.Contains(definition);
    }

    private bool QuestNeedsEnemyType(ActiveQuest quest, EnemyType enemyType)
    {
        if (quest?.definition?.killObjectives == null)
        {
            return false;
        }

        foreach (QuestKillObjective objective in quest.definition.killObjectives)
        {
            if (objective != null && objective.EnemyType == enemyType)
            {
                return true;
            }
        }

        return false;
    }

    private List<QuestItemReward> BuildFinalItemRewards(QuestDefinition definition, int selectedChoiceRewardIndex)
    {
        List<QuestItemReward> rewards = new List<QuestItemReward>();

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

    private void ConsumeCollectionItems(PlayerInventory inventory, List<QuestCollectionObjective> objectives)
    {
        if (inventory == null || objectives == null)
        {
            return;
        }

        foreach (QuestCollectionObjective objective in objectives)
        {
            if (objective != null && objective.Item != null)
            {
                inventory.RemoveItem(objective.Item, objective.RequiredAmount, false);
            }
        }
    }

    private bool CanFitRewardsAfterTurnIn(
        PlayerInventory inventory,
        List<QuestCollectionObjective> consumedItems,
        List<QuestItemReward> rewards)
    {
        if (inventory == null)
        {
            return rewards == null || rewards.Count == 0;
        }

        List<SimulatedSlot> simulatedSlots = new List<SimulatedSlot>();

        foreach (InventorySlotData slot in inventory.Slots)
        {
            simulatedSlots.Add(new SimulatedSlot
            {
                item = slot != null && !slot.IsEmpty ? slot.Item : null,
                quantity = slot != null && !slot.IsEmpty ? slot.Quantity : 0
            });
        }

        if (consumedItems != null)
        {
            foreach (QuestCollectionObjective objective in consumedItems)
            {
                if (objective != null && objective.Item != null)
                {
                    if (!TryRemoveFromSimulatedSlots(simulatedSlots, objective.Item, objective.RequiredAmount))
                    {
                        return false;
                    }
                }
            }
        }

        if (rewards != null)
        {
            foreach (QuestItemReward reward in rewards)
            {
                if (reward != null && reward.Item != null)
                {
                    if (!TryAddToSimulatedSlots(simulatedSlots, reward.Item, reward.Quantity))
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    private bool TryRemoveFromSimulatedSlots(List<SimulatedSlot> slots, ItemData item, int quantity)
    {
        int remaining = quantity;

        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].item != item)
            {
                continue;
            }

            int removed = Mathf.Min(slots[i].quantity, remaining);
            SimulatedSlot updatedSlot = slots[i];
            updatedSlot.quantity -= removed;

            if (updatedSlot.quantity <= 0)
            {
                updatedSlot.item = null;
                updatedSlot.quantity = 0;
            }

            slots[i] = updatedSlot;
            remaining -= removed;

            if (remaining <= 0)
            {
                return true;
            }
        }

        return false;
    }

    private bool TryAddToSimulatedSlots(List<SimulatedSlot> slots, ItemData item, int quantity)
    {
        int remaining = quantity;

        if (item.IsStackable)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                SimulatedSlot slot = slots[i];

                if (slot.item != item)
                {
                    continue;
                }

                int freeSpace = item.MaxStack - slot.quantity;
                if (freeSpace <= 0)
                {
                    continue;
                }

                int added = Mathf.Min(freeSpace, remaining);
                slot.quantity += added;
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
            SimulatedSlot slot = slots[i];

            if (slot.item != null)
            {
                continue;
            }

            int added = item.IsStackable ? Mathf.Min(item.MaxStack, remaining) : 1;
            slot.item = item;
            slot.quantity = added;
            slots[i] = slot;

            remaining -= added;

            if (remaining <= 0)
            {
                return true;
            }
        }

        return false;
    }

    private PlayerInventory GetPlayerInventory()
    {
        if (PlayerInventory.Instance != null)
        {
            return PlayerInventory.Instance;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        return playerObject != null ? playerObject.GetComponent<PlayerInventory>() : null;
    }

    private PlayerProgression GetPlayerProgression()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        return playerObject != null ? playerObject.GetComponent<PlayerProgression>() : null;
    }

    private void PostSystem(string message)
    {
        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem(message);
        }
    }

    private struct SimulatedSlot
    {
        public ItemData item;
        public int quantity;
    }
}