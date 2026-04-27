using UnityEngine;

public class DisplayName : MonoBehaviour
{
    [SerializeField] private string displayName = "Unknown";
    [SerializeField] private Color defaultColor = Color.white;

    private EnemyData enemyData;

    public string BaseDisplay => displayName;

    public string Display
    {
        get
        {
            if (enemyData != null)
            {
                return $"{enemyData.GetDisplayNamePrefix()}{displayName}";
            }

            return displayName;
        }
    }

    public Color DisplayColor
    {
        get
        {
            if (enemyData != null)
            {
                return enemyData.GetTierNameColor(defaultColor);
            }

            return defaultColor;
        }
    }

    private void Awake()
    {
        enemyData = GetComponent<EnemyData>();
    }
}