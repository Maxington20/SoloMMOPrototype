using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private GameObject emptyStateObject;

    private int slotIndex;
    private Action<int> onLeftClicked;
    private Action<int, Vector2> onRightClicked;
    private ItemData currentItem;

    public void Initialize(int index, Action<int> leftClickHandler)
    {
        slotIndex = index;
        onLeftClicked = leftClickHandler;
        onRightClicked = null;
        gameObject.name = $"InventorySlot_{index}";
    }

    public void Initialize(int index, Action<int> leftClickHandler, Action<int, Vector2> rightClickHandler)
    {
        slotIndex = index;
        onLeftClicked = leftClickHandler;
        onRightClicked = rightClickHandler;
        gameObject.name = $"InventorySlot_{index}";
    }

    private void Awake()
    {
        DisableChildRaycasts();
    }

    public void Refresh(InventorySlotData slotData)
    {
        currentItem = null;

        if (slotData == null || slotData.IsEmpty)
        {
            SetEmptyVisual();
            return;
        }

        currentItem = slotData.Item;
        SetFilledVisual(slotData);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            onLeftClicked?.Invoke(slotIndex);
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

            onRightClicked?.Invoke(slotIndex, eventData.position);
        }
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

        if (itemNameText != null)
        {
            itemNameText.raycastTarget = false;
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

    private void SetEmptyVisual()
    {
        if (iconImage != null)
        {
            iconImage.enabled = false;
            iconImage.sprite = null;
            iconImage.color = Color.white;
        }

        if (quantityText != null)
        {
            quantityText.text = string.Empty;
        }

        if (itemNameText != null)
        {
            itemNameText.text = string.Empty;
        }

        if (emptyStateObject != null)
        {
            emptyStateObject.SetActive(true);
        }
    }

    private void SetFilledVisual(InventorySlotData slotData)
    {
        if (iconImage != null)
        {
            iconImage.enabled = true;
            iconImage.sprite = slotData.Item != null ? slotData.Item.Icon : null;

            if (slotData.Item != null && slotData.Item.Icon != null)
            {
                iconImage.color = new Color(0.0f, 0.0f, 0.0f, 0.9f);
            }
            else
            {
                iconImage.color = new Color(0.18f, 0.18f, 0.18f, 0.9f);
            }
        }

        if (quantityText != null)
        {
            quantityText.text = slotData.Quantity > 1 ? slotData.Quantity.ToString() : string.Empty;
        }

        if (itemNameText != null)
        {
            itemNameText.text = slotData.Item != null ? slotData.Item.DisplayName : string.Empty;
        }

        if (emptyStateObject != null)
        {
            emptyStateObject.SetActive(false);
        }
    }
}