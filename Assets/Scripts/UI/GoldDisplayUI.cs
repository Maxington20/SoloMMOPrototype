using TMPro;
using UnityEngine;

public class GoldDisplayUI : MonoBehaviour
{
    [SerializeField] private TMP_Text goldText;

    private void Start()
    {
        PlayerInventory.Instance.OnGoldChanged += UpdateGold;
        UpdateGold(PlayerInventory.Instance.Gold);
    }

    private void UpdateGold(int amount)
    {
        goldText.text = $"Gold: {amount}";
    }
}