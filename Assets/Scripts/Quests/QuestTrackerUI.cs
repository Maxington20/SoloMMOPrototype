using TMPro;
using UnityEngine;

public class QuestTrackerUI : MonoBehaviour
{
    [SerializeField] private GameObject trackerRoot;
    [SerializeField] private TMP_Text questTitleText;
    [SerializeField] private TMP_Text questObjectiveText;

    private void Update()
    {
        Refresh();
    }

    private void Refresh()
    {
        if (QuestManager.Instance == null || QuestManager.Instance.ActiveQuests.Count == 0)
        {
            SetVisible(false);
            return;
        }

        SetVisible(true);

        if (questTitleText != null)
        {
            questTitleText.text = "Active Quests";
        }

        if (questObjectiveText != null)
        {
            questObjectiveText.text = BuildTrackerText();
        }
    }

    private string BuildTrackerText()
    {
        string text = string.Empty;
        PlayerInventory inventory = PlayerInventory.Instance;

        foreach (ActiveQuest activeQuest in QuestManager.Instance.ActiveQuests)
        {
            if (activeQuest == null || activeQuest.definition == null)
            {
                continue;
            }

            if (!string.IsNullOrEmpty(text))
            {
                text += "\n\n";
            }

            QuestDefinition definition = activeQuest.definition;
            text += definition.title;

            if (definition.killObjectives != null)
            {
                foreach (QuestKillObjective objective in definition.killObjectives)
                {
                    if (objective == null)
                    {
                        continue;
                    }

                    text += $"\nKill {GetEnemyDisplayName(objective.EnemyType)}: " +
                            $"{activeQuest.GetKillProgress(objective.EnemyType)}/{objective.RequiredAmount}";
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

                    text += $"\nCollect {objective.Item.DisplayName}: {currentAmount}/{objective.RequiredAmount}";
                }
            }
        }

        return text;
    }

    private void SetVisible(bool visible)
    {
        if (trackerRoot != null)
        {
            trackerRoot.SetActive(visible);
        }
        else
        {
            gameObject.SetActive(visible);
        }
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