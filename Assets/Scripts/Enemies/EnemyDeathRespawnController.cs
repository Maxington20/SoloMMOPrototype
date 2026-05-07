using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyDeathRespawnController : MonoBehaviour
{
    [Header("Respawn")]
    [SerializeField] private float respawnDelay = 8f;
    [SerializeField] private Renderer[] renderersToHideOnDeath;
    [SerializeField] private GameObject[] objectsToHideOnDeath;

    [Header("Corpse / Loot")]
    [SerializeField] private bool hideBodyOnDeath = false;

    private Health health;
    private EnemyStats enemyStats;
    private CharacterController characterController;

    private Vector3 homePosition;
    private Quaternion homeRotation;

    private bool isRespawning;
    private float respawnTimer;

    public bool IsRespawning => isRespawning;
    public bool HideBodyOnDeath => hideBodyOnDeath;

    private void Awake()
    {
        health = GetComponent<Health>();
        enemyStats = GetComponent<EnemyStats>();
        characterController = GetComponent<CharacterController>();

        homePosition = transform.position;
        homeRotation = transform.rotation;
    }

    private void Start()
    {
        if (renderersToHideOnDeath == null || renderersToHideOnDeath.Length == 0)
        {
            renderersToHideOnDeath = GetComponentsInChildren<Renderer>();
        }
    }

    private void Update()
    {
        if (!isRespawning)
        {
            return;
        }

        respawnTimer -= Time.deltaTime;

        if (respawnTimer <= 0f)
        {
            Respawn();
        }
    }

    public void BeginRespawn()
    {
        isRespawning = true;
        respawnTimer = respawnDelay;

        EnemyLoot enemyLoot = GetComponent<EnemyLoot>();

        DisableCollisionOnDeath(enemyLoot != null ? enemyLoot.LootClickCollider : null);

        if (hideBodyOnDeath)
        {
            SetVisible(false);
        }
    }

    public void ForceHomePosition(Vector3 position, Quaternion rotation)
    {
        homePosition = position;
        homeRotation = rotation;
    }

    private void Respawn()
    {
        isRespawning = false;

        transform.position = homePosition;
        transform.rotation = homeRotation;

        EnemyLoot enemyLoot = GetComponent<EnemyLoot>();
        if (enemyLoot != null)
        {
            enemyLoot.ResetLoot();
        }

        ReenableCollisionAfterRespawn();

        if (enemyStats != null)
        {
            enemyStats.RecalculateAndApplyStats(true);
        }
        else if (health != null)
        {
            health.ResetHealth();
        }

        SetVisible(true);

        if (characterController != null)
        {
            characterController.enabled = true;
        }
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