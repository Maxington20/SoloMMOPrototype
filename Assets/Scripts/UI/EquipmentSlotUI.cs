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
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private LayoutElement layoutElement;

    public EquipmentSlotType SlotType => slotType;
    public RectTransform RectTransform => rectTransform;
    public ItemData CurrentItem => currentItem;

    public void Initialize(Action<EquipmentSlotType> leftClickHandler)
    {
        onLeftClicked = leftClickHandler;
        onRightClicked = null;
        onBeginDragAction = null;
        onDragAction = null;
        onEndDragAction = null;
        onDropAction = null;
    }

    public void Initialize(Action<EquipmentSlotType> leftClickHandler, Action<EquipmentSlotType, Vector2> rightClickHandler)
    {
        onLeftClicked = leftClickHandler;
        onRightClicked = rightClickHandler;
        onBeginDragAction = null;
        onDragAction = null;
        onEndDragAction = null;
        onDropAction = null;
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
                iconImage.color = item.Icon != null
                    ? Color.white
                    : new Color(0.18f, 0.18f, 0.18f, 0.9f);
            }
        }

        if (rarityBorderImage != null)
        {
            rarityBorderImage.enabled = !isEmpty;
            rarityBorderImage.color = !isEmpty
                ? ItemRarityUtility.GetColor(item.Rarity)
                : Color.clear;
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

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || currentItem == null)
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