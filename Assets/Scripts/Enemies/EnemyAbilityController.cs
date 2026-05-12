using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyData))]
[RequireComponent(typeof(EnemyStats))]
[RequireComponent(typeof(StatusEffectController))]
public class EnemyAbilityController : MonoBehaviour
{
    [Header("Fallback Abilities")]
    [SerializeField] private AbilityData[] fallbackAbilities = new AbilityData[0];

    private readonly Dictionary<AbilityData, float> cooldownEndTimes = new Dictionary<AbilityData, float>();

    private EnemyData enemyData;
    private EnemyStats enemyStats;
    private StatusEffectController statusEffectController;

    private bool isCasting;
    private AbilityData currentCastingAbility;
    private Transform currentCastTarget;
    private float castStartTime;
    private float castDuration;

    public bool IsCasting => isCasting;
    public AbilityData CurrentCastingAbility => currentCastingAbility;

    public float CastProgress => !isCasting || castDuration <= 0f
        ? 0f
        : Mathf.Clamp01((Time.time - castStartTime) / castDuration);

    public float CastRemainingTime => !isCasting || castDuration <= 0f
        ? 0f
        : Mathf.Max(0f, castDuration - (Time.time - castStartTime));

    public event Action<AbilityData, float> OnCastStarted;
    public event Action<AbilityData> OnCastCompleted;
    public event Action<AbilityData> OnCastCancelled;

    private void Awake()
    {
        enemyData = GetComponent<EnemyData>();
        enemyStats = GetComponent<EnemyStats>();
        statusEffectController = GetComponent<StatusEffectController>();
    }

    private void Update()
    {
        UpdateCasting();
    }

    public bool TryUseAbility(Transform target)
    {
        if (isCasting)
        {
            return true;
        }

        AbilityData[] abilities = GetActiveAbilities();

        if (target == null || abilities == null || abilities.Length == 0)
        {
            return false;
        }

        if (IsStunned())
        {
            return false;
        }

        for (int i = 0; i < abilities.Length; i++)
        {
            AbilityData ability = abilities[i];

            if (ability == null)
            {
                continue;
            }

            if (IsOnCooldown(ability))
            {
                continue;
            }

            if (!IsTargetValidForAbility(ability, target))
            {
                continue;
            }

            if (ability.IsInstant)
            {
                UseAbilityImmediately(ability, target);
                return true;
            }

            StartCast(ability, target);
            return true;
        }

        return false;
    }

    private AbilityData[] GetActiveAbilities()
    {
        if (enemyData != null && enemyData.Abilities != null && enemyData.Abilities.Length > 0)
        {
            return enemyData.Abilities;
        }

        return fallbackAbilities;
    }

    private void StartCast(AbilityData ability, Transform target)
    {
        currentCastingAbility = ability;
        currentCastTarget = target;
        isCasting = true;
        castStartTime = Time.time;

        castDuration = ability.CastType switch
        {
            AbilityCastType.CastTime => ability.CastTimeSeconds,
            AbilityCastType.Channel => ability.ChannelDurationSeconds,
            _ => 0f
        };

        if (castDuration <= 0f)
        {
            UseAbilityImmediately(ability, target);
            ClearCastState();
            return;
        }

        PostSystem($"{GetDisplayName()} begins casting {ability.DisplayName}.");
        OnCastStarted?.Invoke(ability, castDuration);
    }

    private void UpdateCasting()
    {
        if (!isCasting || currentCastingAbility == null)
        {
            return;
        }

        if (IsStunned() && currentCastingAbility.CanBeInterrupted)
        {
            CancelCast($"{GetDisplayName()}'s {currentCastingAbility.DisplayName} was interrupted.");
            return;
        }

        if (currentCastTarget == null)
        {
            CancelCast($"{GetDisplayName()}'s cast was cancelled.");
            return;
        }

        Health targetHealth = currentCastTarget.GetComponent<Health>();
        if (targetHealth == null || targetHealth.IsDead)
        {
            CancelCast($"{GetDisplayName()}'s cast was cancelled.");
            return;
        }

        if (Time.time - castStartTime < castDuration)
        {
            return;
        }

        AbilityData completedAbility = currentCastingAbility;
        Transform completedTarget = currentCastTarget;

        if (IsTargetValidForAbility(completedAbility, completedTarget))
        {
            UseAbilityImmediately(completedAbility, completedTarget);
            OnCastCompleted?.Invoke(completedAbility);
        }
        else
        {
            OnCastCancelled?.Invoke(completedAbility);
        }

        ClearCastState();
    }

    private void UseAbilityImmediately(AbilityData ability, Transform target)
    {
        if (ability == null || target == null)
        {
            return;
        }

        Health targetHealth = target.GetComponent<Health>();
        if (targetHealth == null || targetHealth.IsDead)
        {
            return;
        }

        int abilityDamage = CalculateAbilityDamage(ability);

        if (ability.DealsDamage)
        {
            targetHealth.TakeDamage(abilityDamage, gameObject);
            PostSystem($"{GetDisplayName()} uses {ability.DisplayName}.");
        }

        ApplyStatusEffects(ability, target, abilityDamage);
        StartCooldown(ability);
    }

    private int CalculateAbilityDamage(AbilityData ability)
    {
        if (ability == null || !ability.DealsDamage)
        {
            return 0;
        }

        int baseDamage = enemyStats != null ? enemyStats.GetScaledDamage() : 10;
        int finalDamage = Mathf.RoundToInt(baseDamage * ability.DamageMultiplier);

        return Mathf.Max(1, finalDamage);
    }

    private void ApplyStatusEffects(AbilityData ability, Transform target, int damage)
    {
        if (ability == null || ability.StatusEffects == null || ability.StatusEffects.Length == 0)
        {
            return;
        }

        StatusEffectController targetStatusController = target.GetComponent<StatusEffectController>();

        if (targetStatusController == null)
        {
            return;
        }

        int sourceDamageForEffects = damage > 0
            ? damage
            : enemyStats != null ? enemyStats.GetScaledDamage() : 10;

        for (int i = 0; i < ability.StatusEffects.Length; i++)
        {
            StatusEffectData effect = ability.StatusEffects[i];

            if (effect == null)
            {
                continue;
            }

            targetStatusController.ApplyEffect(effect, gameObject, sourceDamageForEffects);
        }
    }

    private bool IsTargetValidForAbility(AbilityData ability, Transform target)
    {
        if (ability == null || target == null)
        {
            return false;
        }

        Health targetHealth = target.GetComponent<Health>();
        if (targetHealth == null || targetHealth.IsDead)
        {
            return false;
        }

        float distance = Vector3.Distance(transform.position, target.position);
        return distance <= ability.Range;
    }

    private bool IsOnCooldown(AbilityData ability)
    {
        if (ability == null)
        {
            return true;
        }

        if (!cooldownEndTimes.TryGetValue(ability, out float endTime))
        {
            return false;
        }

        return Time.time < endTime;
    }

    private void StartCooldown(AbilityData ability)
    {
        if (ability == null || ability.CooldownSeconds <= 0f)
        {
            return;
        }

        cooldownEndTimes[ability] = Time.time + ability.CooldownSeconds;
    }

    private bool IsStunned()
    {
        return statusEffectController != null && statusEffectController.IsStunned;
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
        currentCastTarget = null;
        castStartTime = 0f;
        castDuration = 0f;
    }

    private string GetDisplayName()
    {
        DisplayName displayName = GetComponent<DisplayName>();
        return displayName != null ? displayName.Display : gameObject.name;
    }

    private void PostSystem(string message)
    {
        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem(message);
        }
    }
}