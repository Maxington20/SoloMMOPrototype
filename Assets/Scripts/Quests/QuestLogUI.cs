using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestLogUI : MonoBehaviour
{
    [Header("Window")]
    [SerializeField] private GameObject questLogWindow;
    [SerializeField] private Button closeButton;

    [Header("Text")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private TMP_Text rewardText;
    [SerializeField] private TMP_Text statusText;

    [Header("Controls")]
    [SerializeField] private KeyCode toggleKey = KeyCode.L;

    private bool isOpen;

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

        if (isOpen)
        {
            Refresh();
        }
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

        Refresh();
    }

    public void Close()
    {
        isOpen = false;

        if (questLogWindow != null)
        {
            questLogWindow.SetActive(false);
        }
    }

    private void Refresh()
    {
        if (QuestManager.Instance == null || !QuestManager.Instance.HasActiveQuest)
        {
            ShowNoQuest();
            return;
        }

        ActiveQuest quest = QuestManager.Instance.CurrentQuest;
        if (quest == null || quest.definition == null)
        {
            ShowNoQuest();
            return;
        }

        QuestDefinition definition = quest.definition;

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
            objectiveText.text =
                $"Objective:\nKill {definition.requiredKills} {GetEnemyDisplayName(definition.targetEnemyType)}\n" +
                $"Progress: {quest.currentKills}/{definition.requiredKills}";
        }

        if (rewardText != null)
        {
            rewardText.text = BuildRewardText(definition);
        }

        if (statusText != null)
        {
            statusText.text = quest.IsComplete
                ? "Complete - return to the quest giver."
                : "In progress";
        }
    }

    private void ShowNoQuest()
    {
        if (titleText != null)
        {
            titleText.text = "No Active Quest";
        }

        if (descriptionText != null)
        {
            descriptionText.text = "You do not currently have an active quest.";
        }

        if (objectiveText != null)
        {
            objectiveText.text = "Objective:\nNone";
        }

        if (rewardText != null)
        {
            rewardText.text = "Reward:\nNone";
        }

        if (statusText != null)
        {
            statusText.text = "No active quest";
        }
    }

    private string BuildRewardText(QuestDefinition definition)
    {
        if (definition == null)
        {
            return "Reward:\nNone";
        }

        string reward = "Reward:";

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

        if (definition.itemReward != null)
        {
            int quantity = Mathf.Max(1, definition.itemRewardQuantity);
            reward += quantity > 1
                ? $"\n{definition.itemReward.DisplayName} x{quantity}"
                : $"\n{definition.itemReward.DisplayName}";
            hasReward = true;
        }

        if (!hasReward)
        {
            reward += "\nNone";
        }

        return reward;
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