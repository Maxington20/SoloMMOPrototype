using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class QuestDefinition
{
    [Header("Quest Info")]
    public string title;

    [TextArea(2, 5)]
    public string description;

    [Header("Stages")]
    public List<QuestStage> stages = new List<QuestStage>();

    [Header("Rewards")]
    public int xpReward;
    public int goldReward;
    public List<QuestItemReward> guaranteedItemRewards = new List<QuestItemReward>();

    [Header("Choose One Reward")]
    public List<QuestItemReward> choiceItemRewards = new List<QuestItemReward>();

    public bool HasChoiceRewards => choiceItemRewards != null && choiceItemRewards.Count > 0;

    public QuestStage GetStage(int index)
    {
        if (stages == null || index < 0 || index >= stages.Count)
        {
            return null;
        }

        return stages[index];
    }

    public int StageCount => stages != null ? stages.Count : 0;

    public List<QuestCollectionObjective> GetAllCollectionObjectives()
    {
        List<QuestCollectionObjective> allObjectives = new List<QuestCollectionObjective>();

        if (stages == null)
        {
            return allObjectives;
        }

        for (int i = 0; i < stages.Count; i++)
        {
            QuestStage stage = stages[i];

            if (stage?.collectionObjectives == null)
            {
                continue;
            }

            allObjectives.AddRange(stage.collectionObjectives);
        }

        return allObjectives;
    }
}