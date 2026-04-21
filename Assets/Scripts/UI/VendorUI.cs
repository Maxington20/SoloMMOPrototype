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

        if (ItemTooltipUI.Instance != null)
        {
            ItemTooltipUI.Instance.Hide();
        }
    }

    public bool IsOpenFor(VendorNPC vendor)
    {
        return isOpen && currentVendor == vendor;
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
            slotUI.Initialize(i, HandleVendorSlotClicked);
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
            slotUI.Initialize(i, HandlePlayerInventorySlotClicked);
            playerSlotUIs.Add(slotUI);
        }
    }

    private void HandleVendorSlotClicked(int slotIndex)
    {
        if (!isOpen || currentVendor == null || PlayerInventory.Instance == null)
        {
            return;
        }

        IReadOnlyList<ItemData> items = currentVendor.ItemsForSale;
        if (slotIndex < 0 || slotIndex >= items.Count)
        {
            return;
        }

        ItemData item = items[slotIndex];
        if (item == null)
        {
            return;
        }

        if (PlayerInventory.Instance.TryBuyItem(item, 1))
        {
            RefreshAll();
        }
    }

    private void HandlePlayerInventorySlotClicked(int slotIndex)
    {
        if (!isOpen || PlayerInventory.Instance == null)
        {
            return;
        }

        if (PlayerInventory.Instance.TrySellItemFromSlot(slotIndex, 1))
        {
            RefreshAll();
        }
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