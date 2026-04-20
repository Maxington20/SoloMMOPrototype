using UnityEngine;

public class EnemyData : MonoBehaviour
{
    [SerializeField] private EnemyType enemyType = EnemyType.Wolf;
    [SerializeField] private int xpReward = 25;
    [SerializeField] private int minGold = 1;
    [SerializeField] private int maxGold = 5;

    public EnemyType EnemyType => enemyType;
    public int XpReward => xpReward;
    public int MinGold => minGold;
    public int MaxGold => maxGold;
}