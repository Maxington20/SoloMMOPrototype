using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("Fallback Quest List")]
    [SerializeField] private List<QuestDefinition> quests = new List<QuestDefinition>();

    [Header("Settings")]
    [SerializeField] private int maxActiveQuests = 10;

    private readonly List<ActiveQuest> activeQuests = new List<ActiveQuest>();
    private readonly List<QuestDefinition> completedQuests = new List<QuestDefinition>();

    private QuestGiver currentQuestGiver;
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

    public void InteractWithQuestGiver()
    {
        currentQuestGiver = null;
        OpenQuestGiverUI();
    }

    public void InteractWithQuestGiver(QuestGiver questGiver)
    {
        currentQuestGiver = questGiver;
        OpenQuestGiverUI();
    }

    public string GetQuestOfferText()
    {
        ActiveQuest completableQuest = GetFirstCompletableQuest();

        if (completableQuest != null)
        {
            return "Click to speak with the quest giver.";
        }

        QuestDefinition availableQuest = GetFirstAvailableQuest();

        if (availableQuest != null)
        {
            return "Click to speak with the quest giver.";
        }

        if (GetInProgressQuests().Count > 0)
        {
            return "Click to review your quests.";
        }

        return "No quests available.";
    }

    public List<QuestDefinition> GetAvailableQuests()
    {
        List<QuestDefinition> available = new List<QuestDefinition>();
        IReadOnlyList<QuestDefinition> sourceQuests = GetCurrentQuestSource();

        if (sourceQuests == null)
        {
            return available;
        }

        for (int i = 0; i < sourceQuests.Count; i++)
        {
            QuestDefinition quest = sourceQuests[i];

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

            if (quest == null || !QuestBelongsToCurrentQuestGiver(quest.definition))
            {
                continue;
            }

            if (quest.IsComplete(inventory))
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

            if (quest == null || !QuestBelongsToCurrentQuestGiver(quest.definition))
            {
                continue;
            }

            if (!quest.IsComplete(inventory))
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

        if (!QuestBelongsToCurrentQuestGiver(definition))
        {
            PostSystem("That quest does not belong to this quest giver.");
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

        if (!QuestBelongsToCurrentQuestGiver(quest.definition))
        {
            PostSystem("You cannot turn that quest in here.");
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

        if (!QuestBelongsToCurrentQuestGiver(quest.definition))
        {
            PostSystem("You cannot turn that quest in here.");
            return;
        }

        QuestDefinition completedQuest = quest.definition;
        PlayerInventory inventory = GetPlayerInventory();

        if (completedQuest == null || !quest.IsComplete(inventory))
        {
            return;
        }

        List<QuestItemReward> finalItemRewards =
            QuestRewardService.BuildFinalItemRewards(
                completedQuest,
                selectedChoiceRewardIndex);

        if (!QuestRewardService.CanFitRewardsAfterTurnIn(
                inventory,
                completedQuest.collectionObjectives,
                finalItemRewards))
        {
            PostSystem("Inventory is full. Make space before turning in this quest.");
            return;
        }

        QuestRewardService.ConsumeCollectionItems(
            inventory,
            completedQuest.collectionObjectives);

        QuestRewardService.GrantRewards(
            completedQuest,
            finalItemRewards,
            inventory,
            GetPlayerProgression());

        activeQuests.Remove(quest);

        if (!completedQuests.Contains(completedQuest))
        {
            completedQuests.Add(completedQuest);
        }

        PostSystem($"Quest completed: {completedQuest.title}.");

        RefreshQuestUI();
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

        for (int i = 0; i < activeQuests.Count; i++)
        {
            ActiveQuest quest = activeQuests[i];

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
                PostSystem($"Return to the correct quest giver to turn in {quest.definition.title}.");
            }
        }

        if (progressedAnyQuest)
        {
            PostSystem("Quest progress updated.");
        }
    }

    private IReadOnlyList<QuestDefinition> GetCurrentQuestSource()
    {
        if (currentQuestGiver != null)
        {
            return currentQuestGiver.AssignedQuests;
        }

        return quests;
    }

    private bool QuestBelongsToCurrentQuestGiver(QuestDefinition definition)
    {
        if (definition == null)
        {
            return false;
        }

        if (currentQuestGiver == null)
        {
            return true;
        }

        IReadOnlyList<QuestDefinition> assignedQuests = currentQuestGiver.AssignedQuests;

        if (assignedQuests == null)
        {
            return false;
        }

        for (int i = 0; i < assignedQuests.Count; i++)
        {
            if (assignedQuests[i] == definition)
            {
                return true;
            }
        }

        return false;
    }

    private void OpenQuestGiverUI()
    {
        if (QuestGiverUI.Instance != null)
        {
            QuestGiverUI.Instance.Open();
            return;
        }

        PostSystem("Quest giver UI is missing.");
    }

    private QuestDefinition GetFirstAvailableQuest()
    {
        List<QuestDefinition> available = GetAvailableQuests();
        return available.Count > 0 ? available[0] : null;
    }

    private ActiveQuest GetFirstCompletableQuest()
    {
        PlayerInventory inventory = GetPlayerInventory();

        for (int i = 0; i < activeQuests.Count; i++)
        {
            ActiveQuest quest = activeQuests[i];

            if (quest == null || !QuestBelongsToCurrentQuestGiver(quest.definition))
            {
                continue;
            }

            if (quest.IsComplete(inventory))
            {
                return quest;
            }
        }

        return null;
    }

    private bool IsQuestAlreadyActive(QuestDefinition definition)
    {
        for (int i = 0; i < activeQuests.Count; i++)
        {
            ActiveQuest quest = activeQuests[i];

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

        for (int i = 0; i < quest.definition.killObjectives.Count; i++)
        {
            QuestKillObjective objective = quest.definition.killObjectives[i];

            if (objective != null && objective.EnemyType == enemyType)
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
        return playerObject != null
            ? playerObject.GetComponent<PlayerInventory>()
            : null;
    }

    private PlayerProgression GetPlayerProgression()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        return playerObject != null
            ? playerObject.GetComponent<PlayerProgression>()
            : null;
    }

    private void RefreshQuestUI()
    {
        if (QuestGiverUI.Instance != null && QuestGiverUI.Instance.IsOpen)
        {
            QuestGiverUI.Instance.Refresh();
        }
    }

    private void PostSystem(string message)
    {
        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem(message);
        }
    }
}