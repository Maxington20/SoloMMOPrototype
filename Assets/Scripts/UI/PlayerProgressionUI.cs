using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerProgressionUI : MonoBehaviour
{
    [SerializeField] private PlayerProgression playerProgression;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Image xpFill;
    [SerializeField] private TextMeshProUGUI playerNameText;

    private void Update()
    {
        if (playerProgression == null)
        {
            return;
        }

        if (levelText != null)
        {
            levelText.text = $"Level {playerProgression.Level}";
        }

        if (xpFill != null)
        {
            xpFill.fillAmount = playerProgression.XpPercent;
        }

        if (playerNameText != null && playerProgression != null)
{
        DisplayName displayName = playerProgression.GetComponent<DisplayName>();

        if (displayName != null)
        {
            playerNameText.text = displayName.Display;
        }
}
    }
}