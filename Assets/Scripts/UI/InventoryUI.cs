using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }

    [Header("Window")]
    [SerializeField] private GameObject inventoryWindow;
    [SerializeField] private Transform slotContainer;
    [SerializeField] private InventorySlotUI slotPrefab;
    [SerializeField] private TMP_Text goldText;

    [Header("Settings")]
    [SerializeField] private KeyCode primaryToggleKey = KeyCode.I;
    [SerializeField] private KeyCode secondaryToggleKey = KeyCode.B;

    private readonly List<InventorySlotUI> slotUIs = new List<InventorySlotUI>();

    private bool isOpen;
    private bool isDragging;
    private bool dropHandledThisDrag;
    private int dragSourceSlotIndex = -1;

    private Canvas rootCanvas;
    private RectTransform canvasRectTransform;

    private InventorySlotUI draggedSlotUI;
    private Transform draggedOriginalParent;
    private int draggedOriginalSiblingIndex;

    private InventorySlotUI dragPlaceholderSlotUI;

    public bool IsOpen => isOpen;
    public bool IsDraggingInventoryItem => isDragging;
    public int DraggedInventorySlotIndex => dragSourceSlotIndex;

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
        isOpen = false;
        rootCanvas = GetComponentInParent<Canvas>();

        if (rootCanvas != null)
        {
            canvasRectTransform = rootCanvas.transform as RectTransform;
        }

        if (inventoryWindow != null)
        {
            inventoryWindow.SetActive(false);
        }

        BuildSlots();
        RefreshAll();

        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.OnGoldChanged += HandleGoldChanged;
            PlayerInventory.Instance.OnInventoryChanged += HandleInventoryChanged;
        }

        ApplyCursorState();
    }

    private void OnDestroy()
    {
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.OnGoldChanged -= HandleGoldChanged;
            PlayerInventory.Instance.OnInventoryChanged -= HandleInventoryChanged;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(primaryToggleKey) || Input.GetKeyDown(secondaryToggleKey))
        {
            ToggleInventory();
        }

        if (isDragging)
        {
            UpdateDraggedSlotPosition(Input.mousePosition);
        }
    }

    private void BuildSlots()
    {
        if (slotContainer == null || slotPrefab == null || PlayerInventory.Instance == null)
        {
            return;
        }

        for (int i = slotContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(slotContainer.GetChild(i).gameObject);
        }

        slotUIs.Clear();

        for (int i = 0; i < PlayerInventory.Instance.SlotCount; i++)
        {
            InventorySlotUI slotUI = Instantiate(slotPrefab, slotContainer);
            slotUI.Initialize(
                i,
                HandleSlotLeftClicked,
                HandleSlotRightClicked,
                HandleSlotBeginDrag,
                HandleSlotDrag,
                HandleSlotEndDrag,
                HandleSlotDrop);
            slotUIs.Add(slotUI);
        }
    }

    private void HandleSlotLeftClicked(int slotIndex)
    {
        // Intentionally empty.
        // Left mouse is reserved for drag-and-drop in the main inventory window.
    }

    private void HandleSlotRightClicked(int slotIndex, Vector2 screenPosition)
    {
        if (!isOpen || isDragging || PlayerInventory.Instance == null || ItemContextMenuUI.Instance == null)
        {
            return;
        }

        InventorySlotData slotData = PlayerInventory.Instance.GetSlot(slotIndex);
        if (slotData == null || slotData.IsEmpty || slotData.Item == null)
        {
            ItemContextMenuUI.Instance.Hide();
            return;
        }

        if (ItemTooltipUI.Instance != null)
        {
            ItemTooltipUI.Instance.Hide();
        }

        ItemContextMenuUI.Instance.OpenForInventorySlot(slotIndex, slotData.Item, screenPosition);
    }

    private void HandleSlotBeginDrag(int slotIndex, PointerEventData eventData)
    {
        if (!isOpen || PlayerInventory.Instance == null || rootCanvas == null)
        {
            return;
        }

        InventorySlotData slotData = PlayerInventory.Instance.GetSlot(slotIndex);
        if (slotData == null || slotData.IsEmpty || slotData.Item == null)
        {
            return;
        }

        InventorySlotUI slotUI = GetSlotUI(slotIndex);
        if (slotUI == null)
        {
            return;
        }

        dragSourceSlotIndex = slotIndex;
        isDragging = true;
        dropHandledThisDrag = false;
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

        CreateDragPlaceholder();

        draggedSlotUI.SetDraggingVisualState(true);
        draggedSlotUI.transform.SetParent(rootCanvas.transform, true);
        draggedSlotUI.transform.SetAsLastSibling();

        UpdateDraggedSlotPosition(eventData.position);
    }

    private void HandleSlotDrag(PointerEventData eventData)
    {
        if (!isDragging)
        {
            return;
        }

        UpdateDraggedSlotPosition(eventData.position);
    }

    private void HandleSlotEndDrag(int slotIndex, PointerEventData eventData)
    {
        if (!isDragging)
        {
            return;
        }

        if (!dropHandledThisDrag)
        {
            RestoreDraggedSlotVisual();
            ClearDragState();
        }
    }

    private void HandleSlotDrop(int targetSlotIndex, PointerEventData eventData)
    {
        if (PlayerInventory.Instance == null)
        {
            return;
        }

        if (isDragging)
        {
            dropHandledThisDrag = true;

            int sourceIndex = dragSourceSlotIndex;
            bool moved = PlayerInventory.Instance.TryMoveOrSwapSlot(sourceIndex, targetSlotIndex);

            RestoreDraggedSlotVisual();

            if (moved)
            {
                RefreshSlots();
            }

            ClearDragState();
            return;
        }

        if (CharacterUI.Instance != null && CharacterUI.Instance.IsDraggingEquipmentItem)
        {
            bool moved = PlayerInventory.Instance.TryMoveEquippedItemToInventorySlot(
                CharacterUI.Instance.DraggedEquipmentSlotType,
                targetSlotIndex);

            CharacterUI.Instance.CompleteExternalDrop(moved);

            if (moved)
            {
                RefreshSlots();
            }
        }
    }

    public void CompleteExternalDrop(bool moved)
    {
        if (!isDragging)
        {
            return;
        }

        dropHandledThisDrag = true;

        RestoreDraggedSlotVisual();

        if (moved)
        {
            RefreshSlots();
        }

        ClearDragState();
    }

    private void ToggleInventory()
    {
        isOpen = !isOpen;

        if (inventoryWindow != null)
        {
            inventoryWindow.SetActive(isOpen);
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

            if (isDragging)
            {
                RestoreDraggedSlotVisual();
                ClearDragState();
            }
        }

        ApplyCursorState();
        RefreshAll();
    }

    private void ApplyCursorState()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void RefreshAll()
    {
        RefreshGold();
        RefreshSlots();
    }

    private void RefreshGold()
    {
        if (goldText == null || PlayerInventory.Instance == null)
        {
            return;
        }

        goldText.text = $"Gold: {PlayerInventory.Instance.Gold}";
    }

    private void RefreshSlots()
    {
        if (PlayerInventory.Instance == null)
        {
            return;
        }

        for (int i = 0; i < slotUIs.Count; i++)
        {
            InventorySlotData slotData = PlayerInventory.Instance.GetSlot(i);
            slotUIs[i].Refresh(slotData);
        }
    }

    private void HandleGoldChanged(int newGold)
    {
        RefreshGold();
    }

    private void HandleInventoryChanged()
    {
        RefreshSlots();
    }

    private InventorySlotUI GetSlotUI(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slotUIs.Count)
        {
            return null;
        }

        return slotUIs[slotIndex];
    }

    private void UpdateDraggedSlotPosition(Vector2 screenPosition)
    {
        if (!isDragging || draggedSlotUI == null || draggedSlotUI.RectTransform == null)
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

    private void CreateDragPlaceholder()
    {
        if (slotPrefab == null || draggedOriginalParent == null)
        {
            return;
        }

        DestroyDragPlaceholder();

        dragPlaceholderSlotUI = Instantiate(slotPrefab, draggedOriginalParent);
        dragPlaceholderSlotUI.transform.SetSiblingIndex(draggedOriginalSiblingIndex);
        dragPlaceholderSlotUI.gameObject.name = "InventoryDragPlaceholder";

        dragPlaceholderSlotUI.Refresh(null);
        dragPlaceholderSlotUI.SetDraggingVisualState(false);

        CanvasGroup placeholderCanvasGroup = dragPlaceholderSlotUI.GetComponent<CanvasGroup>();
        if (placeholderCanvasGroup != null)
        {
            placeholderCanvasGroup.blocksRaycasts = false;
            placeholderCanvasGroup.interactable = false;
        }

        dragPlaceholderSlotUI.enabled = false;
    }

    private void RestoreDraggedSlotVisual()
    {
        if (draggedSlotUI == null)
        {
            DestroyDragPlaceholder();
            return;
        }

        draggedSlotUI.transform.SetParent(draggedOriginalParent, false);

        if (dragPlaceholderSlotUI != null)
        {
            int placeholderSiblingIndex = dragPlaceholderSlotUI.transform.GetSiblingIndex();
            draggedSlotUI.transform.SetSiblingIndex(placeholderSiblingIndex);
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
        if (dragPlaceholderSlotUI != null)
        {
            Destroy(dragPlaceholderSlotUI.gameObject);
            dragPlaceholderSlotUI = null;
        }
    }

    private void ClearDragState()
    {
        isDragging = false;
        dropHandledThisDrag = false;
        dragSourceSlotIndex = -1;
        draggedSlotUI = null;
        draggedOriginalParent = null;
        draggedOriginalSiblingIndex = -1;
    }
}