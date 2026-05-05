using System;
using UnityEngine;

public class PlayerCharacterIdentity : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private CharacterRaceData selectedRace;
    [SerializeField] private CharacterBackgroundData selectedBackground;

    public CharacterRaceData SelectedRace => selectedRace;
    public CharacterBackgroundData SelectedBackground => selectedBackground;

    public StatBlock RaceStats => selectedRace != null ? selectedRace.StatBonuses : StatBlock.Zero;
    public StatBlock BackgroundStats => selectedBackground != null ? selectedBackground.StatBonuses : StatBlock.Zero;

    public AbilityData RaceInnateAbility => selectedRace != null ? selectedRace.InnateAbility : null;

    public event Action OnIdentityChanged;

    public void SetRace(CharacterRaceData race)
    {
        selectedRace = race;
        OnIdentityChanged?.Invoke();
    }

    public void SetBackground(CharacterBackgroundData background)
    {
        selectedBackground = background;
        OnIdentityChanged?.Invoke();
    }

    public void SetIdentity(CharacterRaceData race, CharacterBackgroundData background)
    {
        selectedRace = race;
        selectedBackground = background;
        OnIdentityChanged?.Invoke();
    }
}