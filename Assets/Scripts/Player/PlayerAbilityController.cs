using System;
using UnityEngine;

[RequireComponent(typeof(PlayerCombat))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(AbilityExecutor))]
[RequireComponent(typeof(AbilityCooldownController))]
[RequireComponent(typeof(AbilityResourceController))]
[RequireComponent(typeof(AbilityCastController))]
public class PlayerAbilityController : MonoBehaviour
{
    private PlayerCombat playerCombat;
    private AbilityExecutor abilityExecutor;
    private AbilityCooldownController cooldownController;
    private AbilityResourceController resourceController;
    private AbilityCastController castController;

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
        playerCombat = GetComponent<PlayerCombat>();
        abilityExecutor = GetComponent<AbilityExecutor>();
        cooldownController = GetComponent<AbilityCooldownController>();
        resourceController = GetComponent<AbilityResourceController>();
        castController = GetComponent<AbilityCastController>();
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
        float cooldownRemaining = GetRemainingCooldown(ability);

        if (cooldownRemaining > 0f)
        {
            PostSystem($"{ability.DisplayName} is on cooldown for {Mathf.CeilToInt(cooldownRemaining)} more second(s).");
            return false;
        }

        if (!CanPayResourceCost(ability))
        {
            PostSystem($"Not enough {GetResourceName()} for {ability.DisplayName}.");
            return false;
        }

        if (ability.RequiresTarget)
        {
            return playerCombat != null &&
                   playerCombat.CanUseAbilityOnCurrentTarget(
                       ability.DisplayName,
                       ability.Range,
                       true);
        }

        return abilityExecutor != null &&
               abilityExecutor.CanExecuteSelfAbility(ability, true);
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

        if (!CanPayResourceCost(ability))
        {
            PostSystem($"Not enough {GetResourceName()} for {ability.DisplayName}.");
            return false;
        }

        bool used = ability.RequiresTarget
            ? ExecuteTargetAbility(ability)
            : ExecuteSelfAbility(ability);

        if (!used)
        {
            return false;
        }

        SpendResourceCost(ability);
        GenerateResourceFromAbility(ability);
        StartCooldown(ability);

        return true;
    }

    private bool ExecuteTargetAbility(AbilityData ability)
    {
        if (playerCombat == null || abilityExecutor == null)
        {
            return false;
        }

        if (!playerCombat.CanUseAbilityOnCurrentTarget(
                ability.DisplayName,
                ability.Range,
                true))
        {
            return false;
        }

        playerCombat.FaceCurrentTarget();
        return abilityExecutor.ExecuteTargetAbility(ability);
    }

    private bool ExecuteSelfAbility(AbilityData ability)
    {
        return abilityExecutor != null &&
               abilityExecutor.ExecuteSelfAbility(ability);
    }

    private bool CanPayResourceCost(AbilityData ability)
    {
        return resourceController == null ||
               resourceController.CanPayResourceCost(ability);
    }

    private void SpendResourceCost(AbilityData ability)
    {
        if (resourceController != null)
        {
            resourceController.SpendResourceCost(ability);
        }
    }

    private void GenerateResourceFromAbility(AbilityData ability)
    {
        if (resourceController != null)
        {
            resourceController.GenerateResourceFromAbility(ability);
        }
    }

    private void StartCooldown(AbilityData ability)
    {
        if (cooldownController != null)
        {
            cooldownController.StartCooldown(ability);
        }
    }

    private string GetResourceName()
    {
        return resourceController != null
            ? resourceController.GetResourceName()
            : "resource";
    }

    private void PostSystem(string message)
    {
        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem(message);
        }
    }
}