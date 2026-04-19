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
    private Transform lastTarget;

    private void Awake()
    {
        SetVisible(false);
    }

    private void Update()
    {
        if (playerCombat == null)
        {
            SetVisible(false);
            currentTargetHealth = null;
            lastTarget = null;
            return;
        }

        Transform target = playerCombat.CurrentTargetTransform;

        if (target == null)
        {
            SetVisible(false);
            currentTargetHealth = null;
            lastTarget = null;
            return;
        }

        SetVisible(true);

        if (target != lastTarget)
        {
            lastTarget = target;
            currentTargetHealth = target.GetComponent<Health>();

            DisplayName displayName = target.GetComponent<DisplayName>();

            if (displayName != null)
            {
                nameText.text = displayName.Display;
            }
            else
            {
                nameText.text = target.name;
            }
        }

        if (currentTargetHealth != null && healthFill != null)
        {
            healthFill.fillAmount = currentTargetHealth.HealthPercent;

            if (currentTargetHealth.IsDead)
            {
                SetVisible(false);
                currentTargetHealth = null;
                lastTarget = null;
            }
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