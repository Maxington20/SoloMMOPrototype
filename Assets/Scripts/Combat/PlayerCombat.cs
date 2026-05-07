using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(PlayerTargetingController))]
[RequireComponent(typeof(PlayerAutoAttackController))]
public class PlayerCombat : MonoBehaviour
{
    private PlayerTargetingController targetingController;
    private PlayerAutoAttackController autoAttackController;

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
        Health currentTarget = CurrentTargetHealth;

        if (currentTarget == null)
        {
            if (postMessages)
            {
                PostSystem("No target.");
            }

            return false;
        }

        if (currentTarget.IsDead)
        {
            ClearTarget();

            if (postMessages)
            {
                PostSystem("Target is dead.");
            }

            return false;
        }

        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);
        if (distanceToTarget > range)
        {
            if (postMessages)
            {
                PostSystem($"{abilityName} is out of range.");
            }

            return false;
        }

        return true;
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
        Transform currentTargetTransform = CurrentTargetTransform;

        if (currentTargetTransform == null)
        {
            return;
        }

        FaceTarget(currentTargetTransform);
    }

    public void ClearTarget()
    {
        if (targetingController != null)
        {
            targetingController.ClearTarget();
        }
    }

    private void FaceTarget(Transform target)
    {
        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            10f * Time.deltaTime);
    }

    private void PostSystem(string message)
    {
        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem(message);
        }
    }
}