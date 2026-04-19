using UnityEngine;

[System.Serializable]
public class QuestDefinition
{
    public QuestType questType;
    public string title;
    public string description;
    public EnemyType targetEnemyType;
    public int requiredKills;
    public int xpReward;
}