using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerCombat))]
[RequireComponent(typeof(Health))]
public class PlayerAbilityController : MonoBehaviour
{
    private PlayerCombat playerCombat;
    private Health playerHealth;

    private readonly Dictionary<AbilityData, float> cooldownEndTimes = new Dictionary<AbilityData, float>();

    private void Awake()
    {
        playerCombat = GetComponent<PlayerCombat>();
        playerHealth = GetComponent<Health>();
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

        if (IsOnCooldown(ability))
        {
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

        if (ability.HealthRestoreAmount > 0 && playerHealth != null)
        {
            int restored = playerHealth.RestoreHealth(ability.HealthRestoreAmount);
            if (restored > 0)
            {
                didSomething = true;

                if (ChatManager.Instance != null)
                {
                    ChatManager.Instance.PostSystem(
                        $"You use {ability.DisplayName} and restore {restored} health.");
                }
            }
        }

        return didSomething;
    }
}