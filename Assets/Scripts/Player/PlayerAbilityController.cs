using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerCombat))]
[RequireComponent(typeof(Health))]
public class PlayerAbilityController : MonoBehaviour
{
    [Header("Stat Scaling")]
    [SerializeField] private float primaryStatHealingMultiplier = 1f;

    private PlayerCombat playerCombat;
    private Health playerHealth;
    private PlayerStats playerStats;

    private readonly Dictionary<AbilityData, float> cooldownEndTimes = new Dictionary<AbilityData, float>();

    private void Awake()
    {
        playerCombat = GetComponent<PlayerCombat>();
        playerHealth = GetComponent<Health>();
        playerStats = GetComponent<PlayerStats>();
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

        float cooldownRemaining = GetRemainingCooldown(ability);
        if (cooldownRemaining > 0f)
        {
            PostSystem($"{ability.DisplayName} is on cooldown for {Mathf.CeilToInt(cooldownRemaining)} more second(s).");
            return false;
        }

        bool used = false;

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

        if (ability.CooldownSeconds > 0f)
        {
            cooldownEndTimes[ability] = Time.time + ability.CooldownSeconds;
        }

        return true;
    }

    private bool TryApplySelfAbility(AbilityData ability)
    {
        bool didSomething = false;

        if (ability.HealthRestoreAmount > 0)
        {
            if (playerHealth == null)
            {
                return false;
            }

            if (playerHealth.IsDead)
            {
                PostSystem("You cannot use that while dead.");
                return false;
            }

            if (playerHealth.CurrentHealth >= playerHealth.MaxHealth)
            {
                PostSystem("You are already at full health.");
                return false;
            }

            int finalHealingAmount = CalculateHealingAmount(ability.HealthRestoreAmount);
            int restored = playerHealth.RestoreHealth(finalHealingAmount);

            if (restored > 0)
            {
                didSomething = true;
                PostSystem($"You use {ability.DisplayName} and restore {restored} health.");
            }
        }

        return didSomething;
    }

    private int CalculateHealingAmount(int baseHealingAmount)
    {
        int statBonus = playerStats != null
            ? Mathf.RoundToInt(playerStats.PrimaryStatValue * primaryStatHealingMultiplier)
            : 0;

        return Mathf.Max(0, baseHealingAmount + statBonus);
    }

    private void PostSystem(string message)
    {
        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem(message);
        }
    }
}