using UnityEngine;

public class EnemyData : MonoBehaviour
{
    [SerializeField] private EnemyType enemyType = EnemyType.Wolf;
    [SerializeField] private int xpReward = 25;

    public EnemyType EnemyType => enemyType;
    public int XpReward => xpReward;
}