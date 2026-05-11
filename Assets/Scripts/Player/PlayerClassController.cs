using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(PlayerCombat))]
[RequireComponent(typeof(PlayerHotbar))]
[RequireComponent(typeof(PlayerAbilityLoadout))]
public class PlayerClassController : MonoBehaviour
{
    [Header("Class")]
    [SerializeField] private CharacterClassData selectedClass;

    [Header("Startup")]
    [SerializeField] private bool applyClassOnStart = false;
    [SerializeField] private bool applyClassStatsOnStart = true;
    [SerializeField] private bool assignClassAbilitiesToHotbar = true;
    [SerializeField] private bool autoAssignUnlockedAbilitiesToHotbar = true;

    private Health health;
    private PlayerCombat combat;
    private PlayerStats playerStats;
    private PlayerProgression progression;
    private PlayerAbilityLoadout abilityLoadout;

    public CharacterClassData SelectedClass => selectedClass;
    public string ClassName => selectedClass != null ? selectedClass.ClassName : "No Class";
    public bool HasSelectedClass => selectedClass != null;

    private void Awake()
    {
        health = GetComponent<Health>();
        combat = GetComponent<PlayerCombat>();
        playerStats = GetComponent<PlayerStats>();
        progression = GetComponent<PlayerProgression>();
        abilityLoadout = GetComponent<PlayerAbilityLoadout>();
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

        if (abilityLoadout != null)
        {
            abilityLoadout.ResetLearnedAbilities();
        }

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

        if (abilityLoadout != null)
        {
            abilityLoadout.ApplyClassStartingAbilities(
                selectedClass,
                assignClassAbilitiesToHotbar);

            abilityLoadout.LearnAvailableAbilitiesForLevel(
                selectedClass,
                GetCurrentLevel(),
                false,
                autoAssignUnlockedAbilitiesToHotbar);
        }

        if (playerStats != null)
        {
            playerStats.RecalculateAndApplyStats();
        }

        PostSystem($"Class selected: {selectedClass.ClassName}.");
    }

    public bool HasLearnedAbility(AbilityData ability)
    {
        return abilityLoadout != null && abilityLoadout.HasLearnedAbility(ability);
    }

    public IReadOnlyCollection<AbilityData> GetLearnedAbilities()
    {
        if (abilityLoadout != null)
        {
            return abilityLoadout.LearnedAbilities;
        }

        return new List<AbilityData>();
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

    private void HandleLevelChanged()
    {
        if (abilityLoadout == null || selectedClass == null)
        {
            return;
        }

        abilityLoadout.LearnAvailableAbilitiesForLevel(
            selectedClass,
            GetCurrentLevel(),
            true,
            autoAssignUnlockedAbilitiesToHotbar);
    }

    private int GetCurrentLevel()
    {
        return progression != null
            ? progression.Level
            : 1;
    }

    private void PostSystem(string message)
    {
        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem(message);
        }
    }
}