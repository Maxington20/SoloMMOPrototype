using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(PlayerTargetingController))]
[RequireComponent(typeof(PlayerAutoAttackController))]
[RequireComponent(typeof(AbilityTargetValidator))]
public class PlayerCombat : MonoBehaviour
{
    private PlayerTargetingController targetingController;
    private PlayerAutoAttackController autoAttackController;
    private AbilityTargetValidator targetValidator;

    public Transform CurrentTargetTransform => targetingController != null
        ? targetingController.CurrentTargetTransform
        : null;

    public Health CurrentTargetHealth => targetingController != null
        ? targetingController.CurrentTargetHealth
        : null;

    public EnemyController CurrentEnemyTarget => targetingController != null
        ? targetingController.CurrentEnemyTarget
        : null;

    public int Damage => autoAttackController != null
        ? autoAttackController.Damage
        : 0;

    public float CurrentAttackRange => autoAttackController != null
        ? autoAttackController.CurrentAttackRange
        : 0f;

    public bool CurrentAutoAttackIsMelee => autoAttackController == null || autoAttackController.CurrentAutoAttackIsMelee;

    private void Awake()
    {
        targetingController = GetComponent<PlayerTargetingController>();
        autoAttackController = GetComponent<PlayerAutoAttackController>();
        targetValidator = GetComponent<AbilityTargetValidator>();
    }

    public void SetBaseDamage(int amount)
    {
        if (autoAttackController != null)
        {
            autoAttackController.SetBaseDamage(amount);
        }
    }

    public void IncreaseDamage(int amount)
    {
        if (autoAttackController != null)
        {
            autoAttackController.IncreaseDamage(amount);
        }
    }

    public void SetEquipmentBonusDamage(int amount)
    {
        if (autoAttackController != null)
        {
            autoAttackController.SetEquipmentBonusDamage(amount);
        }
    }

    public int CalculateAbilityDamage(AbilityData ability)
    {
        AbilityExecutor executor = GetComponent<AbilityExecutor>();

        return executor != null
            ? executor.CalculateAbilityDamage(ability)
            : 0;
    }

    public bool CanUseAbilityOnCurrentTarget(string abilityName, float range, bool postMessages)
    {
        return targetValidator != null &&
               targetValidator.CanUseAbilityOnCurrentTarget(
                   abilityName,
                   range,
                   postMessages);
    }

    public bool TryUseAbilityOnCurrentTarget(AbilityData ability)
    {
        if (ability == null)
        {
            return false;
        }

        if (!CanUseAbilityOnCurrentTarget(ability.DisplayName, ability.Range, true))
        {
            return false;
        }

        FaceCurrentTarget();

        AbilityExecutor executor = GetComponent<AbilityExecutor>();

        if (executor == null)
        {
            return false;
        }

        return executor.ExecuteTargetAbility(ability);
    }

    public void FaceCurrentTarget()
    {
        if (targetValidator != null)
        {
            targetValidator.FaceCurrentTarget();
        }
    }

    public void ClearTarget()
    {
        if (targetValidator != null)
        {
            targetValidator.ClearTarget();
        }
        else if (targetingController != null)
        {
            targetingController.ClearTarget();
        }
    }
}