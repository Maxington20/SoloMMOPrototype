using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HotbarUI : MonoBehaviour
{
    [SerializeField] private Transform slotContainer;
    [SerializeField] private HotbarSlotUI slotPrefab;

    private readonly List<HotbarSlotUI> slotUIs = new List<HotbarSlotUI>();

    private void Start()
    {
        BuildSlots();
        RefreshAll();

        if (PlayerHotbar.Instance != null)
        {
            PlayerHotbar.Instance.OnHotbarChanged += HandleHotbarChanged;
        }

        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.OnInventoryChanged += HandleInventoryChanged;
        }
    }

    private void OnDestroy()
    {
        if (PlayerHotbar.Instance != null)
        {
            PlayerHotbar.Instance.OnHotbarChanged -= HandleHotbarChanged;
        }

        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.OnInventoryChanged -= HandleInventoryChanged;
        }
    }

    private void Update()
    {
        RefreshAll();
    }

    private void BuildSlots()
    {
        if (slotContainer == null || slotPrefab == null || PlayerHotbar.Instance == null)
        {
            return;
        }

        for (int i = slotContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(slotContainer.GetChild(i).gameObject);
        }

        slotUIs.Clear();

        for (int i = 0; i < PlayerHotbar.Instance.SlotCount; i++)
        {
            HotbarSlotUI slotUI = Instantiate(slotPrefab, slotContainer);
            slotUI.Initialize(i, HandleSlotLeftClicked, HandleSlotRightClicked, HandleSlotDrop);
            slotUIs.Add(slotUI);
        }
    }

    private void HandleSlotLeftClicked(int slotIndex)
    {
        if (PlayerHotbar.Instance == null)
        {
            return;
        }

        PlayerHotbar.Instance.TryUseSlot(slotIndex);
    }

    private void HandleSlotRightClicked(int slotIndex)
    {
        if (PlayerHotbar.Instance == null)
        {
            return;
        }

        PlayerHotbar.Instance.ClearSlot(slotIndex);
    }

    private void HandleSlotDrop(int slotIndex, PointerEventData eventData)
    {
        if (PlayerHotbar.Instance == null)
        {
            return;
        }

        if (ClassAbilityBookUI.Instance != null && ClassAbilityBookUI.Instance.IsDraggingAbility)
        {
            AbilityData ability = ClassAbilityBookUI.Instance.DraggedAbility;

            if (ability != null)
            {
                PlayerHotbar.Instance.AssignAbilityToSlot(slotIndex, ability);
                ClassAbilityBookUI.Instance.CompleteAbilityDrag(true);
                RefreshAll();
            }

            return;
        }

        if (PlayerInventory.Instance == null || InventoryUI.Instance == null)
        {
            return;
        }

        if (!InventoryUI.Instance.IsDraggingInventoryItem)
        {
            return;
        }

        InventorySlotData draggedSlot = PlayerInventory.Instance.GetSlot(InventoryUI.Instance.DraggedInventorySlotIndex);
        if (draggedSlot == null || draggedSlot.IsEmpty || draggedSlot.Item == null)
        {
            InventoryUI.Instance.CompleteExternalDrop(false);
            return;
        }

        ItemData item = draggedSlot.Item;

        if (!item.IsUsable)
        {
            InventoryUI.Instance.CompleteExternalDrop(false);
            return;
        }

        PlayerHotbar.Instance.AssignItemToSlot(slotIndex, item);
        InventoryUI.Instance.CompleteExternalDrop(false);
        RefreshAll();
    }

    private void HandleHotbarChanged()
    {
        RefreshAll();
    }

    private void HandleInventoryChanged()
    {
        RefreshAll();
    }

    private void RefreshAll()
    {
        if (PlayerHotbar.Instance == null)
        {
            return;
        }

        for (int i = 0; i < slotUIs.Count; i++)
        {
            switch (PlayerHotbar.Instance.GetSlotContentType(i))
            {
                case HotbarSlotContentType.Item:
                    slotUIs[i].RefreshItem(
                        PlayerHotbar.Instance.GetAssignedItem(i),
                        PlayerHotbar.Instance.GetQuantityForSlot(i));
                    break;

                case HotbarSlotContentType.Ability:
                    slotUIs[i].RefreshAbility(
                        PlayerHotbar.Instance.GetAssignedAbility(i),
                        PlayerHotbar.Instance.GetCooldownRemainingForSlot(i));
                    break;

                default:
                    slotUIs[i].RefreshEmpty();
                    break;
            }
        }
    }
}