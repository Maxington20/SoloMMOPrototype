using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Health))]
public class SimulatedAdventurer : MonoBehaviour
{
    public enum PreferredEnemyType
    {
        Any,
        Wolf,
        Goblin
    }

    [Header("Identity")]
    [SerializeField] private string adventurerName = "Aldric";
    [SerializeField] private string classLabel = "Warrior";

    [Header("Combat")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float attackRange = 2.5f;
    [SerializeField] private float attackCooldown = 1.2f;
    [SerializeField] private int damage = 15;
    [SerializeField] private float gravity = -20f;

    [Header("Behavior - Resting")]
    [SerializeField] private float idleTimeMin = 2f;
    [SerializeField] private float idleTimeMax = 5f;
    [SerializeField] private float postFightRestMultiplier = 1.25f;

    [Header("Behavior - Hunting Personality")]
    [SerializeField] private PreferredEnemyType preferredEnemyType = PreferredEnemyType.Any;
    [SerializeField] private float maxHuntDistanceFromHome = 25f;
    [SerializeField] private float targetSearchRadius = 30f;
    [SerializeField] private float chanceToSkipTarget = 0.15f;
    [SerializeField] private bool prefersClosestTargets = true;
    [SerializeField] private bool wanderNearTownBeforeHunting = false;
    [SerializeField] private float preHuntWanderRadius = 4f;
    [SerializeField] private float preHuntWanderStopDistance = 0.2f;

    private CharacterController controller;
    private Health health;

    private Transform currentTarget;
    private float lastAttackTime;
    private Vector3 verticalVelocity;

    private Vector3 homePosition;
    private float idleTimer;

    private Vector3 localWanderDestination;
    private bool hasLocalWanderDestination;

    public string AdventurerName => adventurerName;
    public string ClassLabel => classLabel;

    private enum State
    {
        IdleInTown,
        WanderingNearTown,
        MovingToEnemy,
        Attacking,
        ReturningHome
    }

    private State currentState;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        health = GetComponent<Health>();
        homePosition = transform.position;
    }

    private void Start()
    {
        EnterIdle(false);
    }

    private void Update()
    {
        if (health.IsDead)
        {
            return;
        }

        HandleGravity();

        switch (currentState)
        {
            case State.IdleInTown:
                HandleIdle();
                break;

            case State.WanderingNearTown:
                HandleLocalWander();
                break;

            case State.MovingToEnemy:
                MoveToEnemy();
                break;

            case State.Attacking:
                AttackEnemy();
                break;

            case State.ReturningHome:
                ReturnHome();
                break;
        }
    }

    private void EnterIdle(bool afterFight)
    {
        currentState = State.IdleInTown;
        currentTarget = null;
        hasLocalWanderDestination = false;

        float min = idleTimeMin;
        float max = idleTimeMax;

        if (afterFight)
        {
            min *= postFightRestMultiplier;
            max *= postFightRestMultiplier;

            PostSay(Random.value < 0.5f ? "Heading back." : "Catching my breath.");
        }
        else
        {
            if (Random.value < 0.35f)
            {
                PostSay(Random.value < 0.5f ? "Taking a quick rest." : "Back in town for now.");
            }
        }

        idleTimer = Random.Range(min, max);
    }

    private void HandleIdle()
    {
        idleTimer -= Time.deltaTime;

        if (idleTimer > 0f)
        {
            return;
        }

        if (wanderNearTownBeforeHunting)
        {
            BeginLocalWander();
            return;
        }

        FindNewTarget();
    }

    private void BeginLocalWander()
    {
        Vector2 randomCircle = Random.insideUnitCircle * preHuntWanderRadius;
        localWanderDestination = homePosition + new Vector3(randomCircle.x, 0f, randomCircle.y);
        hasLocalWanderDestination = true;
        currentState = State.WanderingNearTown;

        if (Random.value < 0.4f)
        {
            PostSay("Looking around first.");
        }
    }

    private void HandleLocalWander()
    {
        if (!hasLocalWanderDestination)
        {
            FindNewTarget();
            return;
        }

        float distanceToDestination = Vector3.Distance(transform.position, localWanderDestination);

        if (distanceToDestination <= preHuntWanderStopDistance)
        {
            hasLocalWanderDestination = false;
            FindNewTarget();
            return;
        }

        MoveToward(localWanderDestination);
    }

