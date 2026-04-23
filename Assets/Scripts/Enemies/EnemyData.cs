using UnityEngine;

public class EnemyData : MonoBehaviour
{
    [SerializeField] private EnemyType enemyType = EnemyType.Wolf;
    [SerializeField] private int xpReward = 25;
    [SerializeField] private int minGold = 1;
    [SerializeField] private int maxGold = 5;

    [Header("Loot")]
    [SerializeField] private int lootSlotCount = 6;
    [SerializeField] private EnemyLootTableEntry[] lootTable;

    public EnemyType EnemyType => enemyType;
    public int XpReward => xpReward;
    public int MinGold => minGold;
    public int MaxGold => maxGold;
    public int LootSlotCount => Mathf.Max(1, lootSlotCount);
    public EnemyLootTableEntry[] LootTable => lootTable;
}