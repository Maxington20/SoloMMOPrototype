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
        xpReward = 100
    };

    [SerializeField] private QuestDefinition goblinQuest = new QuestDefinition
    {
        questType = QuestType.KillGoblins,
        title = "Goblin Trouble",
        description = "Thin out the goblins near the woods.",
        targetEnemyType = EnemyType.Goblin,
        requiredKills = 2,
        xpReward = 150
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

        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem($"Still working on {CurrentQuest.definition.title}.");
        }
    }

    public void AcceptQuest(QuestDefinition definition)
    {
        if (definition == null || CurrentQuest != null)
        {
            return;
        }

        CurrentQuest = new ActiveQuest(definition);

        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem($"Quest accepted: {definition.title}.");
        }
    }

    public void TurnInQuest()
    {
        if (CurrentQuest == null || !CurrentQuest.IsComplete)
        {
            return;
        }

        string completedTitle = CurrentQuest.definition.title;
        QuestType completedType = CurrentQuest.definition.questType;
        int xpReward = CurrentQuest.definition.xpReward;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            PlayerProgression progression = playerObject.GetComponent<PlayerProgression>();
            if (progression != null)
            {
                progression.AddXp(xpReward);
            }
        }

        CurrentQuest = null;

        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem($"Quest completed: {completedTitle}.");
        }

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

        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem(
                $"{CurrentQuest.definition.title}: {CurrentQuest.currentKills}/{CurrentQuest.definition.requiredKills}");
        }

        if (CurrentQuest.IsComplete && ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem($"Return to the quest giver to turn in {CurrentQuest.definition.title}.");
        }
    }
}