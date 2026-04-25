using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestLogUI : MonoBehaviour
{
    [Header("Window")]
    [SerializeField] private GameObject questLogWindow;
    [SerializeField] private Button closeButton;

    [Header("Quest List")]
    [SerializeField] private Transform questListContainer;
    [SerializeField] private Button questListButtonPrefab;

    [Header("Detail Text")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private TMP_Text rewardText;
    [SerializeField] private TMP_Text statusText;

    [Header("Controls")]
    [SerializeField] private KeyCode toggleKey = KeyCode.L;

    private ActiveQuest selectedQuest;
    private bool isOpen;
    private int lastQuestCount = -1;

    private void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Close);
        }

        Close();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            Toggle();
        }

        if (!isOpen)
        {
            return;
        }

        int currentQuestCount = QuestManager.Instance != null
            ? QuestManager.Instance.ActiveQuests.Count
            : 0;

        if (currentQuestCount != lastQuestCount || !IsSelectedQuestStillActive())
        {
            RebuildQuestList();
        }

        RefreshDetails();
    }

    public void Toggle()
    {
        if (isOpen)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    public void Open()
    {
        isOpen = true;

        if (questLogWindow != null)
        {
            questLogWindow.SetActive(true);
        }

        RebuildQuestList();
        RefreshDetails();
    }

    public void Close()
    {
        isOpen = false;

        if (questLogWindow != null)
        {
            questLogWindow.SetActive(false);
        }
    }

    private void RebuildQuestList()
    {
        ClearQuestList();

        lastQuestCount = QuestManager.Instance != null
            ? QuestManager.Instance.ActiveQuests.Count
            : 0;

        if (QuestManager.Instance == null || QuestManager.Instance.ActiveQuests.Count == 0)
        {
            selectedQuest = null;
            return;
        }

        if (!IsSelectedQuestStillActive())
        {
            selectedQuest = QuestManager.Instance.ActiveQuests[0];
        }

        for (int i = 0; i < QuestManager.Instance.ActiveQuests.Count; i++)
        {
            ActiveQuest quest = QuestManager.Instance.ActiveQuests[i];

            if (quest == null || quest.definition == null)
            {
                continue;
            }

            ActiveQuest capturedQuest = quest;

            Button button = Instantiate(questListButtonPrefab, questListContainer);
            button.gameObject.SetActive(true);

            TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>(true);
            if (buttonText != null)
            {
                bool isSelected = capturedQuest == selectedQuest;
                bool isComplete = capturedQuest.IsComplete(PlayerInventory.Instance);

                string prefix = isSelected ? "> " : "  ";
                string suffix = isComplete ? " ✓" : string.Empty;

                buttonText.text = prefix + capturedQuest.definition.title + suffix;
                buttonText.color = isComplete ? Color.yellow : Color.white;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                SelectQuest(capturedQuest);
            });
        }
    }

    private void SelectQuest(ActiveQuest quest)
    {
        selectedQuest = quest;
        RebuildQuestList();
        RefreshDetails();
    }

    private void RefreshDetails()
    {
        if (selectedQuest == null || selectedQuest.definition == null)
        {
            ShowNoQuestSelected();
            return;
        }

        PlayerInventory inventory = PlayerInventory.Instance;
        QuestDefinition definition = selectedQuest.definition;

        if (titleText != null)
        {
            titleText.text = definition.title;
        }

        if (descriptionText != null)
        {
            descriptionText.text = definition.description;
        }

        if (objectiveText != null)
        {
            objectiveText.text = BuildObjectiveText(selectedQuest, inventory);
        }

        if (rewardText != null)
        {
            rewardText.text = BuildRewardText(definition);
        }

        if (statusText != null)
        {
            statusText.text = selectedQuest.IsComplete(inventory)
                ? "Complete - return to the quest giver."
                : "In progress";
        }
    }

    private void ClearQuestList()
    {
        if (questListContainer == null)
        {
            return;
        }

        for (int i = questListContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(questListContainer.GetChild(i).gameObject);
        }
    }

    private bool IsSelectedQuestStillActive()
    {
        if (selectedQuest == null || QuestManager.Instance == null)
        {
            return false;
        }

        for (int i = 0; i < QuestManager.Instance.ActiveQuests.Count; i++)
        {
            if (QuestManager.Instance.ActiveQuests[i] == selectedQuest)
            {
                return true;
            }
        }

        return false;
    }

    private string BuildObjectiveText(ActiveQuest quest, PlayerInventory inventory)
    {
        QuestDefinition definition = quest.definition;
        string text = "<b>Objectives</b>";

        bool hasObjective = false;

        if (definition.killObjectives != null)
        {
            foreach (QuestKillObjective objective in definition.killObjectives)
            {
                if (objective == null)
                {
                    continue;
                }

                text += $"\nKill {objective.RequiredAmount} {GetEnemyDisplayName(objective.EnemyType)} " +
                        $"({quest.GetKillProgress(objective.EnemyType)}/{objective.RequiredAmount})";
                hasObjective = true;
            }
        }

        if (definition.collectionObjectives != null)
        {
            foreach (QuestCollectionObjective objective in definition.collectionObjectives)
            {
                if (objective == null || objective.Item == null)
                {
                    continue;
                }

                int currentAmount = inventory != null ? inventory.GetTotalQuantityOfItem(objective.Item) : 0;

                text += $"\nCollect {objective.RequiredAmount} {objective.Item.DisplayName} " +
                        $"({currentAmount}/{objective.RequiredAmount})";
                hasObjective = true;
            }
        }

        if (!hasObjective)
        {
            text += "\nNone";
        }

        return text;
    }

    private string BuildRewardText(QuestDefinition definition)
    {
        string reward = "<b>Rewards</b>";
        bool hasReward = false;

        if (definition.xpReward > 0)
        {
            reward += $"\n{definition.xpReward} XP";
            hasReward = true;
        }

        if (definition.goldReward > 0)
        {
            reward += $"\n{definition.goldReward} Gold";
            hasReward = true;
        }

        if (definition.guaranteedItemRewards != null)
        {
            foreach (QuestItemReward itemReward in definition.guaranteedItemRewards)
            {
                if (itemReward == null || itemReward.Item == null)
                {
                    continue;
                }

                reward += itemReward.Quantity > 1
                    ? $"\n{itemReward.Item.DisplayName} x{itemReward.Quantity}"
                    : $"\n{itemReward.Item.DisplayName}";

                hasReward = true;
            }
        }

        if (definition.choiceItemRewards != null && definition.choiceItemRewards.Count > 0)
        {
            reward += "\n\n<b>Choose One</b>";
            hasReward = true;

            foreach (QuestItemReward itemReward in definition.choiceItemRewards)
            {
                if (itemReward == null || itemReward.Item == null)
                {
                    continue;
                }

                reward += itemReward.Quantity > 1
                    ? $"\n- {itemReward.Item.DisplayName} x{itemReward.Quantity}"
                    : $"\n- {itemReward.Item.DisplayName}";
            }
        }

        if (!hasReward)
        {
            reward += "\nNone";
        }

        return reward;
    }

    private void ShowNoQuestSelected()
    {
        if (titleText != null) titleText.text = "No Active Quest";
        if (descriptionText != null) descriptionText.text = "You do not currently have an active quest.";
        if (objectiveText != null) objectiveText.text = "<b>Objectives</b>\nNone";
        if (rewardText != null) rewardText.text = "<b>Rewards</b>\nNone";
        if (statusText != null) statusText.text = "No active quest";
    }

    private string GetEnemyDisplayName(EnemyType enemyType)
    {
        return enemyType switch
        {
            EnemyType.Wolf => "Wolves",
            EnemyType.Goblin => "Goblins",
            _ => enemyType.ToString()
        };
    }
}