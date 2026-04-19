using UnityEngine;

public class QuestGiver : MonoBehaviour
{
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private Transform player;
    [SerializeField] private KeyCode interactionKey = KeyCode.F;

    private void Start()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }
    }

    private void Update()
    {
        if (player == null || QuestManager.Instance == null)
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > interactionRange)
        {
            return;
        }

        if (Input.GetKeyDown(interactionKey))
        {
            QuestManager.Instance.InteractWithQuestGiver();
        }
    }
}