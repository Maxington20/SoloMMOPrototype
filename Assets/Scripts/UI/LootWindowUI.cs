using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LootWindowUI : MonoBehaviour
{
    public static LootWindowUI Instance { get; private set; }

    [Header("Window")]
    [SerializeField] private GameObject lootWindow;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Button closeButton;

    [Header("Gold")]
    [SerializeField] private GameObject goldRow;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private Button lootGoldButton;

    [Header("Items")]
    [SerializeField] private Transform itemContainer;
    [SerializeField] private InventorySlotUI itemSlotPrefab;

    private ILootContainer currentLoot;
    private PlayerInventory playerInventory;
    private PlayerAnimationController playerAnimationController;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            playerInventory = playerObject.GetComponent<PlayerInventory>();
            playerAnimationController = playerObject.GetComponent<PlayerAnimationController>();
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Close);
        }

        if (lootGoldButton != null)
        {
            lootGoldButton.onClick.AddListener(LootGold);
        }

        Close();
    }

    private void Update()
    {
        if (lootWindow != null && lootWindow.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
    }

    public void OpenLoot(ILootContainer lootContainer)
    {
        if (lootContainer == null || !lootContainer.CanBeLooted)
        {
            return;
        }

        if (currentLoot != null)
        {
            currentLoot.OnLootChanged -= Refresh;
        }

        currentLoot = lootContainer;
        currentLoot.OnLootChanged += Refresh;

        lootWindow.SetActive(true);

        if (playerAnimationController != null)
        {
            playerAnimationController.StartPickupHold();
        }

        Refresh();
    }

    public void Close()
    {
        if (currentLoot != null)
        {
            currentLoot.OnLootChanged -= Refresh;
        }

        currentLoot = null;

        lootWindow.SetActive(false);

        if (playerAnimationController != null)
        {
            playerAnimationController.FinishPickupHold();
        }
    }

    private void Refresh()
    {
        ClearItems();

        if (currentLoot == null)
        {
            return;
        }

        if (!currentLoot.CanBeLooted)
        {
            Close();
            return;
        }

        titleText.text = currentLoot.LootDisplayName;

        RefreshGold();
        RefreshItems();
    }

    private void RefreshGold()
    {
        bool hasGold = currentLoot != null && currentLoot.GoldAmount > 0;

        goldRow.SetActive(hasGold);

        goldText.text = hasGold
            ? $"{currentLoot.GoldAmount} Gold"
            : "No Gold";

        lootGoldButton.interactable = hasGold;
    }

    private void RefreshItems()
    {
        if (currentLoot == null || currentLoot.LootSlots == null || itemSlotPrefab == null)
        {
            return;
        }

        for (int i = 0; i < currentLoot.LootSlots.Count; i++)
        {
            InventorySlotData slot = currentLoot.LootSlots[i];

            if (slot == null || slot.IsEmpty)
            {
                continue;
            }

            int capturedIndex = i;

            InventorySlotUI slotUI = Instantiate(itemSlotPrefab, itemContainer);
            slotUI.gameObject.SetActive(true);

            slotUI.Initialize(capturedIndex, OnItemClicked);
            slotUI.Refresh(slot);
        }
    }

    private void OnItemClicked(int slotIndex)
    {
        if (currentLoot == null || playerInventory == null)
        {
            return;
        }

        currentLoot.TryLootItem(slotIndex, playerInventory);
    }

    private void LootGold()
    {
        if (currentLoot == null || playerInventory == null)
        {
            return;
        }

        currentLoot.TryLootGold(playerInventory);
    }

    private void ClearItems()
    {
        for (int i = itemContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(itemContainer.GetChild(i).gameObject);
        }
    }
}