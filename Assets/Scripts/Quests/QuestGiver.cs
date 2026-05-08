using System.Collections.Generic;
using UnityEngine;

public class QuestGiver : MonoBehaviour
{
    [Header("Quests")]
    [SerializeField] private List<QuestDefinition> assignedQuests = new List<QuestDefinition>();

    public IReadOnlyList<QuestDefinition> AssignedQuests => assignedQuests;

    public void ApplyNpcData(NpcData npcData)
    {
        assignedQuests.Clear();

        if (npcData == null || npcData.Quests == null)
        {
            return;
        }

        for (int i = 0; i < npcData.Quests.Count; i++)
        {
            QuestDefinition quest = npcData.Quests[i];

            if (quest != null)
            {
                assignedQuests.Add(quest);
            }
        }
    }

    public void Interact()
    {
        if (QuestManager.Instance == null)
        {
            PostSystem("Quest manager is missing.");
            return;
        }

        if (assignedQuests.Count == 0)
        {
            PostSystem($"{gameObject.name} has no assigned quests.");
        }

        QuestManager.Instance.InteractWithQuestGiver(this);
    }

    private void PostSystem(string message)
    {
        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem(message);
        }
    }
}