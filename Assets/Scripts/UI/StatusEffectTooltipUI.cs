using TMPro;
using UnityEngine;

public class StatusEffectTooltipUI : MonoBehaviour
{
    public static StatusEffectTooltipUI Instance { get; private set; }

    [Header("Root")]
    [SerializeField] private GameObject root;

    [Header("Text")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text detailsText;

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

    public void Show(StatusEffectUIInfo info)
    {
        if (info.Data == null)
        {
            return;
        }

        if (root != null)
        {
            root.SetActive(true);
        }

        if (nameText != null)
        {
            nameText.text = info.Data.DisplayName;
        }

        if (detailsText != null)
        {
            detailsText.text = BuildDetailsText(info);
        }

        UpdatePosition();
    }

    public void Hide()
    {
        if (root != null)
        {
            root.SetActive(false);
        }
    }

    private string BuildDetailsText(StatusEffectUIInfo info)
    {
        StatusEffectData effect = info.Data;

        string result = effect.EffectType switch
        {
            StatusEffectType.DamageOverTime =>
                $"Damage over time\nDeals {effect.TickDamageMultiplier * 100f:0.#}% ability damage every {effect.TickIntervalSeconds:0.#}s.",

            StatusEffectType.Stun =>
                "Stunned\nCannot move or attack.",

            StatusEffectType.Slow =>
                $"Slowed\nMovement speed reduced by {effect.SlowAmount * 100f:0.#}%.",

            _ =>
                "Status effect"
        };

        result += $"\n\nRemaining: {info.RemainingTime:0.0}s";
        result += $"\nDuration: {info.Duration:0.0}s";

        return result;
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