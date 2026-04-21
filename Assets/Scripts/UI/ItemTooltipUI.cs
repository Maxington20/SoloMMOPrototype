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

    [Header("Settings")]
    [SerializeField] private Vector2 cursorOffset = new Vector2(18f, -18f);

    private Canvas rootCanvas;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        rootCanvas = GetComponentInParent<Canvas>();

        DisableTooltipRaycasts();
        Hide();
    }

    private void Update()
    {
        if (tooltipRoot == null || !tooltipRoot.activeSelf || tooltipPanel == null)
        {
            return;
        }

        FollowMouse();
    }

    public void Show(ItemData item)
    {
        if (item == null)
        {
            Hide();
            return;
        }

        if (tooltipRoot != null)
        {
            tooltipRoot.SetActive(true);
        }

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        if (itemNameText != null)
        {
            itemNameText.text = item.DisplayName;
        }

        if (itemTypeText != null)
        {
            itemTypeText.text = BuildTypeLine(item);
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

        FollowMouse();
    }

    public void Hide()
    {
        if (tooltipRoot != null)
        {
            tooltipRoot.SetActive(false);
        }
    }

    private void DisableTooltipRaycasts()
    {
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        Graphic[] graphics = GetComponentsInChildren<Graphic>(true);
        for (int i = 0; i < graphics.Length; i++)
        {
            graphics[i].raycastTarget = false;
        }
    }

    private string BuildTypeLine(ItemData item)
    {
        if (item.IsEquippable)
        {
            return $"Equipment - {GetEquipmentSlotName(item.EquipmentSlot)}";
        }

        return item.ItemType.ToString();
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

    private void FollowMouse()
    {
        Vector2 screenPosition = (Vector2)Input.mousePosition + cursorOffset;

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