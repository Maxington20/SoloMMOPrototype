using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TargetFrameUI : MonoBehaviour
{
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image healthFill;
    [SerializeField] private GameObject visualRoot;

    [Header("Status Effects")]
    [SerializeField] private Transform statusEffectContainer;
    [SerializeField] private StatusEffectIconUI statusEffectIconPrefab;

    private readonly List<StatusEffectIconUI> statusEffectIcons = new List<StatusEffectIconUI>();

    private Health currentTargetHealth;
    private DisplayName currentDisplayName;
    private StatusEffectController currentStatusEffectController;
    private Transform lastTarget;

    private void Awake()
    {
        SetVisible(false);
    }

    private void OnDisable()
    {
        UnsubscribeFromStatusEffects();
    }

    private void Update()
    {
        if (playerCombat == null)
        {
            ClearTargetFrame();
            return;
        }

        Transform target = playerCombat.CurrentTargetTransform;

        if (target == null)
        {
            ClearTargetFrame();
            return;
        }

        SetVisible(true);

        if (target != lastTarget)
        {
            SetTarget(target);
        }

        if (currentTargetHealth != null && healthFill != null)
        {
            healthFill.fillAmount = currentTargetHealth.HealthPercent;

            if (currentTargetHealth.IsDead)
            {
                ClearTargetFrame();
                return;
            }
        }

        RefreshStatusEffects();
    }

    private void SetTarget(Transform target)
    {
        UnsubscribeFromStatusEffects();

        lastTarget = target;
        currentTargetHealth = target.GetComponent<Health>();
        currentDisplayName = target.GetComponent<DisplayName>();
        currentStatusEffectController = target.GetComponent<StatusEffectController>();

        if (currentStatusEffectController != null)
        {
            currentStatusEffectController.OnStatusEffectsChanged += RefreshStatusEffects;
        }

        RefreshName(target);
        RefreshStatusEffects();
    }

    private void RefreshName(Transform target)
    {
        if (nameText == null)
        {
            return;
        }

        if (currentDisplayName != null)
        {
            nameText.text = currentDisplayName.Display;
            nameText.color = currentDisplayName.DisplayColor;
            return;
        }

        nameText.text = target != null ? target.name : string.Empty;
        nameText.color = Color.white;
    }

    private void RefreshStatusEffects()
    {
        if (statusEffectContainer == null || statusEffectIconPrefab == null)
        {
            return;
        }

        if (currentStatusEffectController == null)
        {
            ClearStatusEffectIcons();
            return;
        }

        IReadOnlyList<StatusEffectUIInfo> effects = currentStatusEffectController.GetStatusEffectUIInfos();

        while (statusEffectIcons.Count < effects.Count)
        {
            StatusEffectIconUI icon = Instantiate(statusEffectIconPrefab, statusEffectContainer);
            icon.gameObject.SetActive(true);
            statusEffectIcons.Add(icon);
        }

        for (int i = 0; i < statusEffectIcons.Count; i++)
        {
            if (i < effects.Count)
            {
                statusEffectIcons[i].gameObject.SetActive(true);
                statusEffectIcons[i].Refresh(effects[i]);
            }
            else
            {
                statusEffectIcons[i].gameObject.SetActive(false);
            }
        }
    }

    private void ClearStatusEffectIcons()
    {
        for (int i = 0; i < statusEffectIcons.Count; i++)
        {
            if (statusEffectIcons[i] != null)
            {
                statusEffectIcons[i].gameObject.SetActive(false);
            }
        }
    }

    private void ClearTargetFrame()
    {
        SetVisible(false);

        UnsubscribeFromStatusEffects();

        currentTargetHealth = null;
        currentDisplayName = null;
        currentStatusEffectController = null;
        lastTarget = null;

        ClearStatusEffectIcons();
    }

    private void UnsubscribeFromStatusEffects()
    {
        if (currentStatusEffectController != null)
        {
            currentStatusEffectController.OnStatusEffectsChanged -= RefreshStatusEffects;
        }
    }

    private void SetVisible(bool visible)
    {
        if (visualRoot != null)
        {
            visualRoot.SetActive(visible);
        }
    }
}