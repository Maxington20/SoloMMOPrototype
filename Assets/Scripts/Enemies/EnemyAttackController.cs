using UnityEngine;

[RequireComponent(typeof(EnemyStats))]
[RequireComponent(typeof(StatusEffectController))]
public class EnemyAttackController : MonoBehaviour
{
    [Header("Basic Attack")]
    [Tooltip("Legacy fallback damage. EnemyData Base Damage is used when EnemyStats is present.")]
    [SerializeField] private int fallbackDamage = 10;

    [SerializeField] private float attackCooldown = 1.5f;

    private EnemyStats enemyStats;
    private StatusEffectController statusEffectController;

    private float lastAttackTime;

    private void Awake()
    {
        enemyStats = GetComponent<EnemyStats>();
        statusEffectController = GetComponent<StatusEffectController>();
    }

    public bool TryBasicAttack(Transform target)
    {
        if (target == null)
        {
            return false;
        }

        if (IsStunned())
        {
            return false;
        }

        if (Time.time - lastAttackTime < attackCooldown)
        {
            return false;
        }

        Health targetHealth = target.GetComponent<Health>();

        if (targetHealth == null || targetHealth.IsDead)
        {
            return false;
        }

        lastAttackTime = Time.time;

        int finalDamage = enemyStats != null
            ? enemyStats.GetScaledDamage()
            : fallbackDamage;

        Debug.Log($"{gameObject.name} attacks {target.name} for {finalDamage}");
        targetHealth.TakeDamage(finalDamage, gameObject);

        return true;
    }

    public void ResetAttackTimer()
    {
        lastAttackTime = 0f;
    }

    private bool IsStunned()
    {
        return statusEffectController != null && statusEffectController.IsStunned;
    }
}