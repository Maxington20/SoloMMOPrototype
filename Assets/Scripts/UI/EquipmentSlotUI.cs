using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquipmentSlotUI : MonoBehaviour,
    IPointerClickHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IDropHandler
{
    [SerializeField] private EquipmentSlotType slotType;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image rarityBorderImage;
    [SerializeField] private GameObject emptyStateObject;

    private Action<EquipmentSlotType> onLeftClicked;
    private Action<EquipmentSlotType, Vector2> onRightClicked;
    private Action<EquipmentSlotType, PointerEventData> onBeginDragAction;
    private Action<PointerEventData> onDragAction;
    private Action<EquipmentSlotType, PointerEventData> onEndDragAction;
    private Action<EquipmentSlotType, PointerEventData> onDropAction;

    private ItemData currentItem;
    private bool isBlockedPreview;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private LayoutElement layoutElement;

    public EquipmentSlotType SlotType => slotType;
    public RectTransform RectTransform => rectTransform;
    public ItemData CurrentItem => currentItem;
    public bool IsBlockedPreview => isBlockedPreview;

    public void Initialize(Action<EquipmentSlotType> leftClickHandler)
    {
        onLeftClicked = leftClickHandler;
    }

    public void Initialize(Action<EquipmentSlotType> leftClickHandler, Action<EquipmentSlotType, Vector2> rightClickHandler)
    {
        onLeftClicked = leftClickHandler;
        onRightClicked = rightClickHandler;
    }

    public void Initialize(
        Action<EquipmentSlotType> leftClickHandler,
        Action<EquipmentSlotType, Vector2> rightClickHandler,
        Action<EquipmentSlotType, PointerEventData> beginDragHandler,
        Action<PointerEventData> dragHandler,
        Action<EquipmentSlotType, PointerEventData> endDragHandler,
        Action<EquipmentSlotType, PointerEventData> dropHandler)
    {
        onLeftClicked = leftClickHandler;
        onRightClicked = rightClickHandler;
        onBeginDragAction = beginDragHandler;
        onDragAction = dragHandler;
        onEndDragAction = endDragHandler;
        onDropAction = dropHandler;
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        layoutElement = GetComponent<LayoutElement>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        if (layoutElement == null)
        {
            layoutElement = gameObject.AddComponent<LayoutElement>();
        }

        DisableChildRaycasts();
    }

    public void Refresh(ItemData item)
    {
        RefreshInternal(item, false);
    }

    public void RefreshBlockedByTwoHandedWeapon(ItemData twoHandedWeapon)
    {
        RefreshInternal(twoHandedWeapon, true);
    }

    private void RefreshInternal(ItemData item, bool blockedPreview)
    {
        currentItem = item;
        isBlockedPreview = blockedPreview;

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
                iconImage.color = blockedPreview
                    ? new Color(1f, 1f, 1f, 0.35f)
                    : Color.white;
            }
        }

        if (rarityBorderImage != null)
        {
            rarityBorderImage.enabled = !isEmpty;
            rarityBorderImage.color = !isEmpty
                ? ItemRarityUtility.GetColor(item.Rarity)
                : Color.clear;

            if (blockedPreview)
            {
                Color borderColor = rarityBorderImage.color;
                borderColor.a = 0.35f;
                rarityBorderImage.color = borderColor;
            }
        }
    }

    public void SetDraggingVisualState(bool isDragging)
    {
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = !isDragging;
        }

        if (layoutElement != null)
        {
            layoutElement.ignoreLayout = isDragging;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isBlockedPreview)
        {
            return;
        }

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            onLeftClicked?.Invoke(slotType);
            return;
        }

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (currentItem == null)
            {
                ItemContextMenuUI.Instance?.Hide();
                ItemTooltipUI.Instance?.Hide();
                return;
            }

            onRightClicked?.Invoke(slotType, eventData.position);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isBlockedPreview || eventData.button != PointerEventData.InputButton.Left || currentItem == null)
        {
            return;
        }

        onBeginDragAction?.Invoke(slotType, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        onDragAction?.Invoke(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        onEndDragAction?.Invoke(slotType, eventData);
    }

    public void OnDrop(PointerEventData eventData)
    {
        onDropAction?.Invoke(slotType, eventData);
    }

    private void DisableChildRaycasts()
    {
        if (iconImage != null) iconImage.raycastTarget = false;
        if (rarityBorderImage != null) rarityBorderImage.raycastTarget = false;

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