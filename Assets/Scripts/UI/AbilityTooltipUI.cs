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
            string damage = ability.DamageAmount > 0 ? $"Damage: {ability.DamageAmount}" : "";
            string heal = ability.HealthRestoreAmount > 0 ? $"Heal: {ability.HealthRestoreAmount}" : "";
            string mana = ability.ManaCost > 0 ? $"Mana: {ability.ManaCost}" : "Mana: 0";
            string cd = $"CD: {ability.CooldownSeconds:0.#}s";
            string range = ability.RequiresTarget ? $"Range: {ability.Range:0.#}" : "Self";

            string combined = $"{damage} {heal}".Trim();

            if (!string.IsNullOrEmpty(combined))
            {
                combined += "\n";
            }

            detailsText.text = $"{combined}{mana} | {cd} | {range}";
        }

        if (descriptionText != null)
        {
            descriptionText.text = ability.Description;
        }
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