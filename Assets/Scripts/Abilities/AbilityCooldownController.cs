using System.Collections.Generic;
using UnityEngine;

public class AbilityCooldownController : MonoBehaviour
{
    private readonly Dictionary<AbilityData, float> cooldownEndTimes = new Dictionary<AbilityData, float>();

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

    public void StartCooldown(AbilityData ability)
    {
        if (ability == null || ability.CooldownSeconds <= 0f)
        {
            return;
        }

        cooldownEndTimes[ability] = Time.time + ability.CooldownSeconds;
    }

    public void ClearCooldown(AbilityData ability)
    {
        if (ability == null)
        {
            return;
        }

        cooldownEndTimes.Remove(ability);
    }

    public void ClearAllCooldowns()
    {
        cooldownEndTimes.Clear();
    }
}