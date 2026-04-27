using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemTooltipUI : MonoBehaviour
{
    public static ItemTooltipUI Instance { get; private set; }

    [Header("Root")]
    [SerializeField] private GameObject tooltipRoot;
    [SerializeField] private RectTransform tooltipPanel;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Text")]
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text itemTypeText;
    [SerializeField] private TMP_Text bonusText;
    [SerializeField] private TMP_Text sellValueText;

    [Header("Optional Close Button")]
    [SerializeField] private Button closeButton;

    [Header("Settings")]
    [SerializeField] private Vector2 cursorOffset = new Vector2(18f, -18f);
    [SerializeField] private Vector2 screenPadding = new Vector2(16f, 16f);

    private bool followMouse = true;
    private PlayerEquipment playerEquipment;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        playerEquipment = FindFirstObjectByType<PlayerEquipment>();

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Hide);
        }

        DisableTooltipRaycasts();
        Hide();
    }

    private void Update()
    {
        if (tooltipRoot == null || !tooltipRoot.activeSelf || tooltipPanel == null)
        {
            return;
        }

        if (followMouse)
        {
            Vector2 screenPosition = (Vector2)Input.mousePosition + cursorOffset;
            PositionTooltipClamped(screenPosition, false);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Hide();
        }
    }

    public void Show(ItemData item)
    {
        if (item == null)
        {
            Hide();
            return;
        }

        followMouse = true;
        Populate(item);

        if (tooltipRoot != null)
        {
            tooltipRoot.SetActive(true);
        }

        transform.SetAsLastSibling();

        Vector2 screenPosition = (Vector2)Input.mousePosition + cursorOffset;
        PositionTooltipClamped(screenPosition, false);
    }

    public void ShowAtScreenPosition(ItemData item, Vector2 screenPosition)
    {
        if (item == null)
        {
            Hide();
            return;
        }

        followMouse = false;
        Populate(item);

        if (tooltipRoot != null)
        {
            tooltipRoot.SetActive(true);
        }

        transform.SetAsLastSibling();

        PositionTooltipClamped(screenPosition, true);
    }

    public void Hide()
    {
        if (tooltipRoot != null)
        {
            tooltipRoot.SetActive(false);
        }
    }

    private void Populate(ItemData item)
    {
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        Color rarityColor = ItemRarityUtility.GetColor(item.Rarity);

        if (itemNameText != null)
        {
            itemNameText.text = item.DisplayName;
            itemNameText.color = rarityColor;
        }

        if (itemTypeText != null)
        {
            itemTypeText.text = BuildTypeLine(item);
            itemTypeText.color = rarityColor;
        }

        if (bonusText != null)
        {
            string tooltipBody = BuildBonusText(item);
            string equipWarning = BuildEquipWarningText(item);

            if (string.IsNullOrWhiteSpace(tooltipBody))
            {
                tooltipBody = "No bonuses";
            }

            if (!string.IsNullOrWhiteSpace(equipWarning))
            {
                tooltipBody += $"\n\n<color=#FF5555>{equipWarning}</color>";
            }

            bonusText.text = tooltipBody;
        }

        if (sellValueText != null)
        {
            sellValueText.text = $"Sell Value: {item.SellValue} Gold";
        }
    }

    private string BuildTypeLine(ItemData item)
    {
        string rarityName = ItemRarityUtility.GetDisplayName(item.Rarity);

        if (item.IsEquippable)
        {
            if (item.WeaponType != WeaponType.None)
            {
                return $"{rarityName} {FormatWeaponType(item.WeaponType)} - {GetEquipmentSlotName(item.EquipmentSlot)}";
            }

            return $"{rarityName} Equipment - {GetEquipmentSlotName(item.EquipmentSlot)}";
        }

        return $"{rarityName} {item.ItemType}";
    }

    private string BuildBonusText(ItemData item)
    {
        string result = string.Empty;

        result = AppendStatBonus(result, "Strength", item.StatBonuses.Strength);
        result = AppendStatBonus(result, "Agility", item.StatBonuses.Agility);
        result = AppendStatBonus(result, "Intellect", item.StatBonuses.Intellect);
        result = AppendStatBonus(result, "Stamina", item.StatBonuses.Stamina);
        result = AppendStatBonus(result, "Armour", item.StatBonuses.Armor);

        if (item.DamageBonus > 0)
        {
            result = AppendLine(result, $"+{item.DamageBonus} Damage");
        }

        if (item.HealthBonus > 0)
        {
            result = AppendLine(result, $"+{item.HealthBonus} Health");
        }

        if (item.IsWeapon)
        {
            if (!string.IsNullOrWhiteSpace(result))
            {
                result += "\n";
            }

            string rangeType = item.IsMeleeWeapon ? "Melee" : "Ranged";
            result = AppendLine(result, $"{rangeType} weapon");
            result = AppendLine(result, $"Hand: {FormatHandRequirement(item.HandRequirement)}");
            result = AppendLine(result, $"Attack Range: {item.WeaponAttackRange:0.#}");
        }

        if (item.IsUsable && item.HealthRestoreAmount > 0)
        {
            if (!string.IsNullOrWhiteSpace(result))
            {
                result += "\n";
            }

            result = AppendLine(result, $"Restores {item.HealthRestoreAmount} Health");
        }

        return result;
    }

    private string AppendStatBonus(string currentText, string statName, int amount)
    {
        if (amount == 0)
        {
            return currentText;
        }

        string sign = amount > 0 ? "+" : "";
        string color = amount > 0 ? "#55FF55" : "#FF5555";

        return AppendLine(currentText, $"<color={color}>{sign}{amount} {statName}</color>");
    }

    private string AppendLine(string currentText, string line)
    {
        if (string.IsNullOrWhiteSpace(currentText))
        {
            return line;
        }

        return currentText + "\n" + line;
    }

    private string BuildEquipWarningText(ItemData item)
    {
        if (item == null || !item.IsEquippable)
        {
            return string.Empty;
        }

        if (playerEquipment == null)
        {
            playerEquipment = FindFirstObjectByType<PlayerEquipment>();
        }

        if (playerEquipment == null)
        {
            return string.Empty;
        }

        EquipmentSlotType targetSlot = item.EquipmentSlot;
        string reason = playerEquipment.GetCannotEquipReason(item, targetSlot);

        if (string.IsNullOrWhiteSpace(reason))
        {
            return string.Empty;
        }

        return $"Cannot equip: {reason}";
    }

    private string FormatWeaponType(WeaponType weaponType)
    {
        return weaponType switch
        {
            WeaponType.OneHandedSword => "One-Handed Sword",
            WeaponType.TwoHandedSword => "Two-Handed Sword",
            WeaponType.OneHandedAxe => "One-Handed Axe",
            WeaponType.TwoHandedAxe => "Two-Handed Axe",
            WeaponType.OneHandedMace => "One-Handed Mace",
            WeaponType.TwoHandedMace => "Two-Handed Mace",
            WeaponType.Dagger => "Dagger",
            WeaponType.Staff => "Staff",
            WeaponType.Wand => "Wand",
            WeaponType.Bow => "Bow",
            WeaponType.Crossbow => "Crossbow",
            WeaponType.Shield => "Shield",
            WeaponType.Grimoire => "Grimoire",
            _ => "Unknown Weapon"
        };
    }

    private string FormatHandRequirement(WeaponHandRequirement handRequirement)
    {
        return handRequirement switch
        {
            WeaponHandRequirement.MainHandOnly => "Main Hand",
            WeaponHandRequirement.OffhandOnly => "Offhand",
            WeaponHandRequirement.OneHand => "One-Handed",
            WeaponHandRequirement.TwoHand => "Two-Handed",
            _ => "None"
        };
    }

    private string GetEquipmentSlotName(EquipmentSlotType slotType)
    {
        return slotType switch
        {
            EquipmentSlotType.Head => "Head",
            EquipmentSlotType.Chest => "Chest",
            EquipmentSlotType.Legs => "Legs",
            EquipmentSlotType.Feet => "Feet",
            EquipmentSlotType.Weapon => "Weapon",
            EquipmentSlotType.Offhand => "Offhand",
            _ => "Unknown"
        };
    }

    private void DisableTooltipRaycasts()
    {
        Graphic[] graphics = GetComponentsInChildren<Graphic>(true);

        for (int i = 0; i < graphics.Length; i++)
        {
            graphics[i].raycastTarget = false;
        }

        if (closeButton != null)
        {
            Image closeImage = closeButton.GetComponent<Image>();
            if (closeImage != null)
            {
                closeImage.raycastTarget = true;
            }

            TMP_Text closeText = closeButton.GetComponentInChildren<TMP_Text>(true);
            if (closeText != null)
            {
                closeText.raycastTarget = false;
            }
        }

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }
    }

    private void PositionTooltipClamped(Vector2 desiredScreenPosition, bool preferLeftWhenNeeded)
    {
        if (tooltipPanel == null)
        {
            return;
        }

        Canvas.ForceUpdateCanvases();

        Vector2 panelSize = tooltipPanel.rect.size;
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        Vector2 finalPosition = desiredScreenPosition;

        if (preferLeftWhenNeeded)
        {
            if (finalPosition.x + panelSize.x > screenWidth - screenPadding.x)
            {
                finalPosition.x -= panelSize.x;
            }
        }

        if (finalPosition.x + panelSize.x > screenWidth - screenPadding.x)
        {
            finalPosition.x = screenWidth - screenPadding.x - panelSize.x;
        }

        if (finalPosition.y - panelSize.y < screenPadding.y)
        {
            finalPosition.y = screenPadding.y + panelSize.y;
        }

        finalPosition.x = Mathf.Max(screenPadding.x, finalPosition.x);
        finalPosition.y = Mathf.Min(screenHeight - screenPadding.y, finalPosition.y);

        tooltipPanel.position = finalPosition;
    }
}