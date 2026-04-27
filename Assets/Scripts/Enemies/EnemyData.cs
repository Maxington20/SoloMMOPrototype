using UnityEngine;

public class EnemyData : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private EnemyType enemyType = EnemyType.Wolf;
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
    public int XpReward => xpReward;

    public int BaseMaxHealth => Mathf.Max(1, baseMaxHealth);
    public int BaseDamage => Mathf.Max(0, baseDamage);
    public PrimaryStatType PrimaryStat => primaryStat;
    public StatBlock Stats => stats ?? StatBlock.Zero;
    public ClassCombatTuning CombatTuning => combatTuning ?? new ClassCombatTuning();

    public float GoldDropChance => Mathf.Clamp01(goldDropChance);
    public int MinGold => Mathf.Max(0, minGold);
    public int MaxGold => Mathf.Max(MinGold, maxGold);

    public int LootSlotCount => Mathf.Max(1, lootSlotCount);
    public int ItemDropRolls => Mathf.Max(0, itemDropRolls);
    public EnemyLootTableEntry[] LootTable => lootTable;
}