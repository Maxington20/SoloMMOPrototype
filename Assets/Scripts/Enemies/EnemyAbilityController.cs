using System.Collections.Generic;
using UnityEngine;

public class EnemyAbilityController : MonoBehaviour
{
    [Header("Abilities")]
    [SerializeField] private AbilityData[] abilities;

    private Dictionary<AbilityData, float> cooldowns = new Dictionary<AbilityData, float>();

    private PlayerCombat playerCombat;
    private StatusEffectController statusController;

    private void Awake()
    {
        statusController = GetComponent<StatusEffectController>();
    }

    private void Update()
    {
        UpdateCooldowns();
    }

    public bool TryUseAbility(Transform target)
    {
        if (target == null || abilities == null || abilities.Length == 0)
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

            float distance = Vector3.Distance(transform.position, target.position);

            if (distance > ability.Range)
            {
                continue;
            }

            UseAbility(ability, target);
            return true;
        }

        return false;
    }

    private void UseAbility(AbilityData ability, Transform target)
    {
        Health targetHealth = target.GetComponent<Health>();

        if (targetHealth == null || targetHealth.IsDead)
        {
            return;
        }

        int damage = 0;

        if (ability.DealsDamage)
        {
            EnemyStats enemyStats = GetComponent<EnemyStats>();
            int baseDamage = enemyStats != null ? enemyStats.GetScaledDamage() : 10;

            damage = Mathf.RoundToInt(baseDamage * ability.DamageMultiplier);
            targetHealth.TakeDamage(damage, gameObject);
        }

        ApplyStatusEffects(ability, target, damage);

        StartCooldown(ability);
    }

    private void ApplyStatusEffects(AbilityData ability, Transform target, int damage)
    {
        if (ability.StatusEffects == null || ability.StatusEffects.Length == 0)
        {
            return;
        }

        StatusEffectController controller = target.GetComponent<StatusEffectController>();

        if (controller == null)
        {
            return;
        }

        int baseDamage = damage > 0 ? damage : 10;

        for (int i = 0; i < ability.StatusEffects.Length; i++)
        {
            StatusEffectData effect = ability.StatusEffects[i];

            if (effect == null)
            {
                continue;
            }

            controller.ApplyEffect(effect, gameObject, baseDamage);
        }
    }

    private void StartCooldown(AbilityData ability)
    {
        cooldowns[ability] = Time.time + ability.CooldownSeconds;
    }

    private bool IsOnCooldown(AbilityData ability)
    {
        if (!cooldowns.ContainsKey(ability))
        {
            return false;
        }

        return Time.time < cooldowns[ability];
    }

    private void UpdateCooldowns()
    {
        // no-op for now (we check Time.time directly)
    }
}