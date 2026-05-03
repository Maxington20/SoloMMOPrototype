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
    private PlayerResource playerResource;
    private PlayerCombat playerCombat;
    private PlayerAbilityController abilityController;

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

        playerResource = FindFirstObjectByType<PlayerResource>();
        playerCombat = FindFirstObjectByType<PlayerCombat>();
        abilityController = FindFirstObjectByType<PlayerAbilityController>();

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
        string result = string.Empty;

        string effectText = BuildEffectText(ability);
        if (!string.IsNullOrWhiteSpace(effectText))
        {
            result = AppendLine(result, effectText);
        }

        string statusText = BuildStatusEffectsText(ability);
        if (!string.IsNullOrWhiteSpace(statusText))
        {
            result = AppendLine(result, string.Empty);
            result = AppendLine(result, statusText);
        }

        string resourceName = GetResourceName();

        result = AppendLine(result, string.Empty);

        string costLine = ability.ResourceCost > 0
            ? $"{resourceName} Cost: {ability.ResourceCost}"
            : $"{resourceName} Cost: 0";

        result = AppendLine(result, costLine);

        if (ability.ResourceGenerated > 0)
        {
            result = AppendLine(result, $"Generates: {ability.ResourceGenerated} {resourceName}");
        }

        result = AppendLine(result, $"Cooldown: {ability.CooldownSeconds:0.#}s");
        result = AppendLine(result, ability.RequiresTarget ? $"Range: {ability.Range:0.#}" : "Target: Self");
        result = AppendLine(result, BuildCastLine(ability));

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

    private string BuildEffectText(AbilityData ability)
    {
        string result = string.Empty;

        if (ability.DealsDamage)
        {
            int actualDamage = GetCurrentAbilityDamage(ability);
            int baseDamage = GetCurrentBaseDamage();

            result = AppendLine(
                result,
                $"Damage: {actualDamage}  <size=85%><color=#BDBDBD>({baseDamage} × {ability.DamageMultiplier:0.##})</color></size>");
        }

        if (ability.RestoresHealth)
        {
            int actualHealing = GetCurrentAbilityHealing(ability);
            float baseHealingPower = GetCurrentBaseHealingPower();

            result = AppendLine(
                result,
                $"Healing: {actualHealing}  <size=85%><color=#BDBDBD>({baseHealingPower:0.#} × {ability.HealingMultiplier:0.##})</color></size>");
        }

        return result;
    }

    private string BuildStatusEffectsText(AbilityData ability)
    {
        if (ability.StatusEffects == null || ability.StatusEffects.Length == 0)
        {
            return string.Empty;
        }

        string result = "<color=#FFD966>Status Effects</color>";

        for (int i = 0; i < ability.StatusEffects.Length; i++)
        {
            StatusEffectData effect = ability.StatusEffects[i];

            if (effect == null)
            {
                continue;
            }

            result = AppendLine(result, BuildSingleStatusEffectLine(effect));
        }

        return result;
    }

    private string BuildSingleStatusEffectLine(StatusEffectData effect)
    {
        return effect.EffectType switch
        {
            StatusEffectType.DamageOverTime =>
                $"{effect.DisplayName}: deals {effect.TickDamageMultiplier * 100f:0.#}% ability damage every {effect.TickIntervalSeconds:0.#}s for {effect.DurationSeconds:0.#}s",

            StatusEffectType.Stun =>
                $"{effect.DisplayName}: prevents movement and attacks for {effect.DurationSeconds:0.#}s",

            StatusEffectType.Slow =>
                $"{effect.DisplayName}: slows movement by {effect.SlowAmount * 100f:0.#}% for {effect.DurationSeconds:0.#}s",

            _ =>
                $"{effect.DisplayName}: lasts {effect.DurationSeconds:0.#}s"
        };
    }

    private int GetCurrentAbilityDamage(AbilityData ability)
    {
        if (playerCombat == null)
        {
            playerCombat = FindFirstObjectByType<PlayerCombat>();
        }

        if (playerCombat == null)
        {
            return 0;
        }

        return playerCombat.CalculateAbilityDamage(ability);
    }

    private int GetCurrentBaseDamage()
    {
        if (playerCombat == null)
        {
            playerCombat = FindFirstObjectByType<PlayerCombat>();
        }

        if (playerCombat == null)
        {
            return 0;
        }

        return playerCombat.Damage;
    }

    private int GetCurrentAbilityHealing(AbilityData ability)
    {
        if (abilityController == null)
        {
            abilityController = FindFirstObjectByType<PlayerAbilityController>();
        }

        if (abilityController == null)
        {
            return 0;
        }

        return abilityController.CalculateAbilityHealing(ability);
    }

    private float GetCurrentBaseHealingPower()
    {
        if (abilityController == null)
        {
            abilityController = FindFirstObjectByType<PlayerAbilityController>();
        }

        if (abilityController == null)
        {
            return 0f;
        }

        return abilityController.GetBaseHealingPower();
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

    private string GetResourceName()
    {
        if (playerResource == null)
        {
            playerResource = FindFirstObjectByType<PlayerResource>();
        }

        if (playerResource == null || !playerResource.HasResource)
        {
            return "Resource";
        }

        return playerResource.ResourceDisplayName;
    }

    private string AppendLine(string currentText, string line)
    {
        if (string.IsNullOrWhiteSpace(currentText))
        {
            return line;
        }

        if (string.IsNullOrWhiteSpace(line))
        {
            return currentText + "\n";
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