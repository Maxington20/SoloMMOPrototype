using TMPro;
using UnityEngine;

public class AbilityTooltipUI : MonoBehaviour
{
    public static AbilityTooltipUI Instance { get; private set; }

    [Header("Root")]
    [SerializeField] private GameObject root;

    [Header("Text")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text detailsText;
    [SerializeField] private TMP_Text descriptionText;

    [Header("Positioning")]
    [SerializeField] private Vector2 offset = new Vector2(12f, -12f);
    [SerializeField] private Vector2 screenPadding = new Vector2(16f, 16f);

    private Canvas rootCanvas;
    private RectTransform rectTransform;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        rootCanvas = GetComponentInParent<Canvas>();
        rectTransform = GetComponent<RectTransform>();

        Hide();
    }

    private void Update()
    {
        if (root != null && root.activeSelf)
        {
            UpdatePosition();
        }
    }

    public void Show(AbilityData ability)
    {
        if (ability == null)
        {
            return;
        }

        if (root != null)
        {
            root.SetActive(true);
        }

        BuildText(ability);
        UpdatePosition();
    }

    public void Hide()
    {
        if (root != null)
        {
            root.SetActive(false);
        }
    }

    private void BuildText(AbilityData ability)
    {
        if (nameText != null)
        {
            nameText.text = ability.DisplayName;
        }

        if (detailsText != null)
        {
            detailsText.text = BuildDetailsText(ability);
        }

        if (descriptionText != null)
        {
            descriptionText.text = ability.Description;
        }
    }

    private string BuildDetailsText(AbilityData ability)
    {
        string effectLine = string.Empty;

        if (ability.DamageAmount > 0)
        {
            effectLine = AppendInline(effectLine, $"Damage: {ability.DamageAmount}");
        }

        if (ability.HealthRestoreAmount > 0)
        {
            effectLine = AppendInline(effectLine, $"Heal: {ability.HealthRestoreAmount}");
        }

        string costLine = $"Mana: {ability.ManaCost}";
        string cooldownLine = $"Cooldown: {ability.CooldownSeconds:0.#}s";
        string rangeLine = ability.RequiresTarget ? $"Range: {ability.Range:0.#}" : "Target: Self";
        string castLine = BuildCastLine(ability);

        string result = string.Empty;

        if (!string.IsNullOrWhiteSpace(effectLine))
        {
            result = AppendLine(result, effectLine);
        }

        result = AppendLine(result, costLine);
        result = AppendLine(result, cooldownLine);
        result = AppendLine(result, rangeLine);
        result = AppendLine(result, castLine);

        if (ability.CanBeInterrupted && !ability.IsInstant)
        {
            result = AppendLine(result, "Interruptible");
        }

        if (ability.CanMoveWhileCasting && !ability.IsInstant)
        {
            result = AppendLine(result, "Can move while casting");
        }

        return result;
    }

    private string BuildCastLine(AbilityData ability)
    {
        return ability.CastType switch
        {
            AbilityCastType.CastTime => $"Cast: {ability.CastTimeSeconds:0.#}s",
            AbilityCastType.Channel => $"Channel: {ability.ChannelDurationSeconds:0.#}s",
            _ => "Instant"
        };
    }

    private string AppendInline(string currentText, string value)
    {
        if (string.IsNullOrWhiteSpace(currentText))
        {
            return value;
        }

        return currentText + " | " + value;
    }

    private string AppendLine(string currentText, string line)
    {
        if (string.IsNullOrWhiteSpace(currentText))
        {
            return line;
        }

        return currentText + "\n" + line;
    }

    private void UpdatePosition()
    {
        if (rectTransform == null)
        {
            return;
        }

        Canvas.ForceUpdateCanvases();

        Vector2 desiredScreenPosition = (Vector2)Input.mousePosition + offset;
        Vector2 panelSize = rectTransform.rect.size;

        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        Vector2 finalPosition = desiredScreenPosition;

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

        if (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            RectTransform canvasRect = rootCanvas.transform as RectTransform;

            if (canvasRect != null &&
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    finalPosition,
                    rootCanvas.worldCamera,
                    out Vector2 localPoint))
            {
                rectTransform.localPosition = localPoint;
                return;
            }
        }

        rectTransform.position = finalPosition;
    }
}