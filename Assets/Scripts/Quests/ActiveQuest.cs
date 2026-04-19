[System.Serializable]
public class ActiveQuest
{
    public QuestDefinition definition;
    public int currentKills;

    public bool IsComplete => definition != null && currentKills >= definition.requiredKills;

    public ActiveQuest(QuestDefinition questDefinition)
    {
        definition = questDefinition;
        currentKills = 0;
    }
}