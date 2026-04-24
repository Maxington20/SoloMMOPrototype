using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("Quest Definitions")]
    [SerializeField] private QuestDefinition wolfQuest = new QuestDefinition
    {
        questType = QuestType.KillWolves,
        title = "Wolf Hunt",
        description = "Cull the wolves threatening the outskirts.",
        targetEnemyType = EnemyType.Wolf,
        requiredKills = 3,
        xpReward = 100,
        goldReward = 10,
        itemRewardQuantity = 1
    };

    [SerializeField] private QuestDefinition goblinQuest = new QuestDefinition
    {
        questType = QuestType.KillGoblins,
        title = "Goblin Trouble",
        description = "Thin out the goblins near the woods.",
        targetEnemyType = EnemyType.Goblin,
        requiredKills = 2,
        xpReward = 150,
        goldReward = 20,
        itemRewardQuantity = 1
    };

    public ActiveQuest CurrentQuest { get; private set; }

    public bool HasActiveQuest => CurrentQuest != null;
    public bool HasCompletableQuest => CurrentQuest != null && CurrentQuest.IsComplete;

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
        if (CurrentQuest == null)
        {
            return "Press F to accept Wolf Hunt.";
        }

        if (CurrentQuest.IsComplete)
        {
            return $"Press F to turn in {CurrentQuest.definition.title}.";
        }

        return $"{CurrentQuest.definition.title}: {CurrentQuest.currentKills}/{CurrentQuest.definition.requiredKills}";
    }

    public void InteractWithQuestGiver()
    {
        if (CurrentQuest == null)
        {
            AcceptQuest(wolfQuest);
            return;
        }

        if (CurrentQuest.IsComplete)
        {
            TurnInQuest();
            return;
        }

        PostSystem($"Still working on {CurrentQuest.definition.title}.");
    }

    public void AcceptQuest(QuestDefinition definition)
    {
        if (definition == null || CurrentQuest != null)
        {
            return;
        }

        CurrentQuest = new ActiveQuest(definition);
        PostSystem($"Quest accepted: {definition.title}.");
    }

    public void TurnInQuest()
    {
        if (CurrentQuest == null || !CurrentQuest.IsComplete)
        {
            return;
        }

        QuestDefinition completedQuest = CurrentQuest.definition;
        if (completedQuest == null)
        {
            CurrentQuest = null;
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null)
        {
            return;
        }

        PlayerInventory inventory = playerObject.GetComponent<PlayerInventory>();
        PlayerProgression progression = playerObject.GetComponent<PlayerProgression>();

        int itemQuantity = Mathf.Max(1, completedQuest.itemRewardQuantity);

        if (completedQuest.itemReward != null)
        {
            if (inventory == null)
            {
                PostSystem("Cannot turn in quest because player inventory was not found.");
                return;
            }

            if (!inventory.CanAddItem(completedQuest.itemReward, itemQuantity))
            {
                PostSystem("Inventory is full. Make space before turning in this quest.");
                return;
            }
        }

        string completedTitle = completedQuest.title;
        QuestType completedType = completedQuest.questType;

        if (progression != null && completedQuest.xpReward > 0)
        {
            progression.AddXp(completedQuest.xpReward);
        }

        if (inventory != null && completedQuest.goldReward > 0)
        {
            inventory.AddGold(completedQuest.goldReward, false);
            PostSystem($"You receive {completedQuest.goldReward} gold.");
        }

        if (inventory != null && completedQuest.itemReward != null)
        {
            inventory.AddItem(completedQuest.itemReward, itemQuantity);
        }

        CurrentQuest = null;

        PostSystem($"Quest completed: {completedTitle}.");

        if (completedType == QuestType.KillWolves)
        {
            AcceptQuest(goblinQuest);
        }
    }

    public void RegisterEnemyKilled(GameObject enemyObject, GameObject killer)
    {
        if (CurrentQuest == null || enemyObject == null || killer == null)
        {
            return;
        }

        if (!killer.CompareTag("Player"))
        {
            return;
        }

        EnemyData enemyData = enemyObject.GetComponent<EnemyData>();

        if (enemyData == null)
        {
            return;
        }

        if (enemyData.EnemyType != CurrentQuest.definition.targetEnemyType)
        {
            return;
        }

        if (CurrentQuest.IsComplete)
        {
            return;
        }

        CurrentQuest.currentKills++;

        PostSystem($"{CurrentQuest.definition.title}: {CurrentQuest.currentKills}/{CurrentQuest.definition.requiredKills}");

        if (CurrentQuest.IsComplete)
        {
            PostSystem($"Return to the quest giver to turn in {CurrentQuest.definition.title}.");
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