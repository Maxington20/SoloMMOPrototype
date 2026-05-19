using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class QuestStage
{
    [Header("Stage Info")]
    public string stageName;

    [TextArea(1, 3)]
    public string objectiveSummary;

    [Header("Objectives")]
    public List<QuestKillObjective> killObjectives = new List<QuestKillObjective>();
    public List<QuestCollectionObjective> collectionObjectives = new List<QuestCollectionObjective>();
    public List<QuestTalkObjective> talkObjectives = new List<QuestTalkObjective>();
    public List<QuestWorldInteractObjective> worldInteractObjectives = new List<QuestWorldInteractObjective>();

    public string DisplayName
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(stageName))
            {
                return stageName;
            }

            return "Quest Stage";
        }
    }
}