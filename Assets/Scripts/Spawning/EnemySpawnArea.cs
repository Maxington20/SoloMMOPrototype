using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnArea : MonoBehaviour
{
    [Header("Enemy")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("Area")]
    [SerializeField] private float spawnRadius = 8f;
    [SerializeField] private int maxAlive = 3;

    [Header("Spawn Timing")]
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private float initialSpawnSpacingSeconds = 0.25f;
    [SerializeField] private float respawnDelaySeconds = 10f;

    [Header("Corpse Cleanup")]
    [SerializeField] private float unlootedCorpseLifetimeSeconds = 60f;
    [SerializeField] private float lootedCorpseLifetimeSeconds = 30f;

    [Header("Spawn Placement")]
    [SerializeField] private int maxSpawnPositionAttempts = 20;
    [SerializeField] private LayerMask groundMask = ~0;
    [SerializeField] private float groundRaycastHeight = 10f;
    [SerializeField] private float groundRaycastDistance = 30f;

    [Header("Rotation")]
    [SerializeField] private bool useRandomYRotation = true;
    [SerializeField] private bool useAreaRotationIfNotRandom = true;

    private readonly List<GameObject> aliveEnemies = new List<GameObject>();
    private readonly HashSet<GameObject> enemiesAlreadyHandledDeath = new HashSet<GameObject>();

    private Coroutine initialSpawnCoroutine;

    private void Start()
    {
        if (spawnOnStart)
        {
            initialSpawnCoroutine = StartCoroutine(SpawnInitialEnemies());
        }
    }

    private IEnumerator SpawnInitialEnemies()
    {
        while (GetAliveCount() < maxAlive)
        {
            SpawnEnemy();

            if (initialSpawnSpacingSeconds > 0f)
            {
                yield return new WaitForSeconds(initialSpawnSpacingSeconds);
            }
            else
            {
                yield return null;
            }
        }

        initialSpawnCoroutine = null;
    }

    private void SpawnEnemy()
    {
        CleanupNullEnemies();

        if (enemyPrefab == null)
        {
            Debug.LogWarning($"{name} has no enemy prefab assigned.");
            return;
        }

        if (GetAliveCount() >= maxAlive)
        {
            return;
        }

        Vector3 spawnPosition = GetSpawnPosition();
        Quaternion spawnRotation = GetSpawnRotation();

        GameObject spawnedEnemy = Instantiate(enemyPrefab, spawnPosition, spawnRotation);
        spawnedEnemy.name = enemyPrefab.name;

        InitializeSpawnedEnemy(spawnedEnemy, spawnPosition);

        Health health = spawnedEnemy.GetComponent<Health>();

        if (health != null)
        {
            health.OnDied += () => HandleEnemyDied(spawnedEnemy);
        }
        else
        {
            Debug.LogWarning($"{spawnedEnemy.name} spawned from {name} has no Health component.");
        }

        aliveEnemies.Add(spawnedEnemy);
    }

    private void HandleEnemyDied(GameObject enemy)
    {
        if (enemy == null)
        {
            return;
        }

        if (enemiesAlreadyHandledDeath.Contains(enemy))
        {
            return;
        }

        enemiesAlreadyHandledDeath.Add(enemy);
        aliveEnemies.Remove(enemy);

        StartCoroutine(CleanupCorpseAfterDelay(enemy));
        StartCoroutine(RespawnOneEnemyAfterDelay());
    }

    private IEnumerator RespawnOneEnemyAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelaySeconds);

        CleanupNullEnemies();

        if (GetAliveCount() < maxAlive)
        {
            SpawnEnemy();
        }
    }

    private IEnumerator CleanupCorpseAfterDelay(GameObject corpse)
    {
        if (corpse == null)
        {
            yield break;
        }

        EnemyLoot loot = corpse.GetComponent<EnemyLoot>();
        float deathTime = Time.time;

        if (loot == null)
        {
            yield return new WaitForSeconds(lootedCorpseLifetimeSeconds);

            if (corpse != null)
            {
                Destroy(corpse);
            }

            yield break;
        }

        while (corpse != null && loot.CanBeLooted && Time.time - deathTime < unlootedCorpseLifetimeSeconds)
        {
            yield return null;
        }

        if (corpse == null)
        {
            yield break;
        }

        if (!loot.CanBeLooted)
        {
            yield return new WaitForSeconds(lootedCorpseLifetimeSeconds);
        }

        if (corpse != null)
        {
            Destroy(corpse);
        }
    }

    private void InitializeSpawnedEnemy(GameObject enemy, Vector3 homePosition)
    {
        if (enemy == null)
        {
            return;
        }

        EnemyMovementController movementController = enemy.GetComponent<EnemyMovementController>();

        if (movementController != null)
        {
            movementController.SetHomePosition(homePosition);
            movementController.ClearWanderDestination();
            movementController.ResetWanderTimer();
        }
    }

    private int GetAliveCount()
    {
        CleanupNullEnemies();

        int count = 0;

        for (int i = 0; i < aliveEnemies.Count; i++)
        {
            GameObject enemy = aliveEnemies[i];

            if (enemy == null)
            {
                continue;
            }

            Health health = enemy.GetComponent<Health>();

            if (health != null && !health.IsDead)
            {
                count++;
            }
        }

        return count;
    }

    private Vector3 GetSpawnPosition()
    {
        for (int i = 0; i < maxSpawnPositionAttempts; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 candidate = transform.position + new Vector3(randomCircle.x, 0f, randomCircle.y);

            Vector3 rayStart = candidate + Vector3.up * groundRaycastHeight;

            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, groundRaycastDistance, groundMask, QueryTriggerInteraction.Ignore))
            {
                return hit.point;
            }
        }

        return transform.position;
    }

    private Quaternion GetSpawnRotation()
    {
        if (useRandomYRotation)
        {
            return Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        }

        if (useAreaRotationIfNotRandom)
        {
            return transform.rotation;
        }

        return enemyPrefab != null
            ? enemyPrefab.transform.rotation
            : Quaternion.identity;
    }

    private void CleanupNullEnemies()
    {
        for (int i = aliveEnemies.Count - 1; i >= 0; i--)
        {
            if (aliveEnemies[i] == null)
            {
                aliveEnemies.RemoveAt(i);
            }
        }
    }

    private void OnDisable()
    {
        if (initialSpawnCoroutine != null)
        {
            StopCoroutine(initialSpawnCoroutine);
            initialSpawnCoroutine = null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}