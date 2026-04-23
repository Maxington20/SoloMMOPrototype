using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Health))]
public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private float attackRange = 2.5f;
    [SerializeField] private int damage = 20;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float chaseStopDistance = 1.5f;

    private float lastAttackTime;
    private int equipmentBonusDamage;
    private Health currentTarget;
    private EnemyController currentEnemyTarget;

    public Transform CurrentTargetTransform => currentTarget != null ? currentTarget.transform : null;
    public int Damage => damage + equipmentBonusDamage;

    private void Update()
    {
        HandleTargetSelection();
        HandleAutoAttack();
    }

    public void IncreaseDamage(int amount)
    {
        damage += amount;
        Debug.Log($"Player damage increased to {Damage}");
    }

    public void SetEquipmentBonusDamage(int amount)
    {
        equipmentBonusDamage = Mathf.Max(0, amount);
        Debug.Log($"Player damage updated to {Damage}");
    }

    public bool TryUseAbilityOnCurrentTarget(string abilityName, int amount, float range)
    {
        if (currentTarget == null || currentTarget.IsDead)
        {
            ClearTarget();
            return false;
        }

        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);
        if (distanceToTarget > range)
        {
            return false;
        }

        FaceTarget(currentTarget.transform);

        currentTarget.TakeDamage(amount, gameObject);

        if (currentEnemyTarget != null)
        {
            currentEnemyTarget.SetTarget(transform);
        }

        string targetName = currentTarget.name;
        DisplayName displayName = currentTarget.GetComponent<DisplayName>();
        if (displayName != null)
        {
            targetName = displayName.Display;
        }

        Debug.Log($"Player uses {abilityName} on {targetName} for {amount}");

        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem(
                $"You use {abilityName} on {targetName} for {amount} damage.");
        }

        return true;
    }

    private void HandleTargetSelection()
    {
        if (!Input.GetMouseButtonDown(0))
        {
            return;
        }

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            ClearTarget();
            return;
        }

        Health targetHealth = hit.collider.GetComponent<Health>();
        if (targetHealth == null || targetHealth.IsDead)
        {
            ClearTarget();
            return;
        }

        EnemyController enemy = hit.collider.GetComponent<EnemyController>();
        if (enemy == null)
        {
            ClearTarget();
            return;
        }

        currentTarget = targetHealth;
        currentEnemyTarget = enemy;

        DisplayName displayName = hit.collider.GetComponent<DisplayName>();
        string targetName = displayName != null ? displayName.Display : hit.collider.name;

        Debug.Log($"Target selected: {targetName}");

        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem($"Target selected: {targetName}.");
        }
    }

    private void HandleAutoAttack()
    {
        if (currentTarget == null || currentTarget.IsDead)
        {
            ClearTarget();
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);

        FaceTarget(currentTarget.transform);

        if (distanceToTarget > attackRange)
        {
            return;
        }

        if (Time.time - lastAttackTime < attackCooldown)
        {
            return;
        }

        lastAttackTime = Time.time;

        Debug.Log($"Player auto-attacks {currentTarget.name} for {Damage}");
        currentTarget.TakeDamage(Damage, gameObject);

        if (currentEnemyTarget != null)
        {
            currentEnemyTarget.SetTarget(transform);
        }
    }

    private void FaceTarget(Transform target)
    {
        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            10f * Time.deltaTime);
    }

    private void ClearTarget()
    {
        currentTarget = null;
        currentEnemyTarget = null;
    }
}