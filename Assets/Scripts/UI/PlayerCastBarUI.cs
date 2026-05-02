using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCastBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerAbilityController abilityController;

    [Header("Root")]
    [SerializeField] private GameObject visualRoot;

    [Header("UI")]
    [SerializeField] private Image fillImage;
    [SerializeField] private TMP_Text abilityNameText;
    [SerializeField] private TMP_Text timeText;

    private AbilityData currentAbility;
    private float currentDuration;
    private bool subscribed;

    private void Awake()
    {
        if (abilityController == null)
        {
            abilityController = FindFirstObjectByType<PlayerAbilityController>();
        }

        if (visualRoot == null)
        {
            visualRoot = gameObject;
        }

        SetVisible(false);
    }

    private void OnEnable()
    {
        Subscribe();
    }

    private void Start()
    {
        Subscribe();
        SetVisible(false);
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Update()
    {
        if (!subscribed)
        {
            Subscribe();
        }

        Refresh();
    }

    private void Subscribe()
    {
        if (subscribed || abilityController == null)
        {
            return;
        }

        abilityController.OnCastStarted += HandleCastStarted;
        abilityController.OnCastCompleted += HandleCastEnded;
        abilityController.OnCastCancelled += HandleCastEnded;

        subscribed = true;
    }

    private void Unsubscribe()
    {
        if (!subscribed || abilityController == null)
        {
            return;
        }

        abilityController.OnCastStarted -= HandleCastStarted;
        abilityController.OnCastCompleted -= HandleCastEnded;
        abilityController.OnCastCancelled -= HandleCastEnded;

        subscribed = false;
    }

    private void HandleCastStarted(AbilityData ability, float duration)
    {
        currentAbility = ability;
        currentDuration = Mathf.Max(0.01f, duration);

        if (abilityNameText != null)
        {
            abilityNameText.text = ability != null ? ability.DisplayName : "Casting";
        }

        SetVisible(true);
        Refresh();
    }

    private void HandleCastEnded(AbilityData ability)
    {
        currentAbility = null;
        currentDuration = 0f;
        SetVisible(false);
    }

    private void Refresh()
    {
        if (abilityController == null || !abilityController.IsCasting)
        {
            SetVisible(false);
            return;
        }

        if (currentAbility == null)
        {
            currentAbility = abilityController.CurrentCastingAbility;
        }

        if (currentDuration <= 0f && currentAbility != null)
        {
            currentDuration = currentAbility.CastType == AbilityCastType.Channel
                ? currentAbility.ChannelDurationSeconds
                : currentAbility.CastTimeSeconds;
        }

        if (abilityNameText != null && currentAbility != null)
        {
            abilityNameText.text = currentAbility.DisplayName;
        }

        float progress = abilityController.CastProgress;

        if (fillImage != null)
        {
            fillImage.fillAmount = progress;
        }

        if (timeText != null)
        {
            float duration = Mathf.Max(0.01f, currentDuration);
            float remaining = Mathf.Max(0f, duration * (1f - progress));
            timeText.text = $"{remaining:0.0}s";
        }

        SetVisible(true);
    }

    private void SetVisible(bool visible)
    {
        if (visualRoot != null)
        {
            visualRoot.SetActive(visible);
        }
    }
}