using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerCombat))]
[RequireComponent(typeof(Health))]
public class PlayerAbilityController : MonoBehaviour
{
    [Header("Casting")]
    [SerializeField] private float movementCancelDistance = 0.08f;

    private PlayerCombat playerCombat;
    private Health playerHealth;
    private PlayerStats playerStats;
    private PlayerResource playerResource;

    private readonly Dictionary<AbilityData, float> cooldownEndTimes = new Dictionary<AbilityData, float>();

    private bool isCasting;
    private AbilityData currentCastingAbility;
    private float castStartTime;
    private float castDuration;
    private Vector3 castStartPosition;

    public bool IsCasting => isCasting;
    public AbilityData CurrentCastingAbility => currentCastingAbility;
    public float CastProgress => !isCasting || castDuration <= 0f
        ? 0f
        : Mathf.Clamp01((Time.time - castStartTime) / castDuration);

    public event Action<AbilityData, float> OnCastStarted;
    public event Action<AbilityData> OnCastCompleted;
    public event Action<AbilityData> OnCastCancelled;

    private void Awake()
    {
        playerCombat = GetComponent<PlayerCombat>();
        playerHealth = GetComponent<Health>();
        playerStats = GetComponent<PlayerStats>();
        playerResource = GetComponent<PlayerResource>();
    }

    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDamaged += HandlePlayerDamaged;
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDamaged -= HandlePlayerDamaged;
        }
    }

    private void Update()
    {
        UpdateCasting();
    }

    public float GetRemainingCooldown(AbilityData ability)
    {
        if (ability == null)
        {
            return 0f;
        }

        if (!cooldownEndTimes.TryGetValue(ability, out float endTime))
        {
            return 0f;
        }

        return Mathf.Max(0f, endTime - Time.time);
    }

    public bool IsOnCooldown(AbilityData ability)
    {
        return GetRemainingCooldown(ability) > 0f;
    }

    public bool TryUseAbility(AbilityData ability)
    {
        if (ability == null)
        {
            return false;
        }

        if (isCasting)
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

        return CanApplySelfAbility(ability, true);
    }

    private void StartCast(AbilityData ability)
    {
        currentCastingAbility = ability;
        isCasting = true;
        castStartTime = Time.time;
        castStartPosition = transform.position;

        castDuration = ability.CastType switch
        {
            AbilityCastType.CastTime => ability.CastTimeSeconds,
            AbilityCastType.Channel => ability.ChannelDurationSeconds,
            _ => 0f
        };

        if (castDuration <= 0f)
        {
            ExecuteAbility(ability);
            ClearCastState();
            return;
        }

        string castLabel = ability.CastType == AbilityCastType.Channel ? "channeling" : "casting";
        PostSystem($"You begin {castLabel} {ability.DisplayName}.");

        OnCastStarted?.Invoke(ability, castDuration);
    }

    private void UpdateCasting()
    {
        if (!isCasting || currentCastingAbility == null)
        {
            return;
        }

        if (!currentCastingAbility.CanMoveWhileCasting)
        {
            float distanceMoved = Vector3.Distance(castStartPosition, transform.position);

            if (distanceMoved > movementCancelDistance)
            {
                CancelCast("Casting cancelled by movement.");
                return;
            }
        }

        if (Time.time - castStartTime >= castDuration)
        {
            AbilityData completedAbility = currentCastingAbility;

            bool executed = ExecuteAbility(completedAbility);

            if (executed)
            {
                OnCastCompleted?.Invoke(completedAbility);
            }

            ClearCastState();
        }
    }

    private void HandlePlayerDamaged(int amount, GameObject source)
    {
        if (!isCasting || currentCastingAbility == null)
        {
            return;
        }

        if (!currentCastingAbility.CanBeInterrupted)
        {
            return;
        }

        CancelCast("Casting interrupted.");
    }

    private void CancelCast(string message)
    {
        AbilityData cancelledAbility = currentCastingAbility;

        PostSystem(message);

        OnCastCancelled?.Invoke(cancelledAbility);
        ClearCastState();
    }

    private void ClearCastState()
    {
        isCasting = false;
        currentCastingAbility = null;
        castStartTime = 0f;
        castDuration = 0f;
        castStartPosition = Vector3.zero;
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

        bool used;

        if (ability.RequiresTarget)
        {
            used = playerCombat != null &&
                   playerCombat.TryUseAbilityOnCurrentTarget(
                       ability.DisplayName,
                       ability.DamageAmount,
                       ability.Range);
        }
        else
        {
            used = TryApplySelfAbility(ability);
        }

        if (!used)
        {
            return false;
        }

        SpendResourceCost(ability);
        GenerateResourceFromAbility(ability);
        StartCooldown(ability);

        return true;
    }

    private bool CanApplySelfAbility(AbilityData ability, bool postMessages)
    {
        if (ability.HealthRestoreAmount <= 0)
        {
            return true;
        }

        if (playerHealth == null)
        {
            return false;
        }

        if (playerHealth.IsDead)
        {
            if (postMessages)
            {
                PostSystem("You cannot use that while dead.");
            }

            return false;
        }

        if (playerHealth.CurrentHealth >= playerHealth.MaxHealth)
        {
            if (postMessages)
            {
                PostSystem("You are already at full health.");
            }

            return false;
        }

        return true;
    }

    private bool TryApplySelfAbility(AbilityData ability)
    {
        bool didSomething = false;

        if (ability.HealthRestoreAmount > 0)
        {
            if (!CanApplySelfAbility(ability, true))
            {
                return false;
            }

            int finalHealingAmount = playerStats != null
                ? playerStats.ApplyPrimaryStatHealingScaling(ability.HealthRestoreAmount)
                : ability.HealthRestoreAmount;

            int restored = playerHealth.RestoreHealth(finalHealingAmount);

            if (restored > 0)
            {
                didSomething = true;
                PostSystem($"You use {ability.DisplayName} and restore {restored} health.");
            }
        }

        return didSomething;
    }

    private bool CanPayResourceCost(AbilityData ability)
    {
        if (ability.ResourceCost <= 0)
        {
            return true;
        }

        if (playerResource == null || !playerResource.HasResource)
        {
            return false;
        }

        return playerResource.HasEnoughResource(ability.ResourceCost);
    }

    private void SpendResourceCost(AbilityData ability)
    {
        if (ability.ResourceCost <= 0)
        {
            return;
        }

        if (playerResource != null)
        {
            playerResource.TrySpendResource(ability.ResourceCost);
        }
    }

    private void GenerateResourceFromAbility(AbilityData ability)
    {
        if (ability.ResourceGenerated <= 0)
        {
            return;
        }

        if (playerResource != null)
        {
            playerResource.GenerateResource(ability.ResourceGenerated);
        }
    }

    private void StartCooldown(AbilityData ability)
    {
        if (ability.CooldownSeconds > 0f)
        {
            cooldownEndTimes[ability] = Time.time + ability.CooldownSeconds;
        }
    }

    private string GetResourceName()
    {
        if (playerResource == null || !playerResource.HasResource)
        {
            return "resource";
        }

        return playerResource.ResourceDisplayName.ToLower();
    }

    private void PostSystem(string message)
    {
        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem(message);
        }
    }
}