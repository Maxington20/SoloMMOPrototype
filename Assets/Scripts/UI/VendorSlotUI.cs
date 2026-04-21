using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VendorSlotUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private GameObject emptyStateObject;

    private int slotIndex;
    private Action<int> onClicked;
    private ItemData currentItem;

    public void Initialize(int index, Action<int> clickHandler)
    {
        slotIndex = index;
        onClicked = clickHandler;
        gameObject.name = $"VendorSlot_{index}";
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

        if (itemNameText != null)
        {
            itemNameText.text = isEmpty ? string.Empty : item.DisplayName;
        }

        if (priceText != null)
        {
            priceText.text = isEmpty ? string.Empty : $"{item.BuyValue} Gold";
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        if (currentItem == null)
        {
            return;
        }

        if (ItemTooltipUI.Instance != null)
        {
            ItemTooltipUI.Instance.Hide();
        }

        onClicked?.Invoke(slotIndex);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentItem == null)
        {
            return;
        }

        if (ItemTooltipUI.Instance != null)
        {
            ItemTooltipUI.Instance.Show(currentItem);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (ItemTooltipUI.Instance != null)
        {
            ItemTooltipUI.Instance.Hide();
        }
    }

    private void DisableChildRaycasts()
    {
        if (iconImage != null)
        {
            iconImage.raycastTarget = false;
        }

        if (itemNameText != null)
        {
            itemNameText.raycastTarget = false;
        }

        if (priceText != null)
        {
            priceText.raycastTarget = false;
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