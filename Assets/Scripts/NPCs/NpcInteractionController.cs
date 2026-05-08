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
        vendorNpc = GetComponent<VendorNPC>();
        questGiver = GetComponent<QuestGiver>();
        displayName = GetComponent<DisplayName>();

        ApplyNpcDataToComponents();
    }

    public void Interact(Transform player)
    {
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
    }

    private void PostSystem(string message)
    {
        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem(message);
        }
    }
}