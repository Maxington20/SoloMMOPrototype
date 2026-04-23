using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LootWindowUI : MonoBehaviour
{
    public static LootWindowUI Instance { get; private set; }

    [Header("Window")]
    [SerializeField] private GameObject lootWindow;
    [SerializeField] private TMP_Text corpseNameText;
    [SerializeField] private Button closeButton;

    [Header("Gold")]
    [SerializeField] private Button goldButton;
    [SerializeField] private TMP_Text goldText;

    [Header("Item Grid")]
    [SerializeField] private Transform lootSlotContainer;
    [SerializeField] private InventorySlotUI lootSlotPrefab;

    [Header("Controls")]
    [SerializeField] private KeyCode closeKey = KeyCode.Escape;
    [SerializeField] private int minimumVisibleSlotCount = 6;

    private readonly List<InventorySlotUI> lootSlotUIs = new List<InventorySlotUI>();

    private EnemyLoot currentLootSource;
    private bool isOpen;

    public bool IsOpen => isOpen;
    public EnemyLoot CurrentLootSource => currentLootSource;

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
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Close);
        }

        if (goldButton != null)
        {
            goldButton.onClick.AddListener(HandleGoldClicked);
        }

        if (lootWindow != null)
        {
            lootWindow.SetActive(false);
        }

        BuildLootSlots(minimumVisibleSlotCount);
    }

    private void OnDestroy()
    {
        UnsubscribeFromCurrentLoot();
    }

    private void Update()
    {
        if (!isOpen)
        {
            return;
        }

        if (Input.GetKeyDown(closeKey))
        {
            Close();
        }
    }

    public void OpenLoot(EnemyLoot lootSource)
    {
        if (lootSource == null || !lootSource.CanBeLooted)
        {
            return;
        }

        if (currentLootSource != lootSource)
        {
            UnsubscribeFromCurrentLoot();
            currentLootSource = lootSource;
            currentLootSource.OnLootChanged += HandleLootChanged;
        }

        int slotCount = Mathf.Max(minimumVisibleSlotCount, currentLootSource.SlotCount);
        if (lootSlotUIs.Count != slotCount)
        {
            BuildLootSlots(slotCount);
        }

        isOpen = true;

        if (lootWindow != null)
        {
            lootWindow.SetActive(true);
        }

        RefreshAll();
    }

    public void Close()
    {
        isOpen = false;

        if (lootWindow != null)
        {
            lootWindow.SetActive(false);
        }

        if (ItemContextMenuUI.Instance != null)
        {
            ItemContextMenuUI.Instance.Hide();
        }

        if (ItemTooltipUI.Instance != null)
        {
            ItemTooltipUI.Instance.Hide();
        }

        UnsubscribeFromCurrentLoot();
        currentLootSource = null;
    }

    private void BuildLootSlots(int slotCount)
    {
        if (lootSlotContainer == null || lootSlotPrefab == null)
        {
            return;
        }

        for (int i = lootSlotContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(lootSlotContainer.GetChild(i).gameObject);
        }

        lootSlotUIs.Clear();

        int finalSlotCount = Mathf.Max(1, slotCount);

        for (int i = 0; i < finalSlotCount; i++)
        {
            InventorySlotUI slotUI = Instantiate(lootSlotPrefab, lootSlotContainer);
            slotUI.Initialize(i, HandleLootSlotLeftClicked);
            lootSlotUIs.Add(slotUI);
        }
    }

    private void RefreshAll()
    {
        if (currentLootSource == null)
        {
            Close();
            return;
        }

        if (!currentLootSource.CanBeLooted)
        {
            Close();
            return;
        }

        RefreshHeader();
        RefreshGold();
        RefreshSlots();

        if (!HasAnyRemainingLoot())
        {
            Close();
        }
    }

    private void RefreshHeader()
    {
        if (corpseNameText == null || currentLootSource == null)
        {
            return;
        }

        DisplayName displayName = currentLootSource.GetComponent<DisplayName>();
        corpseNameText.text = displayName != null
            ? displayName.Display
            : currentLootSource.gameObject.name;
    }

    private void RefreshGold()
    {
        if (goldText != null)
        {
            goldText.text = currentLootSource != null && currentLootSource.GoldAmount > 0
                ? $"{currentLootSource.GoldAmount} Gold"
                : "No Gold";
        }

        if (goldButton != null)
        {
            goldButton.interactable = currentLootSource != null && currentLootSource.GoldAmount > 0;
        }
    }

    private void RefreshSlots()
    {
        if (currentLootSource == null)
        {
            return;
        }

        IReadOnlyList<InventorySlotData> lootSlots = currentLootSource.LootSlots;

        for (int i = 0; i < lootSlotUIs.Count; i++)
        {
            InventorySlotData slotData = i < lootSlots.Count ? lootSlots[i] : null;
            lootSlotUIs[i].Refresh(slotData);
        }
    }

    private void HandleGoldClicked()
    {
        if (currentLootSource == null || PlayerInventory.Instance == null)
        {
            return;
        }

        bool looted = currentLootSource.TryLootGold(PlayerInventory.Instance);
        if (looted)
        {
            RefreshAll();
        }
    }

    private void HandleLootSlotLeftClicked(int slotIndex)
    {
        if (currentLootSource == null || PlayerInventory.Instance == null)
        {
            return;
        }

        bool looted = currentLootSource.TryLootItem(slotIndex, PlayerInventory.Instance);
        if (looted)
        {
            RefreshAll();
        }
    }

    private void HandleLootChanged()
    {
        RefreshAll();
    }

    private bool HasAnyRemainingLoot()
    {
        if (currentLootSource == null)
        {
            return false;
        }

        if (currentLootSource.GoldAmount > 0)
        {
            return true;
        }

        IReadOnlyList<InventorySlotData> lootSlots = currentLootSource.LootSlots;
        for (int i = 0; i < lootSlots.Count; i++)
        {
            InventorySlotData slot = lootSlots[i];
            if (slot != null && !slot.IsEmpty && slot.Item != null)
            {
                return true;
            }
        }

        return false;
    }

    private void UnsubscribeFromCurrentLoot()
    {
        if (currentLootSource != null)
        {
            currentLootSource.OnLootChanged -= HandleLootChanged;
        }
    }
}