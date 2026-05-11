using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerHotbar))]
public class PlayerAbilityLoadout : MonoBehaviour
{
    public event Action OnLearnedAbilitiesChanged;

    private readonly HashSet<AbilityData> learnedAbilities = new HashSet<AbilityData>();

    private PlayerHotbar hotbar;

    public IReadOnlyCollection<AbilityData> LearnedAbilities => learnedAbilities;

    private void Awake()
    {
        hotbar = GetComponent<PlayerHotbar>();
    }

    public void ResetLearnedAbilities()
    {
        learnedAbilities.Clear();
        OnLearnedAbilitiesChanged?.Invoke();
    }

    public bool HasLearnedAbility(AbilityData ability)
    {
        return ability != null && learnedAbilities.Contains(ability);
    }

    public void ApplyClassStartingAbilities(CharacterClassData characterClass, bool assignToHotbar)
    {
        if (characterClass == null)
        {
            return;
        }

        AbilityData[] startingAbilities = characterClass.StartingAbilities;

        if (startingAbilities == null)
        {
            OnLearnedAbilitiesChanged?.Invoke();
            return;
        }

        if (assignToHotbar && hotbar != null)
        {
            hotbar.ClearAbilitySlots();
        }

        for (int i = 0; i < startingAbilities.Length; i++)
        {
            AbilityData ability = startingAbilities[i];

            if (ability == null)
            {
                continue;
            }

            LearnAbility(ability, false);

            if (assignToHotbar && hotbar != null && i < hotbar.SlotCount)
            {
                hotbar.AssignAbilityToSlot(i, ability);
            }
        }

        OnLearnedAbilitiesChanged?.Invoke();
    }

    public void LearnAvailableAbilitiesForLevel(
        CharacterClassData characterClass,
        int level,
        bool announceNewAbilities,
        bool autoAssignUnlockedAbilities)
    {
        if (characterClass == null || characterClass.AbilityUnlocks == null)
        {
            return;
        }

        bool learnedAny = false;
        int currentLevel = Mathf.Max(1, level);

        for (int i = 0; i < characterClass.AbilityUnlocks.Length; i++)
        {
            ClassAbilityUnlock unlock = characterClass.AbilityUnlocks[i];

            if (unlock == null || unlock.Ability == null)
            {
                continue;
            }

            if (unlock.UnlockLevel > currentLevel)
            {
                continue;
            }

            if (HasLearnedAbility(unlock.Ability))
            {
                continue;
            }

            LearnAbility(unlock.Ability, announceNewAbilities);
            learnedAny = true;

            if (autoAssignUnlockedAbilities && unlock.AutoAssignToHotbar && hotbar != null)
            {
                hotbar.AssignAbilityToFirstEmptySlot(unlock.Ability);
            }
        }

        if (learnedAny)
        {
            OnLearnedAbilitiesChanged?.Invoke();
        }
    }

    public bool LearnAbility(AbilityData ability, bool announce)
    {
        if (ability == null)
        {
            return false;
        }

        if (!learnedAbilities.Add(ability))
        {
            return false;
        }

        if (announce)
        {
            PostSystem($"You learned {ability.DisplayName}.");
        }

        return true;
    }

    private void PostSystem(string message)
    {
        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem(message);
        }
    }
}