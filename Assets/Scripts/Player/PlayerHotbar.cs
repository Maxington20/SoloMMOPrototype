using System;
using UnityEngine;

public class PlayerHotbar : MonoBehaviour
{
    public static PlayerHotbar Instance { get; private set; }

    public event Action OnHotbarChanged;

    [SerializeField] private int slotCount = 6;

    private ItemData[] assignedItems;
    private PlayerInventory playerInventory;

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

        assignedItems = new ItemData[SlotCount];
    }

    private void Update()
    {
        for (int i = 0; i < SlotCount; i++)
        {
            if (GetKeyForSlot(i) != KeyCode.None && Input.GetKeyDown(GetKeyForSlot(i)))
            {
                TryUseSlot(i);
            }
        }
    }

    public ItemData GetAssignedItem(int slotIndex)
    {
        if (!IsValidSlot(slotIndex))
        {
            return null;
        }

        return assignedItems[slotIndex];
    }

    public void AssignItemToSlot(int slotIndex, ItemData item)
    {
        if (!IsValidSlot(slotIndex))
        {
            return;
        }

        assignedItems[slotIndex] = item;
        OnHotbarChanged?.Invoke();
    }

    public void ClearSlot(int slotIndex)
    {
        if (!IsValidSlot(slotIndex))
        {
            return;
        }

        assignedItems[slotIndex] = null;
        OnHotbarChanged?.Invoke();
    }

    public int GetQuantityForSlot(int slotIndex)
    {
        if (!IsValidSlot(slotIndex) || playerInventory == null)
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

    public bool TryUseSlot(int slotIndex)
    {
        if (!IsValidSlot(slotIndex) || playerInventory == null)
        {
            return false;
        }

        ItemData item = assignedItems[slotIndex];
        if (item == null)
        {
            return false;
        }

        return playerInventory.TryUseFirstMatchingItem(item);
    }

    private bool IsValidSlot(int slotIndex)
    {
        return assignedItems != null && slotIndex >= 0 && slotIndex < assignedItems.Length;
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
}