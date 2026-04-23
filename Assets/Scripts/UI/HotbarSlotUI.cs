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
    [SerializeField] private TMP_Text cooldownText;
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

    public void RefreshEmpty()
    {
        if (keybindText != null)
        {
            keybindText.text = (slotIndex + 1).ToString();
        }

        if (emptyStateObject != null)
        {
            emptyStateObject.SetActive(true);
        }

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

        if (cooldownText != null)
        {
            cooldownText.text = string.Empty;
        }
    }

    public void RefreshItem(ItemData item, int quantity)
    {
        if (keybindText != null)
        {
            keybindText.text = (slotIndex + 1).ToString();
        }

        if (emptyStateObject != null)
        {
            emptyStateObject.SetActive(item == null);
        }

        if (iconImage != null)
        {
            if (item == null)
            {
                iconImage.enabled = false;
                iconImage.sprite = null;
                iconImage.color = Color.white;
            }
            else
            {
                iconImage.enabled = true;
                iconImage.sprite = item.Icon;
                iconImage.color = quantity > 0
                    ? Color.white
                    : new Color(1f, 1f, 1f, 0.35f);
            }
        }

        if (quantityText != null)
        {
            quantityText.text = quantity > 1 ? quantity.ToString() : string.Empty;
        }

        if (cooldownText != null)
        {
            cooldownText.text = string.Empty;
        }
    }

    public void RefreshAbility(AbilityData ability, float cooldownRemaining)
    {
        if (keybindText != null)
        {
            keybindText.text = (slotIndex + 1).ToString();
        }

        if (emptyStateObject != null)
        {
            emptyStateObject.SetActive(ability == null);
        }

        if (iconImage != null)
        {
            if (ability == null)
            {
                iconImage.enabled = false;
                iconImage.sprite = null;
                iconImage.color = Color.white;
            }
            else
            {
                iconImage.enabled = true;
                iconImage.sprite = ability.Icon;
                iconImage.color = cooldownRemaining > 0f
                    ? new Color(1f, 1f, 1f, 0.45f)
                    : Color.white;
            }
        }

        if (quantityText != null)
        {
            quantityText.text = string.Empty;
        }

        if (cooldownText != null)
        {
            cooldownText.text = cooldownRemaining > 0f
                ? Mathf.CeilToInt(cooldownRemaining).ToString()
                : string.Empty;
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

        if (cooldownText != null)
        {
            cooldownText.raycastTarget = false;
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