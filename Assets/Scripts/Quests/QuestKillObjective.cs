using System;
using UnityEngine;

[Serializable]
public class QuestKillObjective
{
    [SerializeField] private EnemyType enemyType;
    [SerializeField] private int requiredAmount = 1;

    public EnemyType EnemyType => enemyType;
    public int RequiredAmount => Mathf.Max(1, requiredAmount);
}