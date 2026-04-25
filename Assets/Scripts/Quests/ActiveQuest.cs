using System.Collections.Generic;

public class ActiveQuest
{
    public QuestDefinition definition;

    private readonly Dictionary<EnemyType, int> killProgress = new Dictionary<EnemyType, int>();

    public ActiveQuest(QuestDefinition definition)
    {
        this.definition = definition;
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

    public bool IsComplete(PlayerInventory inventory)
    {
        if (definition == null)
        {
            return false;
        }

        if (definition.killObjectives != null)
        {
            for (int i = 0; i < definition.killObjectives.Count; i++)
            {
                QuestKillObjective objective = definition.killObjectives[i];
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

        if (definition.collectionObjectives != null)
        {
            for (int i = 0; i < definition.collectionObjectives.Count; i++)
            {
                QuestCollectionObjective objective = definition.collectionObjectives[i];
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

        return true;
    }
}