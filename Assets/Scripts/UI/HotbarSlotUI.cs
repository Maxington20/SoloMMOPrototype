using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HotbarSlotUI : MonoBehaviour, IPointerClickHandler, IDropHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private TMP_Text keybindText;
    [SerializeField] private GameObject emptyStateObject;

    private int slotIndex;
    private Action<int> onLeftClicked;
    private Action<int> onRightClicked;
    private Action<int, PointerEventData> onDropAction;

    public void Initialize(int index, Action<int> leftClickHandler, Action<int> rightClickHandler, Action<int, PointerEventData> dropHandler)
    {
        slotIndex = index;
        onLeftClicked = leftClickHandler;
        onRightClicked = rightClickHandler;
        onDropAction = dropHandler;
        gameObject.name = $"HotbarSlot_{index}";
    }

    private void Awake()
    {
        DisableChildRaycasts();
    }

    public void Refresh(ItemData item, int quantity)
    {
        bool isEmpty = item == null;

        if (keybindText != null)
        {
            keybindText.text = (slotIndex + 1).ToString();
        }

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

                if (quantity > 0)
                {
                    iconImage.color = Color.white;
                }
                else
                {
                    iconImage.color = new Color(1f, 1f, 1f, 0.35f);
                }
            }
        }

        if (quantityText != null)
        {
            quantityText.text = quantity > 1 ? quantity.ToString() : string.Empty;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            onLeftClicked?.Invoke(slotIndex);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            onRightClicked?.Invoke(slotIndex);
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        onDropAction?.Invoke(slotIndex, eventData);
    }

    private void DisableChildRaycasts()
    {
        if (iconImage != null)
        {
            iconImage.raycastTarget = false;
        }

        if (quantityText != null)
        {
            quantityText.raycastTarget = false;
        }

        if (keybindText != null)
        {
            keybindText.raycastTarget = false;
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
}