    private void FindNewTarget()
    {
        EnemyController[] enemies = GameObject.FindObjectsByType<EnemyController>(FindObjectsSortMode.None);

        EnemyController chosenEnemy = null;
        float bestScore = float.MinValue;
        string chosenEnemyName = string.Empty;

        foreach (EnemyController enemy in enemies)
        {
            if (enemy == null)
            {
                continue;
            }

            Health enemyHealth = enemy.GetComponent<Health>();
            if (enemyHealth == null || enemyHealth.IsDead)
            {
                continue;
            }

            DisplayName displayName = enemy.GetComponent<DisplayName>();
            string enemyName = displayName != null ? displayName.Display : enemy.gameObject.name;

            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            float distanceFromHomeToEnemy = Vector3.Distance(homePosition, enemy.transform.position);

            if (distanceToEnemy > targetSearchRadius)
            {
                continue;
            }

            if (distanceFromHomeToEnemy > maxHuntDistanceFromHome)
            {
                continue;
            }

            if (ShouldSkipTargetThisAttempt())
            {
                continue;
            }

            float score = ScoreEnemy(enemyName, distanceToEnemy);

            if (score > bestScore)
            {
                bestScore = score;
                chosenEnemy = enemy;
                chosenEnemyName = enemyName;
            }
        }

        if (chosenEnemy != null)
        {
            currentTarget = chosenEnemy.transform;
            currentState = State.MovingToEnemy;

            PostTargetChoiceMessage(chosenEnemyName);
            return;
        }

        EnterIdle(false);
    }

    private bool ShouldSkipTargetThisAttempt()
    {
        return Random.value < chanceToSkipTarget;
    }

    private float ScoreEnemy(string enemyName, float distanceToEnemy)
    {
        float score = 0f;

        if (prefersClosestTargets)
        {
            score += Mathf.Max(0f, 100f - distanceToEnemy * 4f);
        }
        else
        {
            score += Random.Range(20f, 60f);
            score -= distanceToEnemy;
        }

        switch (preferredEnemyType)
        {
            case PreferredEnemyType.Wolf:
                if (enemyName.Contains("Wolf"))
                {
                    score += 100f;
                }
                break;

            case PreferredEnemyType.Goblin:
                if (enemyName.Contains("Goblin"))
                {
                    score += 100f;
                }
                break;

            case PreferredEnemyType.Any:
                score += 10f;
                break;
        }

        score += Random.Range(0f, 15f);

        return score;
    }

    private void MoveToEnemy()
    {
        if (currentTarget == null)
        {
            EnterIdle(false);
            return;
        }

        Health targetHealth = currentTarget.GetComponent<Health>();
        if (targetHealth == null || targetHealth.IsDead)
        {
            currentTarget = null;
            EnterIdle(false);
            return;
        }

        float distance = Vector3.Distance(transform.position, currentTarget.position);

        if (distance <= attackRange)
        {
            currentState = State.Attacking;
            return;
        }

        MoveToward(currentTarget.position);
    }

    private void AttackEnemy()
    {
        if (currentTarget == null)
        {
            EnterIdle(false);
            return;
        }

        Health targetHealth = currentTarget.GetComponent<Health>();

        if (targetHealth == null || targetHealth.IsDead)
        {
            currentTarget = null;
            currentState = State.ReturningHome;
            return;
        }

        float distance = Vector3.Distance(transform.position, currentTarget.position);

        if (distance > attackRange)
        {
            currentState = State.MovingToEnemy;
            return;
        }

        FaceTarget();

        if (Time.time - lastAttackTime < attackCooldown)
        {
            return;
        }

        lastAttackTime = Time.time;

        targetHealth.TakeDamage(damage, gameObject);

        EnemyController enemy = currentTarget.GetComponent<EnemyController>();
        if (enemy != null)
        {
            enemy.SetTarget(transform);
        }
    }

    private void ReturnHome()
    {
        float distance = Vector3.Distance(transform.position, homePosition);

        if (distance <= 0.2f)
        {
            EnterIdle(true);
            return;
        }

        MoveToward(homePosition);
    }

    private void MoveToward(Vector3 destination)
    {
        Vector3 direction = destination - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
        {
            return;
        }

        direction.Normalize();

        controller.Move(direction * moveSpeed * Time.deltaTime);

        Quaternion rotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 10f * Time.deltaTime);
    }

    private void FaceTarget()
    {
        Vector3 direction = currentTarget.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
        {
            return;
        }

        Quaternion rotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 10f * Time.deltaTime);
    }

    private void HandleGravity()
    {
        if (controller.isGrounded && verticalVelocity.y < 0f)
        {
            verticalVelocity.y = -2f;
        }

        verticalVelocity.y += gravity * Time.deltaTime;
        controller.Move(verticalVelocity * Time.deltaTime);
    }

    private void PostSay(string message)
    {
        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSay(adventurerName, message);
        }
    }

    private void PostTargetChoiceMessage(string enemyName)
    {
        if (enemyName.Contains("Wolf"))
        {
            string[] options =
            {
                "Heading for the wolves.",
                "I’ll take the wolves.",
                "Going after a wolf."
            };

            PostSay(options[Random.Range(0, options.Length)]);
            return;
        }

        if (enemyName.Contains("Goblin"))
        {
            string[] options =
            {
                "Going after goblins.",
                "I found a goblin.",
                "Heading for the goblins."
            };

            PostSay(options[Random.Range(0, options.Length)]);
            return;
        }

        string[] genericOptions =
        {
            "Heading out.",
            "Found something.",
            "Moving in."
        };

        PostSay(genericOptions[Random.Range(0, genericOptions.Length)]);
    }
}