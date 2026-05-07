using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(EnemyData))]
[RequireComponent(typeof(EnemyStats))]
[RequireComponent(typeof(StatusEffectController))]
[RequireComponent(typeof(ThreatTable))]
[RequireComponent(typeof(EnemyDeathRespawnController))]
[RequireComponent(typeof(EnemyMovementController))]
public class EnemyController : MonoBehaviour
{
    [SerializeField] private float aggroRange = 8f;
    [SerializeField] private float leashRange = 14f;
    [SerializeField] private float attackRange = 1.5f;

    [Tooltip("Legacy fallback damage. EnemyData Base Damage is used when EnemyStats is present.")]
    [SerializeField] private int damage = 10;

    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float returnStopDistance = 0.2f;

    private Transform target;
    private Transform player;
    private float lastAttackTime;

    private Health health;
    private EnemyStats enemyStats;
    private StatusEffectController statusEffectController;
    private EnemyAbilityController abilityController;
    private ThreatTable threatTable;
    private EnemyDeathRespawnController deathRespawnController;
    private EnemyMovementController movementController;

    private Vector3 homePosition;
    private Quaternion homeRotation;
    private bool isReturningHome;

    private void Awake()
    {
        health = GetComponent<Health>();
        enemyStats = GetComponent<EnemyStats>();
        statusEffectController = GetComponent<StatusEffectController>();
        abilityController = GetComponent<EnemyAbilityController>();
        threatTable = GetComponent<ThreatTable>();
        deathRespawnController = GetComponent<EnemyDeathRespawnController>();
        movementController = GetComponent<EnemyMovementController>();

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

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
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

        AcquireTargetIfNeeded();

        if (target != null)
        {
            GameObject highestThreat = threatTable != null
                ? threatTable.GetHighestThreatTarget()
                : null;

            if (highestThreat != null)
            {
                target = highestThreat.transform;
            }

            float distanceToTarget = Vector3.Distance(transform.position, target.position);

            if (distanceToTarget > leashRange)
            {
                DropAggroAndReturnHome();
                return;
            }

            if (distanceToTarget > attackRange)
            {
                MoveTowardPosition(target.position);
            }
            else
            {
                FacePosition(target.position);
                TryAttack();
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

        target = newTarget;
        isReturningHome = false;

        if (movementController != null)
        {
            movementController.ClearWanderDestination();
        }

        if (threatTable != null && newTarget != null)
        {
            threatTable.AddThreat(newTarget.gameObject, 1f);
        }
    }

    private void AcquireTargetIfNeeded()
    {
        if (threatTable != null)
        {
            GameObject highestThreat = threatTable.GetHighestThreatTarget();

            if (highestThreat != null)
            {
                target = highestThreat.transform;
                return;
            }
        }

        if (target != null || player == null)
        {
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= aggroRange)
        {
            target = player;
            isReturningHome = false;

            if (movementController != null)
            {
                movementController.ClearWanderDestination();
            }

            if (threatTable != null)
            {
                threatTable.AddThreat(player.gameObject, 1f);
            }
        }
    }

    private void DropAggroAndReturnHome()
    {
        target = null;
        isReturningHome = true;

        if (movementController != null)
        {
            movementController.ClearWanderDestination();
        }

        if (threatTable != null)
        {
            threatTable.ClearAll();
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

    private void TryAttack()
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

        target = null;
        isReturningHome = false;
        lastAttackTime = 0f;

        if (movementController != null)
        {
            movementController.ResetVerticalVelocity();
            movementController.ClearWanderDestination();
        }

        if (threatTable != null)
        {
            threatTable.ClearAll();
        }

        if (deathRespawnController != null)
        {
            deathRespawnController.BeginRespawn();
        }
    }
}