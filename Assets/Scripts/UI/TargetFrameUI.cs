using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TargetFrameUI : MonoBehaviour
{
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image healthFill;
    [SerializeField] private GameObject visualRoot;

    private Health currentTargetHealth;
    private DisplayName currentDisplayName;
    private Transform lastTarget;

    private void Awake()
    {
        SetVisible(false);
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
            lastTarget = target;
            currentTargetHealth = target.GetComponent<Health>();
            currentDisplayName = target.GetComponent<DisplayName>();
            RefreshName(target);
        }

        if (currentTargetHealth != null && healthFill != null)
        {
            healthFill.fillAmount = currentTargetHealth.HealthPercent;

            if (currentTargetHealth.IsDead)
            {
                ClearTargetFrame();
            }
        }
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

    private void ClearTargetFrame()
    {
        SetVisible(false);
        currentTargetHealth = null;
        currentDisplayName = null;
        lastTarget = null;
    }

    private void SetVisible(bool visible)
    {
        if (visualRoot != null)
        {
            visualRoot.SetActive(visible);
        }
    }
}