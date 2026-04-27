using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterUI : MonoBehaviour
{
    public static CharacterUI Instance { get; private set; }

    [Header("Window")]
    [SerializeField] private GameObject characterWindow;

    [Header("Text")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text damageText;
    [SerializeField] private TMP_Text goldText;

    [Header("Equipment Slots")]
    [SerializeField] private EquipmentSlotUI headSlot;
    [SerializeField] private EquipmentSlotUI chestSlot;
    [SerializeField] private EquipmentSlotUI legsSlot;
    [SerializeField] private EquipmentSlotUI feetSlot;
    [SerializeField] private EquipmentSlotUI weaponSlot;
    [SerializeField] private EquipmentSlotUI offhandSlot;

    [Header("Player References")]
    [SerializeField] private DisplayName displayName;
    [SerializeField] private PlayerProgression progression;
    [SerializeField] private Health health;
    [SerializeField] private PlayerCombat combat;
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private PlayerEquipment equipment;

    [Header("Controls")]
    [SerializeField] private KeyCode toggleKey = KeyCode.C;

    private bool isOpen;

    private bool isDraggingEquipment;
    private bool dropHandledThisDrag;
    private EquipmentSlotType draggedEquipmentSlotType = EquipmentSlotType.None;

    private Canvas rootCanvas;
    private RectTransform canvasRectTransform;

    private EquipmentSlotUI draggedSlotUI;
    private Transform draggedOriginalParent;
    private int draggedOriginalSiblingIndex;

    private GameObject dragPlaceholderObject;

    public bool IsDraggingEquipmentItem => isDraggingEquipment;
    public EquipmentSlotType DraggedEquipmentSlotType => draggedEquipmentSlotType;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        rootCanvas = GetComponentInParent<Canvas>();

        if (rootCanvas != null)
        {
            canvasRectTransform = rootCanvas.transform as RectTransform;
        }

        if (characterWindow != null)
        {
            characterWindow.SetActive(false);
        }

        if (headSlot != null) headSlot.Initialize(HandleEquipmentSlotLeftClicked, HandleEquipmentSlotRightClicked, HandleEquipmentSlotBeginDrag, HandleEquipmentSlotDrag, HandleEquipmentSlotEndDrag, HandleEquipmentSlotDrop);
        if (chestSlot != null) chestSlot.Initialize(HandleEquipmentSlotLeftClicked, HandleEquipmentSlotRightClicked, HandleEquipmentSlotBeginDrag, HandleEquipmentSlotDrag, HandleEquipmentSlotEndDrag, HandleEquipmentSlotDrop);
        if (legsSlot != null) legsSlot.Initialize(HandleEquipmentSlotLeftClicked, HandleEquipmentSlotRightClicked, HandleEquipmentSlotBeginDrag, HandleEquipmentSlotDrag, HandleEquipmentSlotEndDrag, HandleEquipmentSlotDrop);
        if (feetSlot != null) feetSlot.Initialize(HandleEquipmentSlotLeftClicked, HandleEquipmentSlotRightClicked, HandleEquipmentSlotBeginDrag, HandleEquipmentSlotDrag, HandleEquipmentSlotEndDrag, HandleEquipmentSlotDrop);
        if (weaponSlot != null) weaponSlot.Initialize(HandleEquipmentSlotLeftClicked, HandleEquipmentSlotRightClicked, HandleEquipmentSlotBeginDrag, HandleEquipmentSlotDrag, HandleEquipmentSlotEndDrag, HandleEquipmentSlotDrop);
        if (offhandSlot != null)
        {
            ItemData equippedWeapon = equipment.GetEquippedItem(EquipmentSlotType.Weapon);

            if (equipment.IsOffhandBlockedByTwoHandedWeapon())
            {
                offhandSlot.RefreshBlockedByTwoHandedWeapon(equippedWeapon);
            }
            else
            {
                offhandSlot.Refresh(equipment.GetEquippedItem(EquipmentSlotType.Offhand));
            }
        }            
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            Toggle();
        }

        if (isOpen)
        {
            Refresh();
        }

        if (isDraggingEquipment)
        {
            UpdateDraggedSlotPosition(Input.mousePosition);
        }
    }

    public void CompleteExternalDrop(bool moved)
    {
        if (!isDraggingEquipment)
        {
            return;
        }

        dropHandledThisDrag = true;

        RestoreDraggedSlotVisual();

        if (moved)
        {
            Refresh();
        }

        ClearDragState();
    }

    private void Toggle()
    {
        isOpen = !isOpen;

        if (characterWindow != null)
        {
            characterWindow.SetActive(isOpen);
        }

        if (!isOpen)
        {
            if (ItemContextMenuUI.Instance != null)
            {
                ItemContextMenuUI.Instance.Hide();
            }

            if (ItemTooltipUI.Instance != null)
            {
                ItemTooltipUI.Instance.Hide();
            }

            if (isDraggingEquipment)
            {
                RestoreDraggedSlotVisual();
                ClearDragState();
            }
        }

        if (isOpen)
        {
            Refresh();
        }
    }

    private void Refresh()
    {
        if (nameText != null)
        {
            nameText.text = displayName != null ? displayName.Display : "Unknown";
        }

        if (levelText != null)
        {
            levelText.text = progression != null ? $"Level: {progression.Level}" : "Level: ?";
        }

        if (healthText != null)
        {
            healthText.text = health != null ? $"Health: {health.CurrentHealth}/{health.MaxHealth}" : "Health: ?";
        }

        if (damageText != null)
        {
            damageText.text = combat != null ? $"Damage: {combat.Damage}" : "Damage: ?";
        }

        if (goldText != null)
        {
            goldText.text = inventory != null ? $"Gold: {inventory.Gold}" : "Gold: 0";
        }

        RefreshEquipmentSlots();
    }

    private void RefreshEquipmentSlots()
    {
        if (equipment == null)
        {
            return;
        }

        if (headSlot != null) headSlot.Refresh(equipment.GetEquippedItem(EquipmentSlotType.Head));
        if (chestSlot != null) chestSlot.Refresh(equipment.GetEquippedItem(EquipmentSlotType.Chest));
        if (legsSlot != null) legsSlot.Refresh(equipment.GetEquippedItem(EquipmentSlotType.Legs));
        if (feetSlot != null) feetSlot.Refresh(equipment.GetEquippedItem(EquipmentSlotType.Feet));
        if (weaponSlot != null) weaponSlot.Refresh(equipment.GetEquippedItem(EquipmentSlotType.Weapon));
        if (offhandSlot != null) offhandSlot.Refresh(equipment.GetEquippedItem(EquipmentSlotType.Offhand));
    }

    private void HandleEquipmentSlotLeftClicked(EquipmentSlotType slotType)
    {
        // Intentionally empty.
        // Left mouse is reserved for drag-and-drop for equipped items.
    }

    private void HandleEquipmentSlotRightClicked(EquipmentSlotType slotType, Vector2 screenPosition)
    {
        if (!isOpen || isDraggingEquipment || equipment == null || ItemContextMenuUI.Instance == null)
        {
            return;
        }

        ItemData item = equipment.GetEquippedItem(slotType);
        if (item == null)
        {
            ItemContextMenuUI.Instance.Hide();
            return;
        }

        if (ItemTooltipUI.Instance != null)
        {
            ItemTooltipUI.Instance.Hide();
        }

        ItemContextMenuUI.Instance.OpenForEquipmentSlot(slotType, item, screenPosition);
    }

    private void HandleEquipmentSlotBeginDrag(EquipmentSlotType slotType, PointerEventData eventData)
    {
        if (!isOpen || equipment == null || rootCanvas == null)
        {
            return;
        }

        ItemData item = equipment.GetEquippedItem(slotType);
        if (item == null)
        {
            return;
        }

        EquipmentSlotUI slotUI = GetSlotUI(slotType);
        if (slotUI == null)
        {
            return;
        }

        isDraggingEquipment = true;
        dropHandledThisDrag = false;
        draggedEquipmentSlotType = slotType;
        draggedSlotUI = slotUI;
        draggedOriginalParent = slotUI.transform.parent;
        draggedOriginalSiblingIndex = slotUI.transform.GetSiblingIndex();

        if (ItemContextMenuUI.Instance != null)
        {
            ItemContextMenuUI.Instance.Hide();
        }

        if (ItemTooltipUI.Instance != null)
        {
            ItemTooltipUI.Instance.Hide();
        }

        CreateDragPlaceholder(slotUI);

        draggedSlotUI.SetDraggingVisualState(true);
        draggedSlotUI.transform.SetParent(rootCanvas.transform, true);
        draggedSlotUI.transform.SetAsLastSibling();

        UpdateDraggedSlotPosition(eventData.position);
    }

    private void HandleEquipmentSlotDrag(PointerEventData eventData)
    {
        if (!isDraggingEquipment)
        {
            return;
        }

        UpdateDraggedSlotPosition(eventData.position);
    }

    private void HandleEquipmentSlotEndDrag(EquipmentSlotType slotType, PointerEventData eventData)
    {
        if (!isDraggingEquipment)
        {
            return;
        }

        if (!dropHandledThisDrag)
        {
            RestoreDraggedSlotVisual();
            ClearDragState();
        }
    }

    private void HandleEquipmentSlotDrop(EquipmentSlotType targetSlotType, PointerEventData eventData)
    {
        if (PlayerInventory.Instance == null)
        {
            return;
        }

        if (InventoryUI.Instance != null && InventoryUI.Instance.IsDraggingInventoryItem)
        {
            bool moved = PlayerInventory.Instance.TryEquipFromSlotToEquipmentSlot(
                InventoryUI.Instance.DraggedInventorySlotIndex,
                targetSlotType);

            InventoryUI.Instance.CompleteExternalDrop(moved);

            if (moved)
            {
                Refresh();
            }
        }
    }

    private EquipmentSlotUI GetSlotUI(EquipmentSlotType slotType)
    {
        return slotType switch
        {
            EquipmentSlotType.Head => headSlot,
            EquipmentSlotType.Chest => chestSlot,
            EquipmentSlotType.Legs => legsSlot,
            EquipmentSlotType.Feet => feetSlot,
            EquipmentSlotType.Weapon => weaponSlot,
            EquipmentSlotType.Offhand => offhandSlot,
            _ => null
        };
    }

    private void UpdateDraggedSlotPosition(Vector2 screenPosition)
    {
        if (!isDraggingEquipment || draggedSlotUI == null || draggedSlotUI.RectTransform == null)
        {
            return;
        }

        if (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            if (canvasRectTransform != null &&
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRectTransform,
                    screenPosition,
                    rootCanvas.worldCamera,
                    out Vector2 localPoint))
            {
                draggedSlotUI.RectTransform.localPosition = localPoint;
                return;
            }
        }

        draggedSlotUI.RectTransform.position = screenPosition;
    }

    private void CreateDragPlaceholder(EquipmentSlotUI slotUI)
    {
        if (slotUI == null || draggedOriginalParent == null)
        {
            return;
        }

        DestroyDragPlaceholder();

        dragPlaceholderObject = Instantiate(slotUI.gameObject, draggedOriginalParent);
        dragPlaceholderObject.transform.SetSiblingIndex(draggedOriginalSiblingIndex);
        dragPlaceholderObject.name = "EquipmentDragPlaceholder";

        RectTransform sourceRect = slotUI.GetComponent<RectTransform>();
        RectTransform placeholderRect = dragPlaceholderObject.GetComponent<RectTransform>();

        if (sourceRect != null && placeholderRect != null)
        {
            placeholderRect.anchorMin = sourceRect.anchorMin;
            placeholderRect.anchorMax = sourceRect.anchorMax;
            placeholderRect.pivot = sourceRect.pivot;
            placeholderRect.anchoredPosition = sourceRect.anchoredPosition;
            placeholderRect.sizeDelta = sourceRect.sizeDelta;
            placeholderRect.localScale = sourceRect.localScale;
            placeholderRect.localRotation = sourceRect.localRotation;
        }

        EquipmentSlotUI placeholderSlot = dragPlaceholderObject.GetComponent<EquipmentSlotUI>();
        if (placeholderSlot != null)
        {
            placeholderSlot.Refresh(null);
            placeholderSlot.SetDraggingVisualState(false);
            placeholderSlot.enabled = false;
        }

        CanvasGroup placeholderCanvasGroup = dragPlaceholderObject.GetComponent<CanvasGroup>();
        if (placeholderCanvasGroup != null)
        {
            placeholderCanvasGroup.blocksRaycasts = false;
            placeholderCanvasGroup.interactable = false;
        }
    }

    private void RestoreDraggedSlotVisual()
    {
        if (draggedSlotUI == null)
        {
            DestroyDragPlaceholder();
            return;
        }

        draggedSlotUI.transform.SetParent(draggedOriginalParent, false);

        RectTransform draggedRect = draggedSlotUI.GetComponent<RectTransform>();

        if (dragPlaceholderObject != null)
        {
            RectTransform placeholderRect = dragPlaceholderObject.GetComponent<RectTransform>();
            int placeholderSiblingIndex = dragPlaceholderObject.transform.GetSiblingIndex();
            draggedSlotUI.transform.SetSiblingIndex(placeholderSiblingIndex);

            if (draggedRect != null && placeholderRect != null)
            {
                draggedRect.anchorMin = placeholderRect.anchorMin;
                draggedRect.anchorMax = placeholderRect.anchorMax;
                draggedRect.pivot = placeholderRect.pivot;
                draggedRect.anchoredPosition = placeholderRect.anchoredPosition;
                draggedRect.sizeDelta = placeholderRect.sizeDelta;
                draggedRect.localScale = placeholderRect.localScale;
                draggedRect.localRotation = placeholderRect.localRotation;
            }
        }
        else
        {
            draggedSlotUI.transform.SetSiblingIndex(draggedOriginalSiblingIndex);
        }

        draggedSlotUI.SetDraggingVisualState(false);
        DestroyDragPlaceholder();
    }

    private void DestroyDragPlaceholder()
    {
        if (dragPlaceholderObject != null)
        {
            Destroy(dragPlaceholderObject);
            dragPlaceholderObject = null;
        }
    }

    private void ClearDragState()
    {
        isDraggingEquipment = false;
        dropHandledThisDrag = false;
        draggedEquipmentSlotType = EquipmentSlotType.None;
        draggedSlotUI = null;
        draggedOriginalParent = null;
        draggedOriginalSiblingIndex = -1;
    }
}