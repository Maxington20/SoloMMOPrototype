using UnityEngine;

[CreateAssetMenu(fileName = "NewRace", menuName = "Solo MMO/Character Race")]
public class CharacterRaceData : ScriptableObject
{
    [Header("Basic")]
    [SerializeField] private string raceName = "New Race";

    [TextArea(3, 6)]
    [SerializeField] private string description;

    [SerializeField] private Sprite icon;

    [Header("Bonuses")]
    [SerializeField] private CharacterStatModifier statModifier = new CharacterStatModifier();

    [Header("Innate Ability")]
    [SerializeField] private AbilityData innateAbility;

    public string RaceName => raceName;
    public string Description => description;
    public Sprite Icon => icon;
    public StatBlock StatBonuses => statModifier != null ? statModifier.StatBonuses : StatBlock.Zero;
    public AbilityData InnateAbility => innateAbility;
}