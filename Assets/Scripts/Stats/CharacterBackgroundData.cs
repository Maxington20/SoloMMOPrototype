using UnityEngine;

[CreateAssetMenu(fileName = "NewBackground", menuName = "Solo MMO/Character Background")]
public class CharacterBackgroundData : ScriptableObject
{
    [Header("Basic")]
    [SerializeField] private string backgroundName = "New Background";

    [TextArea(3, 6)]
    [SerializeField] private string description;

    [SerializeField] private Sprite icon;

    [Header("Bonuses")]
    [SerializeField] private CharacterStatModifier statModifier = new CharacterStatModifier();

    public string BackgroundName => backgroundName;
    public string Description => description;
    public Sprite Icon => icon;
    public StatBlock StatBonuses => statModifier != null ? statModifier.StatBonuses : StatBlock.Zero;
}