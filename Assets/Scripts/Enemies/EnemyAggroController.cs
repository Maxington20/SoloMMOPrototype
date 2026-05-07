using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(ThreatTable))]
[RequireComponent(typeof(EnemyDeathRespawnController))]
public class EnemyAggroController : MonoBehaviour
{
    [Header("Aggro")]
    [SerializeField] private float aggroRange = 8f;
    [SerializeField] private float leashRange = 14f;

    private Health health;
    private ThreatTable threatTable;
    private EnemyDeathRespawnController deathRespawnController;

    private Transform player;
    private Transform currentTarget;

    public Transform CurrentTarget => currentTarget;
    public bool HasTarget => currentTarget != null;

    private void Awake()
    {
        health = GetComponent<Health>();
        threatTable = GetComponent<ThreatTable>();
        deathRespawnController = GetComponent<EnemyDeathRespawnController>();
    }

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    public void TickTargetSelection()
    {
        if (!CanAggro())
        {
            ClearTarget();
            return;
        }

        SelectHighestThreatTarget();

        if (currentTarget != null)
        {
            return;
        }

        TryAggroPlayerByRange();
    }

    public void SetTarget(Transform newTarget, float initialThreat = 1f)
    {
        if (!CanAggro() || newTarget == null)
        {
            return;
        }

        currentTarget = newTarget;

        if (threatTable != null)
        {
            threatTable.AddThreat(newTarget.gameObject, initialThreat);
        }
    }

    public bool ShouldLeashFromCurrentTarget(Vector3 enemyPosition)
    {
        if (currentTarget == null)
        {
            return false;
        }

        float distanceToTarget = Vector3.Distance(enemyPosition, currentTarget.position);
        return distanceToTarget > leashRange;
    }

    public void ClearTarget()
    {
        currentTarget = null;
    }

    public void ClearTargetAndThreat()
    {
        currentTarget = null;

        if (threatTable != null)
        {
            threatTable.ClearAll();
        }
    }

    private void SelectHighestThreatTarget()
    {
        if (threatTable == null)
        {
            return;
        }

        GameObject highestThreatTarget = threatTable.GetHighestThreatTarget();

        if (highestThreatTarget == null)
        {
            return;
        }

        Health targetHealth = highestThreatTarget.GetComponent<Health>();
        if (targetHealth != null && targetHealth.IsDead)
        {
            return;
        }

        currentTarget = highestThreatTarget.transform;
    }

    private void TryAggroPlayerByRange()
    {
        if (player == null)
        {
            return;
        }

        Health playerHealth = player.GetComponent<Health>();

        if (playerHealth != null && playerHealth.IsDead)
        {
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > aggroRange)
        {
            return;
        }

        SetTarget(player, 1f);
    }

    private bool CanAggro()
    {
        if (health == null || health.IsDead)
        {
            return false;
        }

        if (deathRespawnController != null && deathRespawnController.IsRespawning)
        {
            return false;
        }

        return true;
    }
}