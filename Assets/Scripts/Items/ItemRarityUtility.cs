using UnityEngine;

public static class ItemRarityUtility
{
    public static Color GetColor(ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Poor => new Color(0.55f, 0.55f, 0.55f, 1f),
            ItemRarity.Common => Color.white,
            ItemRarity.Uncommon => new Color(0.1f, 1f, 0.1f, 1f),
            ItemRarity.Rare => new Color(0.25f, 0.45f, 1f, 1f),
            ItemRarity.Epic => new Color(0.65f, 0.25f, 1f, 1f),
            _ => Color.white
        };
    }

    public static string GetDisplayName(ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Poor => "Poor",
            ItemRarity.Common => "Common",
            ItemRarity.Uncommon => "Uncommon",
            ItemRarity.Rare => "Rare",
            ItemRarity.Epic => "Epic",
            _ => "Common"
        };
    }
}