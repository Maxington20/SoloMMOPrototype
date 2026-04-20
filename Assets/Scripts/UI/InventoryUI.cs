using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
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

    public bool IsOpen => isOpen;

    private void Start()
    {
        isOpen = false;

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
            slotUI.Initialize(i, HandleSlotClicked);
            slotUIs.Add(slotUI);
        }
    }

    private void HandleSlotClicked(int slotIndex)
    {
        if (PlayerInventory.Instance == null)
        {
            return;
        }

        if (PlayerInventory.Instance.TryEquipFromSlot(slotIndex))
        {
            RefreshAll();
        }
    }

    private void ToggleInventory()
    {
        isOpen = !isOpen;

        if (inventoryWindow != null)
        {
            inventoryWindow.SetActive(isOpen);
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
}