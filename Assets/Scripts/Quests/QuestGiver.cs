using UnityEngine;

public class QuestGiver : MonoBehaviour
{
    public void Interact()
    {
        if (QuestManager.Instance == null)
        {
            PostSystem("Quest manager is missing.");
            return;
        }

        QuestManager.Instance.InteractWithQuestGiver();
    }

    private void PostSystem(string message)
    {
        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem(message);
        }
    }
}