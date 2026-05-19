using System.Collections.Generic;

public class ActiveQuest
{
    public QuestDefinition definition;

    private readonly Dictionary<EnemyType, int> killProgress = new Dictionary<EnemyType, int>();
    private readonly HashSet<NpcData> talkedToNpcs = new HashSet<NpcData>();
    private readonly HashSet<WorldInteractableData> interactedWorldObjects = new HashSet<WorldInteractableData>();

    public int CurrentStageIndex { get; private set; }

    public int CurrentStageNumber => CurrentStageIndex + 1;
    public int TotalStageCount => definition != null ? definition.StageCount : 0;

    public ActiveQuest(QuestDefinition definition)
    {
        this.definition = definition;
        CurrentStageIndex = 0;
    }

    public QuestStage GetCurrentStage()
    {
        return definition?.GetStage(CurrentStageIndex);
    }

    public List<QuestKillObjective> GetCurrentKillObjectives()
    {
        return GetCurrentStage()?.killObjectives;
    }

    public List<QuestCollectionObjective> GetCurrentCollectionObjectives()
    {
        return GetCurrentStage()?.collectionObjectives;
    }

    public List<QuestTalkObjective> GetCurrentTalkObjectives()
    {
        return GetCurrentStage()?.talkObjectives;
    }

    public List<QuestWorldInteractObjective> GetCurrentWorldInteractObjectives()
    {
        return GetCurrentStage()?.worldInteractObjectives;
    }

    public string GetCurrentStageTitle()
    {
        return GetCurrentStage()?.DisplayName ?? string.Empty;
    }

    public string GetCurrentStageSummary()
    {
        return GetCurrentStage()?.objectiveSummary ?? string.Empty;
    }

    public void RegisterKill(EnemyType enemyType)
    {
        if (!killProgress.ContainsKey(enemyType))
        {
            killProgress[enemyType] = 0;
        }

        killProgress[enemyType]++;
    }

    public int GetKillProgress(EnemyType enemyType)
    {
        return killProgress.TryGetValue(enemyType, out int value) ? value : 0;
    }

    public void RegisterTalkedToNpc(NpcData npc)
    {
        if (npc == null)
        {
            return;
        }

        talkedToNpcs.Add(npc);
    }

    public bool HasTalkedToNpc(NpcData npc)
    {
        return npc != null && talkedToNpcs.Contains(npc);
    }

    public void RegisterWorldInteractable(WorldInteractableData interactable)
    {
        if (interactable == null)
        {
            return;
        }

        interactedWorldObjects.Add(interactable);
    }

    public bool HasInteractedWithWorldInteractable(WorldInteractableData interactable)
    {
        return interactable != null && interactedWorldObjects.Contains(interactable);
    }

    public bool NeedsEnemyTypeInCurrentStage(EnemyType enemyType)
    {
        List<QuestKillObjective> objectives = GetCurrentKillObjectives();

        if (objectives == null)
        {
            return false;
        }

        for (int i = 0; i < objectives.Count; i++)
        {
            QuestKillObjective objective = objectives[i];

            if (objective != null && objective.EnemyType == enemyType)
            {
                return GetKillProgress(enemyType) < objective.RequiredAmount;
            }
        }

        return false;
    }

    public bool NeedsTalkToNpcInCurrentStage(NpcData npcData)
    {
        List<QuestTalkObjective> objectives = GetCurrentTalkObjectives();

        if (objectives == null || npcData == null)
        {
            return false;
        }

        for (int i = 0; i < objectives.Count; i++)
        {
            QuestTalkObjective objective = objectives[i];

            if (objective != null && objective.Npc == npcData)
            {
                return !HasTalkedToNpc(npcData);
            }
        }

        return false;
    }

    public bool NeedsWorldInteractableInCurrentStage(WorldInteractableData interactable)
    {
        List<QuestWorldInteractObjective> objectives = GetCurrentWorldInteractObjectives();

        if (objectives == null || interactable == null)
        {
            return false;
        }

        for (int i = 0; i < objectives.Count; i++)
        {
            QuestWorldInteractObjective objective = objectives[i];

            if (objective != null && objective.Interactable == interactable)
            {
                return !HasInteractedWithWorldInteractable(interactable);
            }
        }

        return false;
    }

    public bool TryAdvanceStage(PlayerInventory inventory)
    {
        bool advanced = false;

        while (definition != null && CurrentStageIndex < definition.StageCount && IsCurrentStageComplete(inventory))
        {
            CurrentStageIndex++;
            advanced = true;
        }

        return advanced;
    }

    public bool IsCurrentStageComplete(PlayerInventory inventory)
    {
        List<QuestKillObjective> killObjectives = GetCurrentKillObjectives();

        if (killObjectives != null)
        {
            for (int i = 0; i < killObjectives.Count; i++)
            {
                QuestKillObjective objective = killObjectives[i];

                if (objective == null)
                {
                    continue;
                }

                if (GetKillProgress(objective.EnemyType) < objective.RequiredAmount)
                {
                    return false;
                }
            }
        }

        List<QuestCollectionObjective> collectionObjectives = GetCurrentCollectionObjectives();

        if (collectionObjectives != null)
        {
            for (int i = 0; i < collectionObjectives.Count; i++)
            {
                QuestCollectionObjective objective = collectionObjectives[i];

                if (objective == null || objective.Item == null)
                {
                    continue;
                }

                if (inventory == null || inventory.GetTotalQuantityOfItem(objective.Item) < objective.RequiredAmount)
                {
                    return false;
                }
            }
        }

        List<QuestTalkObjective> talkObjectives = GetCurrentTalkObjectives();

        if (talkObjectives != null)
        {
            for (int i = 0; i < talkObjectives.Count; i++)
            {
                QuestTalkObjective objective = talkObjectives[i];

                if (objective == null || objective.Npc == null)
                {
                    continue;
                }

                if (!HasTalkedToNpc(objective.Npc))
                {
                    return false;
                }
            }
        }

        List<QuestWorldInteractObjective> worldObjectives = GetCurrentWorldInteractObjectives();

        if (worldObjectives != null)
        {
            for (int i = 0; i < worldObjectives.Count; i++)
            {
                QuestWorldInteractObjective objective = worldObjectives[i];

                if (objective == null || objective.Interactable == null)
                {
                    continue;
                }

                if (!HasInteractedWithWorldInteractable(objective.Interactable))
                {
                    return false;
                }
            }
        }

        return true;
    }

    public bool IsComplete(PlayerInventory inventory)
    {
        TryAdvanceStage(inventory);
        return definition != null && CurrentStageIndex >= definition.StageCount;
    }
}