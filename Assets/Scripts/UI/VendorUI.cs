using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VendorUI : MonoBehaviour
{
    public static VendorUI Instance { get; private set; }

    [Header("Window")]
    [SerializeField] private GameObject vendorWindow;
    [SerializeField] private TMP_Text vendorNameText;
    [SerializeField] private TMP_Text goldText;

    [Header("Vendor Items")]
    [SerializeField] private Transform vendorSlotContainer;
    [SerializeField] private VendorSlotUI vendorSlotPrefab;

    [Header("Player Inventory (Sell View)")]
    [SerializeField] private Transform playerInventorySlotContainer;
    [SerializeField] private InventorySlotUI inventorySlotPrefab;

    [Header("Controls")]
    [SerializeField] private KeyCode closeKey = KeyCode.Escape;

    private readonly List<VendorSlotUI> vendorSlotUIs = new List<VendorSlotUI>();
    private readonly List<InventorySlotUI> playerSlotUIs = new List<InventorySlotUI>();

    private VendorNPC currentVendor;
    private bool isOpen;

    public bool IsOpen => isOpen;

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
        if (vendorWindow != null)
        {
            vendorWindow.SetActive(false);
        }

        BuildVendorSlots();
        BuildPlayerInventorySlots();
        RefreshAll();

        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.OnGoldChanged += HandleGoldChanged;
            PlayerInventory.Instance.OnInventoryChanged += HandleInventoryChanged;
        }
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
        if (!isOpen)
        {
            return;
        }

        if (Input.GetKeyDown(closeKey))
        {
            CloseVendor();
        }
    }

    public void OpenVendor(VendorNPC vendor)
    {
        if (vendor == null)
        {
            return;
        }

        currentVendor = vendor;
        isOpen = true;

        if (vendorWindow != null)
        {
            vendorWindow.SetActive(true);
        }

        RefreshAll();
    }

    public void CloseVendor()
    {
        isOpen = false;
        currentVendor = null;

        if (vendorWindow != null)
        {
            vendorWindow.SetActive(false);
        }

        if (ItemContextMenuUI.Instance != null)
        {
            ItemContextMenuUI.Instance.Hide();
        }

        if (ItemTooltipUI.Instance != null)
        {
            ItemTooltipUI.Instance.Hide();
        }
    }

    public bool IsOpenFor(VendorNPC vendor)
    {
        return isOpen && currentVendor == vendor;
    }

    public bool TryBuyVendorItem(int slotIndex)
    {
        if (!isOpen || currentVendor == null || PlayerInventory.Instance == null)
        {
            return false;
        }

        IReadOnlyList<ItemData> items = currentVendor.ItemsForSale;
        if (slotIndex < 0 || slotIndex >= items.Count)
        {
            return false;
        }

        ItemData item = items[slotIndex];
        if (item == null)
        {
            return false;
        }

        bool result = PlayerInventory.Instance.TryBuyItem(item, 1);

        if (result)
        {
            RefreshAll();
        }

        return result;
    }

    public bool TrySellInventorySlot(int slotIndex)
    {
        if (!isOpen || PlayerInventory.Instance == null)
        {
            return false;
        }

        bool result = PlayerInventory.Instance.TrySellItemFromSlot(slotIndex, 1);

        if (result)
        {
            RefreshAll();
        }

        return result;
    }

    private void BuildVendorSlots()
    {
        if (vendorSlotContainer == null || vendorSlotPrefab == null)
        {
            return;
        }

        for (int i = vendorSlotContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(vendorSlotContainer.GetChild(i).gameObject);
        }

        vendorSlotUIs.Clear();

        int slotCount = 20;

        for (int i = 0; i < slotCount; i++)
        {
            VendorSlotUI slotUI = Instantiate(vendorSlotPrefab, vendorSlotContainer);
            slotUI.Initialize(i, HandleVendorSlotLeftClicked, HandleVendorSlotRightClicked);
            vendorSlotUIs.Add(slotUI);
        }
    }

    private void BuildPlayerInventorySlots()
    {
        if (playerInventorySlotContainer == null || inventorySlotPrefab == null || PlayerInventory.Instance == null)
        {
            return;
        }

        for (int i = playerInventorySlotContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(playerInventorySlotContainer.GetChild(i).gameObject);
        }

        playerSlotUIs.Clear();

        for (int i = 0; i < PlayerInventory.Instance.SlotCount; i++)
        {
            InventorySlotUI slotUI = Instantiate(inventorySlotPrefab, playerInventorySlotContainer);
            slotUI.Initialize(i, HandlePlayerInventorySlotLeftClicked, HandlePlayerInventorySlotRightClicked);
            playerSlotUIs.Add(slotUI);
        }
    }

    private void HandleVendorSlotLeftClicked(int slotIndex)
    {
        // Intentionally empty.
        // Vendor items now use right-click context menu instead of immediate buy on left click.
    }

    private void HandleVendorSlotRightClicked(int slotIndex, Vector2 screenPosition)
    {
        if (!isOpen || currentVendor == null || ItemContextMenuUI.Instance == null)
        {
            return;
        }

        IReadOnlyList<ItemData> items = currentVendor.ItemsForSale;
        if (slotIndex < 0 || slotIndex >= items.Count)
        {
            ItemContextMenuUI.Instance.Hide();
            return;
        }

        ItemData item = items[slotIndex];
        if (item == null)
        {
            ItemContextMenuUI.Instance.Hide();
            return;
        }

        if (ItemTooltipUI.Instance != null)
        {
            ItemTooltipUI.Instance.Hide();
        }

        ItemContextMenuUI.Instance.OpenForVendorBuyItem(slotIndex, item, screenPosition);
    }

    private void HandlePlayerInventorySlotLeftClicked(int slotIndex)
    {
        // Intentionally empty.
        // Vendor sell-side inventory now uses right-click context menu instead of immediate sell on left click.
    }

    private void HandlePlayerInventorySlotRightClicked(int slotIndex, Vector2 screenPosition)
    {
        if (!isOpen || PlayerInventory.Instance == null || ItemContextMenuUI.Instance == null)
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

        ItemContextMenuUI.Instance.OpenForVendorSellInventorySlot(slotIndex, slotData.Item, screenPosition);
    }

    private void HandleGoldChanged(int _)
    {
        if (isOpen)
        {
            RefreshGold();
        }
    }

    private void HandleInventoryChanged()
    {
        if (isOpen)
        {
            RefreshPlayerInventorySlots();
            RefreshGold();
        }
    }

    private void RefreshAll()
    {
        RefreshVendorName();
        RefreshGold();
        RefreshVendorSlots();
        RefreshPlayerInventorySlots();
    }

    private void RefreshVendorName()
    {
        if (vendorNameText == null)
        {
            return;
        }

        vendorNameText.text = currentVendor != null ? currentVendor.VendorName : "Vendor";
    }

    private void RefreshGold()
    {
        if (goldText == null || PlayerInventory.Instance == null)
        {
            return;
        }

        goldText.text = $"Gold: {PlayerInventory.Instance.Gold}";
    }

    private void RefreshVendorSlots()
    {
        for (int i = 0; i < vendorSlotUIs.Count; i++)
        {
            ItemData item = null;

            if (currentVendor != null && i < currentVendor.ItemsForSale.Count)
            {
                item = currentVendor.ItemsForSale[i];
            }

            vendorSlotUIs[i].Refresh(item);
        }
    }

    private void RefreshPlayerInventorySlots()
    {
        if (PlayerInventory.Instance == null)
        {
            return;
        }

        for (int i = 0; i < playerSlotUIs.Count; i++)
        {
            InventorySlotData slotData = PlayerInventory.Instance.GetSlot(i);
            playerSlotUIs[i].Refresh(slotData);
        }
    }
}