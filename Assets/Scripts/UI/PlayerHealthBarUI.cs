using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBarUI : MonoBehaviour
{
    [SerializeField] private Health playerHealth;
    [SerializeField] private Image fillImage;

    private void Update()
    {
        if (playerHealth == null || fillImage == null)
        {
            return;
        }

        fillImage.fillAmount = playerHealth.HealthPercent;
    }
}