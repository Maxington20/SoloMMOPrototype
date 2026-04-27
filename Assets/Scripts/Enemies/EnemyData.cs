using UnityEngine;

public class EnemyData : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private EnemyType enemyType = EnemyType.Wolf;
    [SerializeField] private EnemyDifficultyTier difficultyTier = EnemyDifficultyTier.Normal;
    [SerializeField] private int xpReward = 25;

    [Header("Base Combat")]
    [SerializeField] private int baseMaxHealth = 60;
    [SerializeField] private int baseDamage = 8;
    [SerializeField] private PrimaryStatType primaryStat = PrimaryStatType.Strength;

    [Header("Stats")]
    [SerializeField] private StatBlock stats = new StatBlock(5, 5, 1, 5, 0, 0);

    [Header("Combat Tuning")]
    [SerializeField] private ClassCombatTuning combatTuning = new ClassCombatTuning();

    [Header("Gold Loot")]
    [SerializeField, Range(0f, 1f)] private float goldDropChance = 1f;
    [SerializeField] private int minGold = 1;
    [SerializeField] private int maxGold = 5;

    [Header("Item Loot")]
    [SerializeField] private int lootSlotCount = 6;
    [SerializeField] private int itemDropRolls = 1;
    [SerializeField] private EnemyLootTableEntry[] lootTable;

    public EnemyType EnemyType => enemyType;
    public EnemyDifficultyTier DifficultyTier => difficultyTier;

    public int XpReward => ApplyIntMultiplier(xpReward, GetXpMultiplier());

    public int BaseMaxHealth => ApplyIntMultiplier(baseMaxHealth, GetHealthMultiplier());
    public int BaseDamage => ApplyIntMultiplier(baseDamage, GetDamageMultiplier());

    public PrimaryStatType PrimaryStat => primaryStat;
    public StatBlock Stats => stats ?? StatBlock.Zero;
    public ClassCombatTuning CombatTuning => combatTuning ?? new ClassCombatTuning();

    public float GoldDropChance => Mathf.Clamp01(goldDropChance);
    public int MinGold => ApplyIntMultiplier(Mathf.Max(0, minGold), GetGoldMultiplier());
    public int MaxGold => Mathf.Max(MinGold, ApplyIntMultiplier(maxGold, GetGoldMultiplier()));

    public int LootSlotCount => Mathf.Max(1, lootSlotCount);
    public int ItemDropRolls => Mathf.Max(0, itemDropRolls + GetBonusItemDropRolls());
    public EnemyLootTableEntry[] LootTable => lootTable;

    public string GetDisplayNamePrefix()
    {
        return difficultyTier switch
        {
            EnemyDifficultyTier.Elite => "Elite ",
            EnemyDifficultyTier.Boss => "Boss ",
            _ => string.Empty
        };
    }

    public Color GetTierNameColor(Color fallbackColor)
    {
        return difficultyTier switch
        {
            EnemyDifficultyTier.Elite => new Color(1f, 0.65f, 0.2f),
            EnemyDifficultyTier.Rare => new Color(0.35f, 0.55f, 1f),
            EnemyDifficultyTier.Boss => new Color(0.8f, 0.25f, 1f),
            _ => fallbackColor
        };
    }

    private float GetHealthMultiplier()
    {
        return difficultyTier switch
        {
            EnemyDifficultyTier.Elite => 2.0f,
            EnemyDifficultyTier.Rare => 2.5f,
            EnemyDifficultyTier.Boss => 5.0f,
            _ => 1.0f
        };
    }

    private float GetDamageMultiplier()
    {
        return difficultyTier switch
        {
            EnemyDifficultyTier.Elite => 1.5f,
            EnemyDifficultyTier.Rare => 1.75f,
            EnemyDifficultyTier.Boss => 2.5f,
            _ => 1.0f
        };
    }

    private float GetXpMultiplier()
    {
        return difficultyTier switch
        {
            EnemyDifficultyTier.Elite => 2.0f,
            EnemyDifficultyTier.Rare => 3.0f,
            EnemyDifficultyTier.Boss => 8.0f,
            _ => 1.0f
        };
    }

    private float GetGoldMultiplier()
    {
        return difficultyTier switch
        {
            EnemyDifficultyTier.Elite => 2.0f,
            EnemyDifficultyTier.Rare => 3.0f,
            EnemyDifficultyTier.Boss => 6.0f,
            _ => 1.0f
        };
    }

    private int GetBonusItemDropRolls()
    {
        return difficultyTier switch
        {
            EnemyDifficultyTier.Elite => 1,
            EnemyDifficultyTier.Rare => 2,
            EnemyDifficultyTier.Boss => 4,
            _ => 0
        };
    }

    private int ApplyIntMultiplier(int value, float multiplier)
    {
        return Mathf.Max(0, Mathf.RoundToInt(value * multiplier));
    }
}