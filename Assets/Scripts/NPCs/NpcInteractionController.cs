using UnityEngine;

public class NpcInteractionController : MonoBehaviour
{
    [Header("NPC Data")]
    [SerializeField] private NpcData npcData;

    [Header("Interaction")]
    [SerializeField] private float interactionRange = 4f;
    [SerializeField] private bool openQuestUIBeforeVendor = true;

    private VendorNPC vendorNpc;
    private QuestGiver questGiver;
    private DisplayName displayName;

    public NpcData NpcData => npcData;
    public float InteractionRange => interactionRange;

    private void Awake()
    {
        CacheComponents();
        ApplyNpcDataToComponents();
    }

    private void OnValidate()
    {
        CacheComponents();
        ApplyNpcDataToComponents();
    }

    public void Interact(Transform player)
    {
        CacheComponents();
        ApplyNpcDataToComponents();

        if (player == null)
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > interactionRange)
        {
            PostSystem($"{GetDisplayName()} is too far away.");
            return;
        }

        bool progressedTalkQuest = false;

        if (QuestManager.Instance != null && npcData != null)
        {
            progressedTalkQuest = QuestManager.Instance.RegisterNpcTalkedTo(npcData);
        }

        if (openQuestUIBeforeVendor)
        {
            if (TryOpenQuestUI())
            {
                return;
            }

            if (TryOpenVendorUI())
            {
                return;
            }
        }
        else
        {
            if (TryOpenVendorUI())
            {
                return;
            }

            if (TryOpenQuestUI())
            {
                return;
            }
        }

        if (progressedTalkQuest)
        {
            return;
        }

        PostSystem($"{GetDisplayName()} has nothing to say right now.");
    }

    public string GetDisplayName()
    {
        if (npcData != null && !string.IsNullOrWhiteSpace(npcData.DisplayName))
        {
            return npcData.DisplayName;
        }

        if (displayName != null && !string.IsNullOrWhiteSpace(displayName.Display))
        {
            return displayName.Display;
        }

        return gameObject.name;
    }

    private void CacheComponents()
    {
        if (vendorNpc == null)
        {
            vendorNpc = GetComponent<VendorNPC>();
        }

        if (questGiver == null)
        {
            questGiver = GetComponent<QuestGiver>();
        }

        if (displayName == null)
        {
            displayName = GetComponent<DisplayName>();
        }
    }

    private bool TryOpenQuestUI()
    {
        bool canGiveQuests = npcData == null || npcData.CanGiveQuests;

        if (!canGiveQuests || questGiver == null)
        {
            return false;
        }

        questGiver.Interact();
        return true;
    }

    private bool TryOpenVendorUI()
    {
        bool canVendor = npcData == null || npcData.CanVendor;

        if (!canVendor || vendorNpc == null)
        {
            return false;
        }

        vendorNpc.OpenVendor();
        return true;
    }

    private void ApplyNpcDataToComponents()
    {
        if (npcData == null)
        {
            return;
        }

        if (displayName != null)
        {
            displayName.SetDisplayName(npcData.DisplayName);
        }

        if (vendorNpc != null)
        {
            vendorNpc.ApplyNpcData(npcData);
        }

        if (questGiver != null)
        {
            questGiver.ApplyNpcData(npcData);
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