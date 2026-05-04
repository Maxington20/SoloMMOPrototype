using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TargetCastBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerCombat playerCombat;

    [Header("Root")]
    [SerializeField] private GameObject visualRoot;

    [Header("UI")]
    [SerializeField] private Image fillImage;
    [SerializeField] private TMP_Text abilityNameText;
    [SerializeField] private TMP_Text timeText;

    private EnemyAbilityController currentEnemyAbilityController;
    private Transform lastTarget;

    private void Awake()
    {
        SetVisible(false);
    }

    private void Update()
    {
        RefreshTarget();
        RefreshCastBar();
    }

    private void RefreshTarget()
    {
        if (playerCombat == null)
        {
            currentEnemyAbilityController = null;
            lastTarget = null;
            return;
        }

        Transform currentTarget = playerCombat.CurrentTargetTransform;

        if (currentTarget == lastTarget)
        {
            return;
        }

        lastTarget = currentTarget;
        currentEnemyAbilityController = currentTarget != null
            ? currentTarget.GetComponent<EnemyAbilityController>()
            : null;
    }

    private void RefreshCastBar()
    {
        if (currentEnemyAbilityController == null || !currentEnemyAbilityController.IsCasting)
        {
            SetVisible(false);
            return;
        }

        AbilityData ability = currentEnemyAbilityController.CurrentCastingAbility;

        if (abilityNameText != null)
        {
            abilityNameText.text = ability != null ? ability.DisplayName : "Casting";
        }

        if (fillImage != null)
        {
            fillImage.fillAmount = currentEnemyAbilityController.CastProgress;
        }

        if (timeText != null)
        {
            timeText.text = $"{currentEnemyAbilityController.CastRemainingTime:0.0}s";
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