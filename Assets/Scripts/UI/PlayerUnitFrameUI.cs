using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUnitFrameUI : MonoBehaviour
{
    [Header("Player References")]
    [SerializeField] private DisplayName displayName;
    [SerializeField] private PlayerClassController classController;
    [SerializeField] private PlayerProgression progression;
    [SerializeField] private Health health;
    [SerializeField] private PlayerResource playerResource;

    [Header("Root")]
    [SerializeField] private GameObject visualRoot;

    [Header("Header")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text classLevelText;

    [Header("Health")]
    [SerializeField] private Image healthFillImage;
    [SerializeField] private TMP_Text healthText;

    [Header("Mana")]
    [SerializeField] private GameObject manaRoot;
    [SerializeField] private Image manaFillImage;
    [SerializeField] private TMP_Text manaText;

    [Header("XP")]
    [SerializeField] private Image xpFillImage;
    [SerializeField] private TMP_Text xpText;

    private void OnEnable()
    {
        if (health != null)
        {
            health.OnHealthChanged += Refresh;
        }

        if (playerResource != null)
        {
            playerResource.OnManaChanged += Refresh;
        }

        if (progression != null)
        {
            progression.OnLevelChanged += Refresh;
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnHealthChanged -= Refresh;
        }

        if (playerResource != null)
        {
            playerResource.OnManaChanged -= Refresh;
        }

        if (progression != null)
        {
            progression.OnLevelChanged -= Refresh;
        }
    }

    private void Update()
    {
        Refresh();
    }

    private void Refresh()
    {
        if (visualRoot != null)
        {
            visualRoot.SetActive(true);
        }

        RefreshHeader();
        RefreshHealth();
        RefreshMana();
        RefreshXp();
    }

    private void RefreshHeader()
    {
        if (nameText != null)
        {
            nameText.text = displayName != null ? displayName.Display : "Player";
        }

        if (classLevelText != null)
        {
            string className = classController != null && classController.SelectedClass != null
                ? classController.SelectedClass.ClassName
                : "No Class";

            int level = progression != null ? progression.Level : 1;

            classLevelText.text = $"Level {level} {className}";
        }
    }

    private void RefreshHealth()
    {
        if (health == null)
        {
            if (healthFillImage != null)
            {
                healthFillImage.fillAmount = 0f;
            }

            if (healthText != null)
            {
                healthText.text = "Health: ?";
            }

            return;
        }

        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = health.HealthPercent;
        }

        if (healthText != null)
        {
            healthText.text = $"{health.CurrentHealth}/{health.MaxHealth}";
        }
    }

    private void RefreshMana()
    {
        bool hasMana = playerResource != null && playerResource.HasManaResource;

        if (manaRoot != null)
        {
            manaRoot.SetActive(hasMana);
        }

        if (!hasMana)
        {
            return;
        }

        if (manaFillImage != null)
        {
            manaFillImage.fillAmount = playerResource.ManaPercent;
        }

        if (manaText != null)
        {
            manaText.text = $"{playerResource.CurrentMana}/{playerResource.MaxMana}";
        }
    }

    private void RefreshXp()
    {
        if (progression == null)
        {
            if (xpFillImage != null)
            {
                xpFillImage.fillAmount = 0f;
            }

            if (xpText != null)
            {
                xpText.text = "XP: ?";
            }

            return;
        }

        if (xpFillImage != null)
        {
            xpFillImage.fillAmount = progression.XpPercent;
        }

        if (xpText != null)
        {
            xpText.text = $"{progression.CurrentXp}/{progression.XpToNextLevel} XP";
        }
    }
}