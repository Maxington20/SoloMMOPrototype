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

    [Header("Corpse Cleanup")]
    [SerializeField] private float unlootedCorpseLifetimeSeconds = 60f;
    [SerializeField] private float lootedCorpseLifetimeSeconds = 30f;

    [Header("Spawn Transform")]
    [SerializeField] private bool useSpawnPointRotation = true;

    private GameObject currentLivingEnemy;
    private bool deathAlreadyHandled;

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

        Quaternion spawnRotation = useSpawnPointRotation
            ? transform.rotation
            : enemyPrefab.transform.rotation;

        currentLivingEnemy = Instantiate(enemyPrefab, transform.position, spawnRotation);
        currentLivingEnemy.name = enemyPrefab.name;
        deathAlreadyHandled = false;

        InitializeSpawnedEnemy(currentLivingEnemy);

        Health health = currentLivingEnemy.GetComponent<Health>();

        if (health != null)
        {
            health.OnDied += HandleEnemyDied;
        }
        else
        {
            Debug.LogWarning($"{currentLivingEnemy.name} spawned from {name} has no Health component.");
        }
    }

    private void InitializeSpawnedEnemy(GameObject enemy)
    {
        if (enemy == null)
        {
            return;
        }

        EnemyMovementController movementController = enemy.GetComponent<EnemyMovementController>();

        if (movementController != null)
        {
            movementController.SetHomePosition(transform.position);
            movementController.ClearWanderDestination();
            movementController.ResetWanderTimer();
        }
    }

    private void HandleEnemyDied()
    {
        if (deathAlreadyHandled)
        {
            return;
        }

        deathAlreadyHandled = true;

        GameObject corpse = currentLivingEnemy;
        currentLivingEnemy = null;

        if (corpse != null)
        {
            Health health = corpse.GetComponent<Health>();

            if (health != null)
            {
                health.OnDied -= HandleEnemyDied;
            }

            StartCoroutine(CleanupCorpseAfterDelay(corpse));
        }

        if (respawnAfterDeath)
        {
            StartCoroutine(RespawnAfterDelay());
        }
    }

    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelaySeconds);
        SpawnEnemy();
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);

        Vector3 forward = transform.forward * 1.25f;
        Gizmos.DrawLine(transform.position, transform.position + forward);
    }
}