using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class QuestDefinition
{
    [Header("Quest Info")]
    public string title;
    [TextArea(2, 5)] public string description;

    [Header("Objectives")]
    public List<QuestKillObjective> killObjectives = new List<QuestKillObjective>();
    public List<QuestCollectionObjective> collectionObjectives = new List<QuestCollectionObjective>();

    [Header("Rewards")]
    public int xpReward;
    public int goldReward;
    public List<QuestItemReward> guaranteedItemRewards = new List<QuestItemReward>();

    [Header("Choose One Reward")]
    public List<QuestItemReward> choiceItemRewards = new List<QuestItemReward>();

    public bool HasChoiceRewards => choiceItemRewards != null && choiceItemRewards.Count > 0;
}