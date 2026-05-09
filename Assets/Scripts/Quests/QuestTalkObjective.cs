using System;
using UnityEngine;

[Serializable]
public class QuestTalkObjective
{
    [SerializeField] private NpcData npc;

    public NpcData Npc => npc;

    public string DisplayName
    {
        get
        {
            if (npc != null && !string.IsNullOrWhiteSpace(npc.DisplayName))
            {
                return npc.DisplayName;
            }

            return "NPC";
        }
    }
}