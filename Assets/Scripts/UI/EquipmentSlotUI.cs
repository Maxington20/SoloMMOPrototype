using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquipmentSlotUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private EquipmentSlotType slotType;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text textLabel;
    [SerializeField] private GameObject emptyStateObject;

    private Action<EquipmentSlotType> onLeftClicked;
    private Action<EquipmentSlotType, Vector2> onRightClicked;
    private ItemData currentItem;

    public EquipmentSlotType SlotType => slotType;

    public void Initialize(Action<EquipmentSlotType> leftClickHandler)
    {
        onLeftClicked = leftClickHandler;
        onRightClicked = null;
    }

    public void Initialize(Action<EquipmentSlotType> leftClickHandler, Action<EquipmentSlotType, Vector2> rightClickHandler)
    {
        onLeftClicked = leftClickHandler;
        onRightClicked = rightClickHandler;
    }

    private void Awake()
    {
        DisableChildRaycasts();
    }

    public void Refresh(ItemData item)
    {
        currentItem = item;
        bool isEmpty = item == null;

        if (emptyStateObject != null)
        {
            emptyStateObject.SetActive(isEmpty);
        }

        if (iconImage != null)
        {
            if (isEmpty)
            {
                iconImage.enabled = false;
                iconImage.sprite = null;
                iconImage.color = Color.white;
            }
            else
            {
                iconImage.enabled = true;
                iconImage.sprite = item.Icon;

                if (item.Icon != null)
                {
                    iconImage.color = Color.white;
                }
                else
                {
                    iconImage.color = new Color(0.18f, 0.18f, 0.18f, 0.9f);
                }
            }
        }

        if (textLabel != null)
        {
            textLabel.text = item != null ? item.DisplayName : GetSlotDisplayName(slotType);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            onLeftClicked?.Invoke(slotType);
            return;
        }

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (currentItem == null)
            {
                if (ItemContextMenuUI.Instance != null)
                {
                    ItemContextMenuUI.Instance.Hide();
                }

                if (ItemTooltipUI.Instance != null)
                {
                    ItemTooltipUI.Instance.Hide();
                }

                return;
            }

            onRightClicked?.Invoke(slotType, eventData.position);
        }
    }

    private void DisableChildRaycasts()
    {
        if (iconImage != null)
        {
            iconImage.raycastTarget = false;
        }

        if (textLabel != null)
        {
            textLabel.raycastTarget = false;
        }

        if (emptyStateObject != null)
        {
            Graphic[] graphics = emptyStateObject.GetComponentsInChildren<Graphic>(true);
            for (int i = 0; i < graphics.Length; i++)
            {
                graphics[i].raycastTarget = false;
            }
        }
    }

    private string GetSlotDisplayName(EquipmentSlotType type)
    {
        return type switch
        {
            EquipmentSlotType.Head => "Head",
            EquipmentSlotType.Chest => "Chest",
            EquipmentSlotType.Legs => "Legs",
            EquipmentSlotType.Feet => "Feet",
            EquipmentSlotType.Weapon => "Weapon",
            EquipmentSlotType.Offhand => "Offhand",
            _ => "Empty"
        };
    }
}