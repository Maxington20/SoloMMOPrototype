using TMPro;
using UnityEngine;

public class CharacterUI : MonoBehaviour
{
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

    private void Start()
    {
        if (characterWindow != null)
        {
            characterWindow.SetActive(false);
        }

        if (headSlot != null) headSlot.Initialize(HandleEquipmentSlotLeftClicked, HandleEquipmentSlotRightClicked);
        if (chestSlot != null) chestSlot.Initialize(HandleEquipmentSlotLeftClicked, HandleEquipmentSlotRightClicked);
        if (legsSlot != null) legsSlot.Initialize(HandleEquipmentSlotLeftClicked, HandleEquipmentSlotRightClicked);
        if (feetSlot != null) feetSlot.Initialize(HandleEquipmentSlotLeftClicked, HandleEquipmentSlotRightClicked);
        if (weaponSlot != null) weaponSlot.Initialize(HandleEquipmentSlotLeftClicked, HandleEquipmentSlotRightClicked);
        if (offhandSlot != null) offhandSlot.Initialize(HandleEquipmentSlotLeftClicked, HandleEquipmentSlotRightClicked);
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
        // Equipped items now use right-click context menu instead of auto-unequip on left click.
    }

    private void HandleEquipmentSlotRightClicked(EquipmentSlotType slotType, Vector2 screenPosition)
    {
        if (!isOpen || equipment == null || ItemContextMenuUI.Instance == null)
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
}