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

        if (headSlot != null) headSlot.Initialize(HandleEquipmentSlotClicked);
        if (chestSlot != null) chestSlot.Initialize(HandleEquipmentSlotClicked);
        if (legsSlot != null) legsSlot.Initialize(HandleEquipmentSlotClicked);
        if (feetSlot != null) feetSlot.Initialize(HandleEquipmentSlotClicked);
        if (weaponSlot != null) weaponSlot.Initialize(HandleEquipmentSlotClicked);
        if (offhandSlot != null) offhandSlot.Initialize(HandleEquipmentSlotClicked);
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

    private void HandleEquipmentSlotClicked(EquipmentSlotType slotType)
    {
        if (inventory == null)
        {
            return;
        }

        if (inventory.TryUnequip(slotType))
        {
            Refresh();
        }
    }
}