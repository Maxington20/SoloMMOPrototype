using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemContextMenuUI : MonoBehaviour
{
    public static ItemContextMenuUI Instance { get; private set; }

    private enum ContextType
    {
        None,
        InventorySlot,
        EquipmentSlot,
        VendorBuyItem,
        VendorSellInventorySlot
    }

    [Header("Root")]
    [SerializeField] private GameObject menuRoot;
    [SerializeField] private RectTransform menuPanel;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Buttons")]
    [SerializeField] private Button primaryActionButton;
    [SerializeField] private TMP_Text primaryActionText;
    [SerializeField] private Button detailsButton;

    [Header("Settings")]
    [SerializeField] private Vector2 menuOffset = new Vector2(12f, -12f);
    [SerializeField] private Vector2 detailsOffset = new Vector2(210f, 0f);

    private Canvas rootCanvas;

    private ContextType currentContextType = ContextType.None;
    private int currentInventorySlotIndex = -1;
    private int currentVendorItemIndex = -1;
    private EquipmentSlotType currentEquipmentSlotType = EquipmentSlotType.None;
    private ItemData currentItem;
    private Vector2 currentOpenScreenPosition;
    private int openedFrame = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        rootCanvas = GetComponentInParent<Canvas>();

        if (primaryActionButton != null)
        {
            primaryActionButton.onClick.AddListener(HandlePrimaryActionClicked);
        }

        if (detailsButton != null)
        {
            detailsButton.onClick.AddListener(HandleDetailsClicked);
        }

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        Hide();
    }

    private void Update()
    {
        if (menuRoot == null || !menuRoot.activeSelf)
        {
            return;
        }

        if (Time.frameCount == openedFrame)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Hide();
            return;
        }

        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            if (!IsPointerOverMenu())
            {
                Hide();
            }
        }
    }

    public void OpenForInventorySlot(int slotIndex, ItemData item, Vector2 screenPosition)
    {
        if (item == null)
        {
            Hide();
            return;
        }

        currentContextType = ContextType.InventorySlot;
        currentInventorySlotIndex = slotIndex;
        currentVendorItemIndex = -1;
        currentEquipmentSlotType = EquipmentSlotType.None;
        currentItem = item;
        currentOpenScreenPosition = screenPosition;

        if (item.IsUsable)
        {
            ConfigurePrimaryAction(true, "Use");
        }
        else
        {
            bool canEquip = item.IsEquippable && item.EquipmentSlot != EquipmentSlotType.None;
            ConfigurePrimaryAction(canEquip, "Equip");
        }

        ShowMenu(screenPosition);
    }

    public void OpenForEquipmentSlot(EquipmentSlotType slotType, ItemData item, Vector2 screenPosition)
    {
        if (item == null)
        {
            Hide();
            return;
        }

        currentContextType = ContextType.EquipmentSlot;
        currentInventorySlotIndex = -1;
        currentVendorItemIndex = -1;
        currentEquipmentSlotType = slotType;
        currentItem = item;
        currentOpenScreenPosition = screenPosition;

        ConfigurePrimaryAction(true, "Unequip");
        ShowMenu(screenPosition);
    }

    public void OpenForVendorBuyItem(int vendorItemIndex, ItemData item, Vector2 screenPosition)
    {
        if (item == null)
        {
            Hide();
            return;
        }

        currentContextType = ContextType.VendorBuyItem;
        currentInventorySlotIndex = -1;
        currentVendorItemIndex = vendorItemIndex;
        currentEquipmentSlotType = EquipmentSlotType.None;
        currentItem = item;
        currentOpenScreenPosition = screenPosition;

        ConfigurePrimaryAction(true, "Buy");
        ShowMenu(screenPosition);
    }

    public void OpenForVendorSellInventorySlot(int inventorySlotIndex, ItemData item, Vector2 screenPosition)
    {
        if (item == null)
        {
            Hide();
            return;
        }

        currentContextType = ContextType.VendorSellInventorySlot;
        currentInventorySlotIndex = inventorySlotIndex;
        currentVendorItemIndex = -1;
        currentEquipmentSlotType = EquipmentSlotType.None;
        currentItem = item;
        currentOpenScreenPosition = screenPosition;

        bool canSell = item.SellValue > 0;
        ConfigurePrimaryAction(canSell, "Sell");
        ShowMenu(screenPosition);
    }

    public void Hide()
    {
        currentContextType = ContextType.None;
        currentInventorySlotIndex = -1;
        currentVendorItemIndex = -1;
        currentEquipmentSlotType = EquipmentSlotType.None;
        currentItem = null;

        if (menuRoot != null)
        {
            menuRoot.SetActive(false);
        }
    }

    private void ConfigurePrimaryAction(bool isVisible, string buttonText)
    {
        if (primaryActionButton != null)
        {
            primaryActionButton.gameObject.SetActive(isVisible);
        }

        if (primaryActionText != null)
        {
            primaryActionText.text = buttonText;
        }
    }

    private void ShowMenu(Vector2 screenPosition)
    {
        if (menuRoot == null)
        {
            return;
        }

        menuRoot.SetActive(true);
        openedFrame = Time.frameCount;

        transform.SetAsLastSibling();
        PositionPanel(menuPanel, screenPosition + menuOffset);
    }

    private void HandlePrimaryActionClicked()
    {
        bool changedSomething = false;

        switch (currentContextType)
        {
            case ContextType.InventorySlot:
                if (PlayerInventory.Instance != null)
                {
                    if (currentItem != null && currentItem.IsUsable)
                    {
                        changedSomething = PlayerInventory.Instance.TryUseItemFromSlot(currentInventorySlotIndex);
                    }
                    else
                    {
                        changedSomething = PlayerInventory.Instance.TryEquipFromSlot(currentInventorySlotIndex);
                    }
                }
                break;

            case ContextType.EquipmentSlot:
                if (PlayerInventory.Instance != null)
                {
                    changedSomething = PlayerInventory.Instance.TryUnequip(currentEquipmentSlotType);
                }
                break;

            case ContextType.VendorBuyItem:
                if (VendorUI.Instance != null)
                {
                    changedSomething = VendorUI.Instance.TryBuyVendorItem(currentVendorItemIndex);
                }
                break;

            case ContextType.VendorSellInventorySlot:
                if (VendorUI.Instance != null)
                {
                    changedSomething = VendorUI.Instance.TrySellInventorySlot(currentInventorySlotIndex);
                }
                break;
        }

        if (changedSomething && ItemTooltipUI.Instance != null)
        {
            ItemTooltipUI.Instance.Hide();
        }

        Hide();
    }

    private void HandleDetailsClicked()
    {
        if (currentItem == null)
        {
            Hide();
            return;
        }

        if (ItemTooltipUI.Instance != null)
        {
            Vector2 detailsPosition = currentOpenScreenPosition + detailsOffset;
            ItemTooltipUI.Instance.ShowAtScreenPosition(currentItem, detailsPosition);
        }

        Hide();
    }

    private bool IsPointerOverMenu()
    {
        if (menuPanel == null || !menuPanel.gameObject.activeInHierarchy)
        {
            return false;
        }

        Camera eventCamera = null;
        if (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            eventCamera = rootCanvas.worldCamera;
        }

        return RectTransformUtility.RectangleContainsScreenPoint(menuPanel, Input.mousePosition, eventCamera);
    }

    private void PositionPanel(RectTransform targetPanel, Vector2 screenPosition)
    {
        if (targetPanel == null)
        {
            return;
        }

        if (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            RectTransform canvasRect = rootCanvas.transform as RectTransform;
            if (canvasRect != null &&
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    screenPosition,
                    rootCanvas.worldCamera,
                    out Vector2 localPoint))
            {
                targetPanel.localPosition = localPoint;
                return;
            }
        }

        targetPanel.position = screenPosition;
    }
}