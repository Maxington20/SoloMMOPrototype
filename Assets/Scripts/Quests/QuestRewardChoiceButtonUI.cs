using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestRewardChoiceButtonUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text quantityText;

    public void Refresh(QuestItemReward reward)
    {
        if (reward == null || reward.Item == null)
        {
            if (iconImage != null)
            {
                iconImage.enabled = false;
            }

            if (itemNameText != null)
            {
                itemNameText.text = "Missing Reward";
            }

            if (quantityText != null)
            {
                quantityText.text = string.Empty;
            }

            return;
        }

        ItemData item = reward.Item;

        if (iconImage != null)
        {
            iconImage.enabled = item.Icon != null;
            iconImage.sprite = item.Icon;
            iconImage.color = Color.white;
        }

        if (itemNameText != null)
        {
            itemNameText.text = item.DisplayName;
            itemNameText.color = ItemRarityUtility.GetColor(item.Rarity);
        }

        if (quantityText != null)
        {
            quantityText.text = reward.Quantity > 1 ? $"x{reward.Quantity}" : string.Empty;
        }
    }
}