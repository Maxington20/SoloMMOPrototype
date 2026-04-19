using TMPro;
using UnityEngine;

public class QuestTrackerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI questText;

    private void Update()
    {
        if (questText == null || QuestManager.Instance == null)
        {
            return;
        }

        if (!QuestManager.Instance.HasActiveQuest)
        {
            questText.text = "No active quest";
            return;
        }

        ActiveQuest quest = QuestManager.Instance.CurrentQuest;
        questText.text = $"{quest.definition.title}\n{quest.currentKills}/{quest.definition.requiredKills}";
    }
}