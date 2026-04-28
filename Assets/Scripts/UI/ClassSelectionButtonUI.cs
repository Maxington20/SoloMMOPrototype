using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClassSelectionButtonUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image classIconImage;
    [SerializeField] private TMP_Text classNameText;
    [SerializeField] private TMP_Text roleText;
    [SerializeField] private TMP_Text primaryStatText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Button button;

    private CharacterClassData classData;
    private Action<CharacterClassData> onClicked;

    public void Initialize(CharacterClassData data, Action<CharacterClassData> clickedCallback)
    {
        classData = data;
        onClicked = clickedCallback;

        if (classIconImage != null)
        {
            classIconImage.sprite = classData != null ? classData.Icon : null;
            classIconImage.enabled = classData != null && classData.Icon != null;
        }

        if (classNameText != null)
        {
            classNameText.text = classData != null ? classData.ClassName : "Unknown";
        }

        if (roleText != null)
        {
            roleText.text = classData != null ? $"Role: {classData.Role}" : "Role: ?";
        }

        if (primaryStatText != null)
        {
            primaryStatText.text = classData != null ? $"Primary: {classData.PrimaryStat}" : "Primary: ?";
        }

        if (descriptionText != null)
        {
            descriptionText.text = classData != null ? classData.Description : string.Empty;
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(HandleClicked);
        }
    }

    private void HandleClicked()
    {
        if (classData == null)
        {
            return;
        }

        onClicked?.Invoke(classData);
    }
}