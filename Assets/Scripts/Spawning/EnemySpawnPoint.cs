using System.Collections;
using UnityEngine;

public class EnemySpawnPoint : MonoBehaviour
{
    [Header("Enemy")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private float respawnDelaySeconds = 10f;
    [SerializeField] private bool respawnAfterDeath = true;

    [Header("Spawn Transform")]
    [SerializeField] private bool useSpawnPointRotation = true;

    private GameObject currentEnemy;
    private Health currentEnemyHealth;
    private Coroutine respawnCoroutine;

    private void Start()
    {
        if (spawnOnStart)
        {
            SpawnEnemy();
        }
    }

    public void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning($"{name} has no enemy prefab assigned.");
            return;
        }

        CleanupCurrentEnemy();

        Quaternion spawnRotation = useSpawnPointRotation
            ? transform.rotation
            : enemyPrefab.transform.rotation;

        currentEnemy = Instantiate(
            enemyPrefab,
            transform.position,
            spawnRotation);

        currentEnemy.name = enemyPrefab.name;

        InitializeSpawnedEnemy();

        currentEnemyHealth = currentEnemy.GetComponent<Health>();

        if (currentEnemyHealth != null)
        {
            currentEnemyHealth.OnDied += HandleEnemyDied;
        }
        else
        {
            Debug.LogWarning($"{currentEnemy.name} spawned from {name} has no Health component.");
        }
    }

    private void InitializeSpawnedEnemy()
    {
        if (currentEnemy == null)
        {
            return;
        }

        EnemyMovementController movementController = currentEnemy.GetComponent<EnemyMovementController>();

        if (movementController != null)
        {
            movementController.SetHomePosition(transform.position);
            movementController.ClearWanderDestination();
            movementController.ResetWanderTimer();
        }
    }

    private void HandleEnemyDied()
    {
        if (currentEnemyHealth != null)
        {
            currentEnemyHealth.OnDied -= HandleEnemyDied;
        }

        if (!respawnAfterDeath)
        {
            return;
        }

        if (respawnCoroutine != null)
        {
            StopCoroutine(respawnCoroutine);
        }

        respawnCoroutine = StartCoroutine(RespawnAfterDelay());
    }

    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelaySeconds);

        CleanupCurrentEnemy();
        SpawnEnemy();

        respawnCoroutine = null;
    }

    private void CleanupCurrentEnemy()
    {
        if (currentEnemyHealth != null)
        {
            currentEnemyHealth.OnDied -= HandleEnemyDied;
            currentEnemyHealth = null;
        }

        if (currentEnemy != null)
        {
            Destroy(currentEnemy);
            currentEnemy = null;
        }
    }

    private void OnDisable()
    {
        if (respawnCoroutine != null)
        {
            StopCoroutine(respawnCoroutine);
            respawnCoroutine = null;
        }

        if (currentEnemyHealth != null)
        {
            currentEnemyHealth.OnDied -= HandleEnemyDied;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);

        Vector3 forward = transform.forward * 1.25f;
        Gizmos.DrawLine(transform.position, transform.position + forward);
    }
}