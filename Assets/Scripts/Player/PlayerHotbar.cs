using System;
using UnityEngine;

public enum HotbarSlotContentType
{
    Empty,
    Item,
    Ability
}

public class PlayerHotbar : MonoBehaviour
{
    public static PlayerHotbar Instance { get; private set; }

    public event Action OnHotbarChanged;

    [SerializeField] private int slotCount = 6;
    [SerializeField] private AbilityData[] startingAbilityAssignments = new AbilityData[6];

    private HotbarSlotContentType[] slotTypes;
    private ItemData[] assignedItems;
    private AbilityData[] assignedAbilities;

    private PlayerInventory playerInventory;
    private PlayerAbilityController playerAbilityController;

    public int SlotCount => Mathf.Max(1, slotCount);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        playerInventory = GetComponent<PlayerInventory>();
        playerAbilityController = GetComponent<PlayerAbilityController>();

        slotTypes = new HotbarSlotContentType[SlotCount];
        assignedItems = new ItemData[SlotCount];
        assignedAbilities = new AbilityData[SlotCount];

        for (int i = 0; i < SlotCount; i++)
        {
            if (startingAbilityAssignments != null &&
                i < startingAbilityAssignments.Length &&
                startingAbilityAssignments[i] != null)
            {
                slotTypes[i] = HotbarSlotContentType.Ability;
                assignedAbilities[i] = startingAbilityAssignments[i];
            }
            else
            {
                slotTypes[i] = HotbarSlotContentType.Empty;
            }
        }
    }

    private void Update()
    {
        for (int i = 0; i < SlotCount; i++)
        {
            KeyCode key = GetKeyForSlot(i);
            if (key != KeyCode.None && Input.GetKeyDown(key))
            {
                TryUseSlot(i);
            }
        }
    }

    public HotbarSlotContentType GetSlotContentType(int slotIndex)
    {
        if (!IsValidSlot(slotIndex))
        {
            return HotbarSlotContentType.Empty;
        }

        return slotTypes[slotIndex];
    }

    public ItemData GetAssignedItem(int slotIndex)
    {
        if (!IsValidSlot(slotIndex))
        {
            return null;
        }

        return assignedItems[slotIndex];
    }

    public AbilityData GetAssignedAbility(int slotIndex)
    {
        if (!IsValidSlot(slotIndex))
        {
            return null;
        }

        return assignedAbilities[slotIndex];
    }

    public void AssignItemToSlot(int slotIndex, ItemData item)
    {
        if (!IsValidSlot(slotIndex) || item == null)
        {
            return;
        }

        if (!item.IsUsable)
        {
            PostSystem($"{item.DisplayName} cannot be placed on the hotbar.");
            return;
        }

        slotTypes[slotIndex] = HotbarSlotContentType.Item;
        assignedItems[slotIndex] = item;
        assignedAbilities[slotIndex] = null;

        OnHotbarChanged?.Invoke();
    }

    public void AssignAbilityToSlot(int slotIndex, AbilityData ability)
    {
        if (!IsValidSlot(slotIndex) || ability == null)
        {
            return;
        }

        slotTypes[slotIndex] = HotbarSlotContentType.Ability;
        assignedAbilities[slotIndex] = ability;
        assignedItems[slotIndex] = null;

        OnHotbarChanged?.Invoke();
    }

    public void ClearSlot(int slotIndex)
    {
        if (!IsValidSlot(slotIndex))
        {
            return;
        }

        slotTypes[slotIndex] = HotbarSlotContentType.Empty;
        assignedItems[slotIndex] = null;
        assignedAbilities[slotIndex] = null;

        OnHotbarChanged?.Invoke();
    }

    public int GetQuantityForSlot(int slotIndex)
    {
        if (!IsValidSlot(slotIndex) || playerInventory == null)
        {
            return 0;
        }

        if (slotTypes[slotIndex] != HotbarSlotContentType.Item)
        {
            return 0;
        }

        ItemData item = assignedItems[slotIndex];
        if (item == null)
        {
            return 0;
        }

        return playerInventory.GetTotalQuantityOfItem(item);
    }

    public float GetCooldownRemainingForSlot(int slotIndex)
    {
        if (!IsValidSlot(slotIndex) || playerAbilityController == null)
        {
            return 0f;
        }

        if (slotTypes[slotIndex] != HotbarSlotContentType.Ability)
        {
            return 0f;
        }

        AbilityData ability = assignedAbilities[slotIndex];
        if (ability == null)
        {
            return 0f;
        }

        return playerAbilityController.GetRemainingCooldown(ability);
    }

    public bool TryUseSlot(int slotIndex)
    {
        if (!IsValidSlot(slotIndex))
        {
            return false;
        }

        switch (slotTypes[slotIndex])
        {
            case HotbarSlotContentType.Empty:
                return false;

            case HotbarSlotContentType.Item:
                if (playerInventory == null)
                {
                    return false;
                }

                ItemData item = assignedItems[slotIndex];
                if (item == null)
                {
                    return false;
                }

                if (playerInventory.GetTotalQuantityOfItem(item) <= 0)
                {
                    PostSystem($"You do not have any {item.DisplayName}.");
                    return false;
                }

                return playerInventory.TryUseFirstMatchingItem(item);

            case HotbarSlotContentType.Ability:
                if (playerAbilityController == null)
                {
                    return false;
                }

                AbilityData ability = assignedAbilities[slotIndex];
                if (ability == null)
                {
                    return false;
                }

                return playerAbilityController.TryUseAbility(ability);
        }

        return false;
    }

    private bool IsValidSlot(int slotIndex)
    {
        return slotTypes != null && slotIndex >= 0 && slotIndex < slotTypes.Length;
    }

    private KeyCode GetKeyForSlot(int slotIndex)
    {
        return slotIndex switch
        {
            0 => KeyCode.Alpha1,
            1 => KeyCode.Alpha2,
            2 => KeyCode.Alpha3,
            3 => KeyCode.Alpha4,
            4 => KeyCode.Alpha5,
            5 => KeyCode.Alpha6,
            _ => KeyCode.None
        };
    }

    private void PostSystem(string message)
    {
        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem(message);
        }
    }
}