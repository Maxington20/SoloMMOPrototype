using UnityEngine;

public class EnemyData : MonoBehaviour
{
    [SerializeField] private EnemyType enemyType = EnemyType.Wolf;
    [SerializeField] private int xpReward = 25;

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

    public float GoldDropChance => Mathf.Clamp01(goldDropChance);
    public int MinGold => Mathf.Max(0, minGold);
    public int MaxGold => Mathf.Max(MinGold, maxGold);

    public int LootSlotCount => Mathf.Max(1, lootSlotCount);
    public int ItemDropRolls => Mathf.Max(0, itemDropRolls);
    public EnemyLootTableEntry[] LootTable => lootTable;
}