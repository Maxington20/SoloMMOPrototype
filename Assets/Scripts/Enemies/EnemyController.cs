using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(CharacterController))]
public class EnemyController : MonoBehaviour
{
    [SerializeField] private float aggroRange = 8f;
    [SerializeField] private float leashRange = 14f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float returnStopDistance = 0.2f;

    [Header("Wandering")]
    [SerializeField] private float wanderRadius = 3f;
    [SerializeField] private float wanderStopDistance = 0.2f;
    [SerializeField] private float minIdleTimeBetweenWanders = 1.5f;
    [SerializeField] private float maxIdleTimeBetweenWanders = 4f;

    [Header("Respawn")]
    [SerializeField] private float respawnDelay = 8f;
    [SerializeField] private Renderer[] renderersToHideOnDeath;
    [SerializeField] private GameObject[] objectsToHideOnDeath;

    [Header("Corpse / Loot")]
    [SerializeField] private bool hideBodyOnDeath = false;

    private Transform target;
    private Transform player;
    private float lastAttackTime;
    private Vector3 verticalVelocity;

    private Health health;
    private CharacterController characterController;

    private Vector3 homePosition;
    private Quaternion homeRotation;
    private bool isReturningHome;

    private bool isRespawning;
    private float respawnTimer;

    private Vector3 wanderDestination;
    private bool hasWanderDestination;
    private float wanderIdleTimer;

    private void Awake()
    {
        health = GetComponent<Health>();
        characterController = GetComponent<CharacterController>();

        homePosition = transform.position;
        homeRotation = transform.rotation;
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

        if (renderersToHideOnDeath == null || renderersToHideOnDeath.Length == 0)
        {
            renderersToHideOnDeath = GetComponentsInChildren<Renderer>();
        }

        ResetWanderTimer();
    }

    private void Update()
    {
        if (isRespawning)
        {
            HandleRespawnCountdown();
            return;
        }

        if (health.IsDead)
        {
            return;
        }

        HandleGravity();

        if (isReturningHome)
        {
            ReturnHome();
            return;
        }

        AcquireTargetIfNeeded();

        if (target != null)
        {
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

        HandleWandering();
    }

    public void SetTarget(Transform newTarget)
    {
        if (health.IsDead || isRespawning)
        {
            return;
        }

        target = newTarget;
        isReturningHome = false;
        hasWanderDestination = false;
    }

    private void AcquireTargetIfNeeded()
    {
        if (target != null || player == null)
        {
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= aggroRange)
        {
            target = player;
            isReturningHome = false;
            hasWanderDestination = false;
        }
    }

    private void DropAggroAndReturnHome()
    {
        target = null;
        isReturningHome = true;
        hasWanderDestination = false;
    }

    private void ReturnHome()
    {
        float distanceToHome = Vector3.Distance(transform.position, homePosition);

        if (distanceToHome <= returnStopDistance)
        {
            isReturningHome = false;
            transform.position = homePosition;
            transform.rotation = homeRotation;
            hasWanderDestination = false;
            ResetWanderTimer();
            return;
        }

        MoveTowardPosition(homePosition);
    }

    private void HandleWandering()
    {
        if (hasWanderDestination)
        {
            float distanceToDestination = Vector3.Distance(transform.position, wanderDestination);

            if (distanceToDestination <= wanderStopDistance)
            {
                hasWanderDestination = false;
                ResetWanderTimer();
                return;
            }

            MoveTowardPosition(wanderDestination);
            return;
        }

        wanderIdleTimer -= Time.deltaTime;

        if (wanderIdleTimer <= 0f)
        {
            PickNewWanderDestination();
        }
    }

    private void PickNewWanderDestination()
    {
        Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
        wanderDestination = homePosition + new Vector3(randomCircle.x, 0f, randomCircle.y);
        hasWanderDestination = true;
    }

    private void ResetWanderTimer()
    {
        wanderIdleTimer = Random.Range(minIdleTimeBetweenWanders, maxIdleTimeBetweenWanders);
    }

    private void MoveTowardPosition(Vector3 destination)
    {
        Vector3 direction = destination - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
        {
            return;
        }

        direction.Normalize();

        Vector3 movement = direction * moveSpeed;
        characterController.Move(movement * Time.deltaTime);

        FaceDirection(direction);
    }

    private void FacePosition(Vector3 destination)
    {
        Vector3 direction = destination - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
        {
            return;
        }

        FaceDirection(direction.normalized);
    }

    private void FaceDirection(Vector3 direction)
    {
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            10f * Time.deltaTime);
    }

    private void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown)
        {
            return;
        }

        lastAttackTime = Time.time;

        Health targetHealth = target.GetComponent<Health>();
        if (targetHealth != null && !targetHealth.IsDead)
        {
            Debug.Log($"{gameObject.name} attacks {target.name} for {damage}");
            targetHealth.TakeDamage(damage, gameObject);
        }
    }

    private void HandleGravity()
    {
        if (characterController.isGrounded && verticalVelocity.y < 0f)
        {
            verticalVelocity.y = -2f;
        }

        verticalVelocity.y += gravity * Time.deltaTime;
        characterController.Move(verticalVelocity * Time.deltaTime);
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

        DisableCollisionOnDeath(enemyLoot != null ? enemyLoot.LootClickCollider : null);

        DisplayName displayName = GetComponent<DisplayName>();
        string enemyName = displayName != null ? displayName.Display : gameObject.name;

        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem($"{enemyName} was slain.");
        }

        target = null;
        isReturningHome = false;
        isRespawning = true;
        respawnTimer = respawnDelay;
        hasWanderDestination = false;

        if (hideBodyOnDeath)
        {
            SetVisible(false);
        }
    }

    private void HandleRespawnCountdown()
    {
        respawnTimer -= Time.deltaTime;

        if (respawnTimer > 0f)
        {
            return;
        }

        Respawn();
    }

    private void Respawn()
    {
        isRespawning = false;
        target = null;
        isReturningHome = false;
        lastAttackTime = 0f;
        verticalVelocity = Vector3.zero;
        hasWanderDestination = false;

        transform.position = homePosition;
        transform.rotation = homeRotation;

        EnemyLoot enemyLoot = GetComponent<EnemyLoot>();
        if (enemyLoot != null)
        {
            enemyLoot.ResetLoot();
        }

        ReenableCollisionAfterRespawn();

        health.ResetHealth();
        SetVisible(true);

        characterController.enabled = true;
        ResetWanderTimer();
    }

    private void SetVisible(bool visible)
    {
        if (renderersToHideOnDeath != null)
        {
            foreach (Renderer renderer in renderersToHideOnDeath)
            {
                if (renderer != null)
                {
                    renderer.enabled = visible;
                }
            }
        }

        if (objectsToHideOnDeath != null)
        {
            foreach (GameObject obj in objectsToHideOnDeath)
            {
                if (obj != null)
                {
                    obj.SetActive(visible);
                }
            }
        }
    }

    private void DisableCollisionOnDeath(Collider colliderToKeepEnabled)
    {
        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        foreach (Collider col in colliders)
        {
            if (col == null)
            {
                continue;
            }

            if (colliderToKeepEnabled != null && col == colliderToKeepEnabled)
            {
                col.enabled = true;
                continue;
            }

            col.enabled = false;
        }

        if (characterController != null)
        {
            characterController.enabled = false;
        }

        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = false;
        }
    }

    private void ReenableCollisionAfterRespawn()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        foreach (Collider col in colliders)
        {
            if (col != null)
            {
                col.enabled = true;
            }
        }

        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = true;
        }
    }
}