using System;
using UnityEngine;

[RequireComponent(typeof(PlayerCombat))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(AbilityExecutor))]
[RequireComponent(typeof(AbilityCooldownController))]
[RequireComponent(typeof(AbilityResourceController))]
[RequireComponent(typeof(AbilityCastController))]
[RequireComponent(typeof(AbilityTargetValidator))]
[RequireComponent(typeof(AbilityUseValidator))]
public class PlayerAbilityController : MonoBehaviour
{
    private AbilityExecutor abilityExecutor;
    private AbilityCooldownController cooldownController;
    private AbilityResourceController resourceController;
    private AbilityCastController castController;
    private AbilityTargetValidator targetValidator;
    private AbilityUseValidator useValidator;

    public bool IsCasting => castController != null && castController.IsCasting;

    public AbilityData CurrentCastingAbility => castController != null
        ? castController.CurrentCastingAbility
        : null;

    public float CastProgress => castController != null
        ? castController.CastProgress
        : 0f;

    public event Action<AbilityData, float> OnCastStarted;
    public event Action<AbilityData> OnCastCompleted;
    public event Action<AbilityData> OnCastCancelled;

    private void Awake()
    {
        abilityExecutor = GetComponent<AbilityExecutor>();
        cooldownController = GetComponent<AbilityCooldownController>();
        resourceController = GetComponent<AbilityResourceController>();
        castController = GetComponent<AbilityCastController>();
        targetValidator = GetComponent<AbilityTargetValidator>();
        useValidator = GetComponent<AbilityUseValidator>();
    }

    private void OnEnable()
    {
        if (castController != null)
        {
            castController.OnCastStarted += HandleCastStarted;
            castController.OnCastReadyToComplete += HandleCastReadyToComplete;
            castController.OnCastCancelled += HandleCastCancelled;
        }
    }

    private void OnDisable()
    {
        if (castController != null)
        {
            castController.OnCastStarted -= HandleCastStarted;
            castController.OnCastReadyToComplete -= HandleCastReadyToComplete;
            castController.OnCastCancelled -= HandleCastCancelled;
        }
    }

    public float GetRemainingCooldown(AbilityData ability)
    {
        return cooldownController != null
            ? cooldownController.GetRemainingCooldown(ability)
            : 0f;
    }

    public bool IsOnCooldown(AbilityData ability)
    {
        return cooldownController != null && cooldownController.IsOnCooldown(ability);
    }

    public int CalculateAbilityHealing(AbilityData ability)
    {
        return abilityExecutor != null
            ? abilityExecutor.CalculateAbilityHealing(ability)
            : 0;
    }

    public float GetBaseHealingPower()
    {
        return abilityExecutor != null
            ? abilityExecutor.GetBaseHealingPower()
            : 0f;
    }

    public bool TryUseAbility(AbilityData ability)
    {
        if (ability == null)
        {
            return false;
        }

        if (IsCasting)
        {
            PostSystem("You are already casting.");
            return false;
        }

        if (!CanBeginAbility(ability))
        {
            return false;
        }

        if (ability.IsInstant)
        {
            return ExecuteAbility(ability);
        }

        StartCast(ability);
        return true;
    }

    private bool CanBeginAbility(AbilityData ability)
    {
        return useValidator != null &&
               useValidator.CanBeginAbility(ability, true);
    }

    private void StartCast(AbilityData ability)
    {
        if (castController == null)
        {
            ExecuteAbility(ability);
            return;
        }

        string castLabel = ability.CastType == AbilityCastType.Channel ? "channeling" : "casting";
        PostSystem($"You begin {castLabel} {ability.DisplayName}.");

        castController.BeginCast(ability);
    }

    private void HandleCastStarted(AbilityData ability, float duration)
    {
        OnCastStarted?.Invoke(ability, duration);
    }

    private void HandleCastReadyToComplete(AbilityData ability)
    {
        bool executed = ExecuteAbility(ability);

        if (executed)
        {
            OnCastCompleted?.Invoke(ability);
        }
    }

    private void HandleCastCancelled(AbilityData ability)
    {
        OnCastCancelled?.Invoke(ability);
    }

    private bool ExecuteAbility(AbilityData ability)
    {
        if (ability == null)
        {
            return false;
        }

        if (resourceController != null && !resourceController.CanPayResourceCost(ability))
        {
            PostSystem($"Not enough {resourceController.GetResourceName()} for {ability.DisplayName}.");
            return false;
        }

        bool used = ability.RequiresTarget
            ? ExecuteTargetAbility(ability)
            : ExecuteSelfAbility(ability);

        if (!used)
        {
            return false;
        }

        if (resourceController != null)
        {
            resourceController.SpendResourceCost(ability);
            resourceController.GenerateResourceFromAbility(ability);
        }

        if (cooldownController != null)
        {
            cooldownController.StartCooldown(ability);
        }

        return true;
    }

    private bool ExecuteTargetAbility(AbilityData ability)
    {
        if (abilityExecutor == null || targetValidator == null)
        {
            return false;
        }

        if (!targetValidator.CanUseAbilityOnCurrentTarget(
                ability.DisplayName,
                ability.Range,
                true))
        {
            return false;
        }

        targetValidator.FaceCurrentTarget();
        return abilityExecutor.ExecuteTargetAbility(ability);
    }

    private bool ExecuteSelfAbility(AbilityData ability)
    {
        return abilityExecutor != null &&
               abilityExecutor.ExecuteSelfAbility(ability);
    }

    private void PostSystem(string message)
    {
        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem(message);
        }
    }
}