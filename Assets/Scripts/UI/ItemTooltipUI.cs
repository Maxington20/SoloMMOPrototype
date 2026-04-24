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

    private Canvas rootCanvas;
    private bool followMouse = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        rootCanvas = GetComponentInParent<Canvas>();

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
            PositionTooltipClamped(screenPosition, preferLeftWhenNeeded: false);
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
        PositionTooltipClamped(screenPosition, preferLeftWhenNeeded: false);
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
        PositionTooltipClamped(screenPosition, preferLeftWhenNeeded: true);
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
            string bonuses = BuildBonusText(item);
            bonusText.text = string.IsNullOrWhiteSpace(bonuses) ? "No bonuses" : bonuses;
        }

        if (sellValueText != null)
        {
            sellValueText.text = $"Sell Value: {item.SellValue} Gold";
        }
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

    private string BuildTypeLine(ItemData item)
    {
        string rarityName = ItemRarityUtility.GetDisplayName(item.Rarity);

        if (item.IsEquippable)
        {
            return $"{rarityName} Equipment - {GetEquipmentSlotName(item.EquipmentSlot)}";
        }

        return $"{rarityName} {item.ItemType}";
    }

    private string BuildBonusText(ItemData item)
    {
        string result = string.Empty;

        if (item.DamageBonus > 0)
        {
            result += $"+{item.DamageBonus} Damage";
        }

        if (item.HealthBonus > 0)
        {
            if (!string.IsNullOrEmpty(result))
            {
                result += "\n";
            }

            result += $"+{item.HealthBonus} Health";
        }

        if (item.IsUsable && item.HealthRestoreAmount > 0)
        {
            if (!string.IsNullOrEmpty(result))
            {
                result += "\n";
            }

            result += $"Restores {item.HealthRestoreAmount} Health";
        }

        return result;
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

            if (finalPosition.y - panelSize.y < screenPadding.y)
            {
                finalPosition.y += panelSize.y;
            }
        }

        finalPosition.x = Mathf.Clamp(
            finalPosition.x,
            screenPadding.x,
            screenWidth - panelSize.x - screenPadding.x);

        finalPosition.y = Mathf.Clamp(
            finalPosition.y,
            panelSize.y + screenPadding.y,
            screenHeight - screenPadding.y);

        SetPanelScreenPosition(finalPosition);
    }

    private void SetPanelScreenPosition(Vector2 screenPosition)
    {
        if (tooltipPanel == null)
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
                tooltipPanel.localPosition = localPoint;
                return;
            }
        }

        tooltipPanel.position = screenPosition;
    }
}