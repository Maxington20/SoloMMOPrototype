using UnityEngine;
using TMPro;

public class DisplayName : MonoBehaviour
{
    [SerializeField] private string displayName = "Unit";
    [SerializeField] private Color displayColor = Color.white;
    [SerializeField] private TMP_Text nameText;

    public string Display => displayName;
    public Color DisplayColor => displayColor;

    private void Awake()
    {
        ApplyDisplayName();
    }

    public void SetDisplayName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            return;
        }

        displayName = newName;
        ApplyDisplayName();
    }

    public void SetDisplayColor(Color newColor)
    {
        displayColor = newColor;
        ApplyDisplayName();
    }

    private void ApplyDisplayName()
    {
        if (nameText == null)
        {
            return;
        }

        nameText.text = displayName;
        nameText.color = displayColor;
    }
}