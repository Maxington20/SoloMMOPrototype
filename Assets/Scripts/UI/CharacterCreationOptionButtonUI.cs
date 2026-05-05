using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCreationOptionButtonUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text subtitleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Button button;

    [Header("Selection Visual")]
    [SerializeField] private GameObject selectionOutline;

    private Action onClicked;

    public void Initialize(
        Sprite icon,
        string title,
        string subtitle,
        string description,
        Action clickedCallback)
    {
        onClicked = clickedCallback;

        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
        }

        if (titleText != null)
        {
            titleText.text = title;
        }

        if (subtitleText != null)
        {
            subtitleText.text = subtitle;
        }

        if (descriptionText != null)
        {
            descriptionText.text = description;
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(HandleClicked);
        }

        SetSelected(false);
    }

    private void HandleClicked()
    {
        onClicked?.Invoke();
    }

    public void SetSelected(bool selected)
    {
        if (selectionOutline != null)
        {
            selectionOutline.SetActive(selected);
        }
    }
}