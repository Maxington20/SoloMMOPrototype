using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestTrackerUI : MonoBehaviour
{
    [SerializeField] private GameObject trackerRoot;
    [SerializeField] private TMP_Text questText;

    private void Update()
    {
        Refresh();
    }

    private void Refresh()
    {
        if (QuestManager.Instance == null || QuestManager.Instance.ActiveQuests.Count == 0)
        {
            if (trackerRoot != null)
            {
                trackerRoot.SetActive(false);
            }

            return;
        }

        if (trackerRoot != null)
        {
            trackerRoot.SetActive(true);
        }

        if (questText == null)
        {
            return;
        }

        questText.text = BuildTrackerText();
    }

    private string BuildTrackerText()
    {
        string text = "";

        foreach (ActiveQuest quest in QuestManager.Instance.ActiveQuests)
        {
            if (quest == null || quest.definition == null)
            {
                continue;
            }

            text += $"<b>{quest.definition.title}</b>\n";
            text += $"Stage {quest.CurrentStageNumber}/{quest.TotalStageCount}: {quest.GetCurrentStageTitle()}\n";

            string summary = quest.GetCurrentStageSummary();

            if (!string.IsNullOrWhiteSpace(summary))
            {
                text += $"{summary}\n";
            }

            List<QuestKillObjective> killObjectives = quest.GetCurrentKillObjectives();

            if (killObjectives != null)
            {
                foreach (QuestKillObjective objective in killObjectives)
                {
                    if (objective == null)
                    {
                        continue;
                    }

                    text += $"- Kill {GetEnemyDisplayName(objective.EnemyType)} " +
                            $"({quest.GetKillProgress(objective.EnemyType)}/{objective.RequiredAmount})\n";
                }
            }

            List<QuestCollectionObjective> collectionObjectives = quest.GetCurrentCollectionObjectives();

            if (collectionObjectives != null)
            {
                foreach (QuestCollectionObjective objective in collectionObjectives)
                {
                    if (objective == null || objective.Item == null)
                    {
                        continue;
                    }

                    int currentAmount = PlayerInventory.Instance != null
                        ? PlayerInventory.Instance.GetTotalQuantityOfItem(objective.Item)
                        : 0;

                    text += $"- Collect {objective.Item.DisplayName} " +
                            $"({currentAmount}/{objective.RequiredAmount})\n";
                }
            }

            List<QuestTalkObjective> talkObjectives = quest.GetCurrentTalkObjectives();

            if (talkObjectives != null)
            {
                foreach (QuestTalkObjective objective in talkObjectives)
                {
                    if (objective == null || objective.Npc == null)
                    {
                        continue;
                    }

                    int currentAmount = quest.HasTalkedToNpc(objective.Npc) ? 1 : 0;

                    text += $"- Speak with {objective.DisplayName} ({currentAmount}/1)\n";
                }
            }

            List<QuestWorldInteractObjective> worldObjectives = quest.GetCurrentWorldInteractObjectives();

            if (worldObjectives != null)
            {
                foreach (QuestWorldInteractObjective objective in worldObjectives)
                {
                    if (objective == null || objective.Interactable == null)
                    {
                        continue;
                    }

                    int currentAmount = quest.HasInteractedWithWorldInteractable(objective.Interactable) ? 1 : 0;

                    text += $"- Interact with {objective.DisplayName} ({currentAmount}/1)\n";
                }
            }

            text += "\n";
        }

        return text.TrimEnd();
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