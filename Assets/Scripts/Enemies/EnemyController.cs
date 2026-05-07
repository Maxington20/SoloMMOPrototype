using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(EnemyData))]
[RequireComponent(typeof(EnemyStats))]
[RequireComponent(typeof(StatusEffectController))]
[RequireComponent(typeof(ThreatTable))]
[RequireComponent(typeof(EnemyDeathRespawnController))]
[RequireComponent(typeof(EnemyMovementController))]
[RequireComponent(typeof(EnemyAggroController))]
public class EnemyController : MonoBehaviour
{
    [Header("Combat")]
    [SerializeField] private float attackRange = 1.5f;

    [Tooltip("Legacy fallback damage. EnemyData Base Damage is used when EnemyStats is present.")]
    [SerializeField] private int damage = 10;

    [SerializeField] private float attackCooldown = 1.5f;

    [Header("Return Home")]
    [SerializeField] private float returnStopDistance = 0.2f;

    private float lastAttackTime;

    private Health health;
    private EnemyStats enemyStats;
    private StatusEffectController statusEffectController;
    private EnemyAbilityController abilityController;
    private EnemyDeathRespawnController deathRespawnController;
    private EnemyMovementController movementController;
    private EnemyAggroController aggroController;

    private Vector3 homePosition;
    private Quaternion homeRotation;
    private bool isReturningHome;

    private Transform CurrentTarget => aggroController != null
        ? aggroController.CurrentTarget
        : null;

    private void Awake()
    {
        health = GetComponent<Health>();
        enemyStats = GetComponent<EnemyStats>();
        statusEffectController = GetComponent<StatusEffectController>();
        abilityController = GetComponent<EnemyAbilityController>();
        deathRespawnController = GetComponent<EnemyDeathRespawnController>();
        movementController = GetComponent<EnemyMovementController>();
        aggroController = GetComponent<EnemyAggroController>();

        homePosition = transform.position;
        homeRotation = transform.rotation;

        if (deathRespawnController != null)
        {
            deathRespawnController.ForceHomePosition(homePosition, homeRotation);
        }

        if (movementController != null)
        {
            movementController.SetHomePosition(homePosition);
        }
    }

    private void OnEnable()
    {
        health.OnDied += HandleDeath;
    }

    private void OnDisable()
    {
        health.OnDied -= HandleDeath;
    }

    private void Update()
    {
        if (deathRespawnController != null && deathRespawnController.IsRespawning)
        {
            return;
        }

        if (health.IsDead)
        {
            return;
        }

        if (movementController != null)
        {
            movementController.TickGravity();
        }

        if (IsStunned())
        {
            return;
        }

        if (abilityController != null && abilityController.IsCasting)
        {
            FaceCurrentTarget();
            return;
        }

        if (isReturningHome)
        {
            ReturnHome();
            return;
        }

        if (aggroController != null)
        {
            aggroController.TickTargetSelection();
        }

        Transform target = CurrentTarget;

        if (target != null)
        {
            if (aggroController != null && aggroController.ShouldLeashFromCurrentTarget(transform.position))
            {
                DropAggroAndReturnHome();
                return;
            }

            float distanceToTarget = Vector3.Distance(transform.position, target.position);

            if (distanceToTarget > attackRange)
            {
                MoveTowardPosition(target.position);
            }
            else
            {
                FacePosition(target.position);
                TryAttack(target);
            }

            return;
        }

        if (movementController != null)
        {
            movementController.TickWandering();
        }
    }

    public void SetTarget(Transform newTarget)
    {
        if (health.IsDead || deathRespawnController != null && deathRespawnController.IsRespawning)
        {
            return;
        }

        isReturningHome = false;

        if (movementController != null)
        {
            movementController.ClearWanderDestination();
        }

        if (aggroController != null)
        {
            aggroController.SetTarget(newTarget, 1f);
        }
    }

    private void DropAggroAndReturnHome()
    {
        isReturningHome = true;

        if (movementController != null)
        {
            movementController.ClearWanderDestination();
        }

        if (aggroController != null)
        {
            aggroController.ClearTargetAndThreat();
        }
    }

    private void ReturnHome()
    {
        float distanceToHome = Vector3.Distance(transform.position, homePosition);

        if (distanceToHome <= returnStopDistance)
        {
            isReturningHome = false;
            transform.position = homePosition;
            transform.rotation = homeRotation;

            if (movementController != null)
            {
                movementController.ClearWanderDestination();
                movementController.ResetWanderTimer();
                movementController.ResetVerticalVelocity();
            }

            if (enemyStats != null)
            {
                enemyStats.RecalculateAndApplyStats(true);
            }
            else
            {
                health.ResetHealth();
            }

            return;
        }

        MoveTowardPosition(homePosition);
    }

    private void MoveTowardPosition(Vector3 destination)
    {
        if (movementController != null)
        {
            movementController.MoveTowardPosition(destination);
        }
    }

    private void FaceCurrentTarget()
    {
        Transform target = CurrentTarget;

        if (target == null)
        {
            return;
        }

        FacePosition(target.position);
    }

    private void FacePosition(Vector3 destination)
    {
        if (movementController != null)
        {
            movementController.FacePosition(destination);
        }
    }

    private void TryAttack(Transform target)
    {
        if (IsStunned())
        {
            return;
        }

        if (target == null)
        {
            return;
        }

        if (abilityController != null)
        {
            bool usedAbility = abilityController.TryUseAbility(target);

            if (usedAbility)
            {
                return;
            }
        }

        if (Time.time - lastAttackTime < attackCooldown)
        {
            return;
        }

        lastAttackTime = Time.time;

        Health targetHealth = target.GetComponent<Health>();

        if (targetHealth != null && !targetHealth.IsDead)
        {
            int finalDamage = enemyStats != null
                ? enemyStats.GetScaledDamage()
                : damage;

            Debug.Log($"{gameObject.name} attacks {target.name} for {finalDamage}");
            targetHealth.TakeDamage(finalDamage, gameObject);
        }
    }

    private bool IsStunned()
    {
        return statusEffectController != null && statusEffectController.IsStunned;
    }

    private void HandleDeath()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.RegisterEnemyKilled(gameObject, health.LastDamageSource);
        }

        EnemyLoot enemyLoot = GetComponent<EnemyLoot>();

        if (health.LastDamageSource != null && health.LastDamageSource.CompareTag("Player"))
        {
            PlayerProgression playerProgression = health.LastDamageSource.GetComponent<PlayerProgression>();
            EnemyData enemyData = GetComponent<EnemyData>();

            if (playerProgression != null && enemyData != null)
            {
                playerProgression.AddXp(enemyData.XpReward);
            }

            if (enemyLoot != null)
            {
                enemyLoot.GenerateLoot();
            }
        }

        DisplayName displayName = GetComponent<DisplayName>();
        string enemyName = displayName != null ? displayName.Display : gameObject.name;

        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem($"{enemyName} was slain.");
        }

        isReturningHome = false;
        lastAttackTime = 0f;

        if (movementController != null)
        {
            movementController.ResetVerticalVelocity();
            movementController.ClearWanderDestination();
        }

        if (aggroController != null)
        {
            aggroController.ClearTargetAndThreat();
        }

        if (deathRespawnController != null)
        {
            deathRespawnController.BeginRespawn();
        }
    }
}