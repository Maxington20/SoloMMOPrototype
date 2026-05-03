using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusEffectIconUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image cooldownFillImage;
    [SerializeField] private TMP_Text timerText;

    private StatusEffectUIInfo currentInfo;

    public void Refresh(StatusEffectUIInfo info)
    {
        currentInfo = info;

        if (iconImage != null)
        {
            iconImage.sprite = info.Data != null ? info.Data.Icon : null;
            iconImage.enabled = info.Data != null && info.Data.Icon != null;
        }

        if (cooldownFillImage != null)
        {
            float percent = info.Duration <= 0f ? 0f : info.RemainingTime / info.Duration;
            cooldownFillImage.fillAmount = Mathf.Clamp01(percent);
        }

        if (timerText != null)
        {
            timerText.text = Mathf.CeilToInt(info.RemainingTime).ToString();
        }
    }
}