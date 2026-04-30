using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(PlayerCombat))]
public class PlayerClassController : MonoBehaviour
{
    [Header("Class")]
    [SerializeField] private CharacterClassData selectedClass;

    [Header("Startup")]
    [SerializeField] private bool applyClassOnStart = false;
    [SerializeField] private bool applyClassStatsOnStart = true;
    [SerializeField] private bool assignClassAbilitiesToHotbar = true;

    private Health health;
    private PlayerCombat combat;
    private PlayerHotbar hotbar;
    private PlayerStats playerStats;
    private PlayerProgression progression;

    private readonly HashSet<AbilityData> learnedAbilities = new HashSet<AbilityData>();

    public CharacterClassData SelectedClass => selectedClass;
    public string ClassName => selectedClass != null ? selectedClass.ClassName : "No Class";
    public bool HasSelectedClass => selectedClass != null;

    private void Awake()
    {
        health = GetComponent<Health>();
        combat = GetComponent<PlayerCombat>();
        hotbar = GetComponent<PlayerHotbar>();
        playerStats = GetComponent<PlayerStats>();
        progression = GetComponent<PlayerProgression>();
    }

    private void OnEnable()
    {
        if (progression != null)
        {
            progression.OnLevelChanged += HandleLevelChanged;
        }
    }

    private void OnDisable()
    {
        if (progression != null)
        {
            progression.OnLevelChanged -= HandleLevelChanged;
        }
    }

    private void Start()
    {
        if (applyClassOnStart && selectedClass != null)
        {
            ApplyClass();
        }
    }

    public void SetSelectedClass(CharacterClassData newClass, bool applyImmediately)
    {
        if (newClass == null)
        {
            Debug.LogWarning("Cannot set selected class because newClass is null.");
            return;
        }

        selectedClass = newClass;
        learnedAbilities.Clear();

        if (applyImmediately)
        {
            ApplyClass();
        }
    }

    public void ApplyClass()
    {
        if (selectedClass == null)
        {
            Debug.LogWarning("PlayerClassController has no selected class assigned.");
            return;
        }

        if (applyClassStatsOnStart)
        {
            ApplyClassBaseValues();
        }

        if (assignClassAbilitiesToHotbar)
        {
            AssignStartingClassAbilities();
        }

        LearnAvailableAbilitiesForCurrentLevel(false);

        if (playerStats != null)
        {
            playerStats.RecalculateAndApplyStats();
        }

        PostSystem($"Class selected: {selectedClass.ClassName}.");
    }

    public bool HasLearnedAbility(AbilityData ability)
    {
        return ability != null && learnedAbilities.Contains(ability);
    }

    public IReadOnlyCollection<AbilityData> GetLearnedAbilities()
    {
        return learnedAbilities;
    }

    private void ApplyClassBaseValues()
    {
        if (health != null)
        {
            health.SetBaseMaxHealth(selectedClass.StartingMaxHealth, true);
        }

        if (combat != null)
        {
            combat.SetBaseDamage(selectedClass.StartingBaseDamage);
        }
    }

    private void AssignStartingClassAbilities()
    {
        if (hotbar == null || selectedClass.StartingAbilities == null)
        {
            return;
        }

        int maxSlots = Mathf.Min(hotbar.SlotCount, selectedClass.StartingAbilities.Length);

        for (int i = 0; i < maxSlots; i++)
        {
            AbilityData ability = selectedClass.StartingAbilities[i];

            if (ability == null)
            {
                continue;
            }

            LearnAbility(ability, false);

            hotbar.AssignAbilityToSlot(i, ability);
        }
    }

    private void HandleLevelChanged()
    {
        LearnAvailableAbilitiesForCurrentLevel(true);
    }

    private void LearnAvailableAbilitiesForCurrentLevel(bool announceNewAbilities)
    {
        if (selectedClass == null || selectedClass.AbilityUnlocks == null)
        {
            return;
        }

        int currentLevel = progression != null ? progression.Level : 1;

        foreach (ClassAbilityUnlock unlock in selectedClass.AbilityUnlocks)
        {
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

            if (unlock.AutoAssignToHotbar)
            {
                TryAssignAbilityToFirstEmptyHotbarSlot(unlock.Ability);
            }
        }
    }

    private void LearnAbility(AbilityData ability, bool announce)
    {
        if (ability == null)
        {
            return;
        }

        if (!learnedAbilities.Add(ability))
        {
            return;
        }

        if (announce)
        {
            PostSystem($"You learned {ability.DisplayName}.");
        }
    }

    private void TryAssignAbilityToFirstEmptyHotbarSlot(AbilityData ability)
    {
        if (hotbar == null || ability == null)
        {
            return;
        }

        for (int i = 0; i < hotbar.SlotCount; i++)
        {
            if (hotbar.IsSlotEmpty(i))
            {
                hotbar.AssignAbilityToSlot(i, ability);
                return;
            }
        }
    }

    private void PostSystem(string message)
    {
        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem(message);
        }
    }
}