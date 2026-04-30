using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManaBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerResource playerResource;
    [SerializeField] private GameObject visualRoot;
    [SerializeField] private Image manaFillImage;
    [SerializeField] private TMP_Text manaText;

    private void OnEnable()
    {
        if (playerResource != null)
        {
            playerResource.OnManaChanged += Refresh;
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (playerResource != null)
        {
            playerResource.OnManaChanged -= Refresh;
        }
    }

    private void Update()
    {
        Refresh();
    }

    private void Refresh()
    {
        if (playerResource == null)
        {
            SetVisible(false);
            return;
        }

        bool shouldShow = playerResource.HasManaResource;
        SetVisible(shouldShow);

        if (!shouldShow)
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

    private void SetVisible(bool visible)
    {
        if (visualRoot != null)
        {
            visualRoot.SetActive(visible);
        }
    }
}