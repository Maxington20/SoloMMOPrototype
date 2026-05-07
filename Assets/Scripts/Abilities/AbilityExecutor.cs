using UnityEngine;

[RequireComponent(typeof(PlayerCombat))]
[RequireComponent(typeof(Health))]
public class AbilityExecutor : MonoBehaviour
{
    private PlayerCombat playerCombat;
    private PlayerStats playerStats;
    private Health playerHealth;

    private void Awake()
    {
        playerCombat = GetComponent<PlayerCombat>();
        playerStats = GetComponent<PlayerStats>();
        playerHealth = GetComponent<Health>();
    }

    public int CalculateAbilityDamage(AbilityData ability)
    {
        if (ability == null || !ability.DealsDamage || playerCombat == null)
        {
            return 0;
        }

        int scaledAttackDamage = playerCombat.Damage;
        int finalDamage = Mathf.RoundToInt(scaledAttackDamage * ability.DamageMultiplier);

        return Mathf.Max(1, finalDamage);
    }

    public int CalculateAbilityHealing(AbilityData ability)
    {
        if (ability == null || !ability.RestoresHealth)
        {
            return 0;
        }

        float baseHealingPower = GetBaseHealingPower();
        int finalHealing = Mathf.RoundToInt(baseHealingPower * ability.HealingMultiplier);

        return Mathf.Max(1, finalHealing);
    }

    public float GetBaseHealingPower()
    {
        int primaryStatValue = playerStats != null ? playerStats.PrimaryStatValue : 0;

        return playerStats != null
            ? primaryStatValue * playerStats.CombatTuning.PrimaryStatHealingMultiplier
            : primaryStatValue;
    }

    public bool CanExecuteSelfAbility(AbilityData ability, bool postMessages)
    {
        if (ability == null)
        {
            return false;
        }

        if (!ability.RestoresHealth)
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

    public bool ExecuteSelfAbility(AbilityData ability)
    {
        if (!CanExecuteSelfAbility(ability, true))
        {
            return false;
        }

        bool didSomething = false;

        if (ability.RestoresHealth)
        {
            int restoredAmount = CalculateAbilityHealing(ability);
            int restored = playerHealth.RestoreHealth(restoredAmount);

            if (restored > 0)
            {
                didSomething = true;
                PostSystem($"You use {ability.DisplayName} and restore {restored} health.");
            }
        }

        return didSomething;
    }

    public bool ExecuteTargetAbility(AbilityData ability)
    {
        if (ability == null || playerCombat == null)
        {
            return false;
        }

        Health targetHealth = playerCombat.CurrentTargetHealth;

        if (targetHealth == null)
        {
            return false;
        }

        bool didSomething = false;
        int abilityDamage = CalculateAbilityDamage(ability);

        if (ability.DealsDamage)
        {
            QueueCombatFeedback(targetHealth, ability);

            targetHealth.TakeDamage(abilityDamage, gameObject);
            ApplyThreat(targetHealth.gameObject, ability, abilityDamage);

            string targetName = GetTargetDisplayName(targetHealth.gameObject);

            Debug.Log($"Player uses {ability.DisplayName} on {targetName} for {abilityDamage}");
            PostSystem($"You use {ability.DisplayName} on {targetName} for {abilityDamage} damage.");

            didSomething = true;
        }

        if (ApplyStatusEffects(targetHealth, ability, abilityDamage))
        {
            didSomething = true;
        }

        EnemyController enemyTarget = playerCombat.CurrentEnemyTarget;
        if (enemyTarget != null)
        {
            enemyTarget.SetTarget(transform);
        }

        return didSomething;
    }

    private bool ApplyStatusEffects(Health targetHealth, AbilityData ability, int abilityDamage)
    {
        if (targetHealth == null || ability == null || ability.StatusEffects == null || ability.StatusEffects.Length == 0)
        {
            return false;
        }

        StatusEffectController statusController = targetHealth.GetComponent<StatusEffectController>();

        if (statusController == null)
        {
            return false;
        }

        int sourceDamageForEffects = abilityDamage > 0
            ? abilityDamage
            : playerCombat.Damage;

        bool appliedAny = false;

        for (int i = 0; i < ability.StatusEffects.Length; i++)
        {
            StatusEffectData effect = ability.StatusEffects[i];

            if (effect == null)
            {
                continue;
            }

            statusController.ApplyEffect(effect, gameObject, sourceDamageForEffects);
            appliedAny = true;
        }

        return appliedAny;
    }

    private void QueueCombatFeedback(Health targetHealth, AbilityData ability)
    {
        if (targetHealth == null)
        {
            return;
        }

        CombatFeedbackReceiver feedbackReceiver = targetHealth.GetComponent<CombatFeedbackReceiver>();

        if (feedbackReceiver == null)
        {
            return;
        }

        feedbackReceiver.QueueAbilityImpactFeedback(ability);
    }

    private void ApplyThreat(GameObject targetObject, AbilityData ability, int damage)
    {
        if (targetObject == null)
        {
            return;
        }

        ThreatTable threatTable = targetObject.GetComponent<ThreatTable>();
        if (threatTable == null)
        {
            return;
        }

        float threat = damage;

        if (ability != null)
        {
            threat *= ability.ThreatMultiplier;
            threat += ability.BonusThreat;

            if (ability.IsTaunt)
            {
                threatTable.SetHighestThreat(gameObject, 50f);
                return;
            }
        }

        threatTable.AddThreat(gameObject, threat);
    }

    private string GetTargetDisplayName(GameObject targetObject)
    {
        if (targetObject == null)
        {
            return "target";
        }

        DisplayName displayName = targetObject.GetComponent<DisplayName>();
        return displayName != null ? displayName.Display : targetObject.name;
    }

    private void PostSystem(string message)
    {
        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem(message);
        }
    }
}