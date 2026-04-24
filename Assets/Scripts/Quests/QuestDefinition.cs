using UnityEngine;

[System.Serializable]
public class QuestDefinition
{
    [Header("Quest Info")]
    public QuestType questType;
    public string title;
    [TextArea(2, 5)] public string description;

    [Header("Objective")]
    public EnemyType targetEnemyType;
    public int requiredKills;

    [Header("Rewards")]
    public int xpReward;
    public int goldReward;
    public ItemData itemReward;
    public int itemRewardQuantity = 1;
}