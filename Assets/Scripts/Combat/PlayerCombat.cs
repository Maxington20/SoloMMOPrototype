using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Health))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Auto Attack")]
    [SerializeField] private float attackRange = 1.8f;
    [SerializeField] private int damage = 20;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float chaseStopDistance = 1.5f;

    private float lastAttackTime;
    private int equipmentBonusDamage;
    private Health currentTarget;
    private EnemyController currentEnemyTarget;
    private PlayerEquipment playerEquipment;
    private PlayerStats playerStats;
    private PlayerResource playerResource;

    public Transform CurrentTargetTransform => currentTarget != null ? currentTarget.transform : null;
    public int Damage => CalculateAutoAttackDamage();
    public float CurrentAttackRange => GetCurrentAttackRange();
    public bool CurrentAutoAttackIsMelee => GetCurrentAutoAttackIsMelee();

    private void Awake()
    {
        playerEquipment = GetComponent<PlayerEquipment>();
        playerStats = GetComponent<PlayerStats>();
        playerResource = GetComponent<PlayerResource>();
    }

    private void Update()
    {
        HandleTargetSelection();
        HandleAutoAttack();
    }

    public void SetBaseDamage(int amount)
    {
        damage = Mathf.Max(0, amount);
        Debug.Log($"Player base damage set to {damage}. Total damage: {Damage}");
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

    public int CalculateAbilityDamage(AbilityData ability)
    {
        if (ability == null || !ability.DealsDamage)
        {
            return 0;
        }

        int scaledAttackDamage = CalculateAutoAttackDamage();
        int finalDamage = Mathf.RoundToInt(scaledAttackDamage * ability.DamageMultiplier);

        return Mathf.Max(1, finalDamage);
    }

    public bool CanUseAbilityOnCurrentTarget(string abilityName, float range, bool postMessages)
    {
        if (currentTarget == null)
        {
            if (postMessages)
            {
                PostSystem("No target.");
            }

            return false;
        }

        if (currentTarget.IsDead)
        {
            ClearTarget();

            if (postMessages)
            {
                PostSystem("Target is dead.");
            }

            return false;
        }

        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);
        if (distanceToTarget > range)
        {
            if (postMessages)
            {
                PostSystem($"{abilityName} is out of range.");
            }

            return false;
        }

        return true;
    }

    public bool TryUseAbilityOnCurrentTarget(AbilityData ability)
    {
        if (ability == null)
        {
            return false;
        }

        if (!CanUseAbilityOnCurrentTarget(ability.DisplayName, ability.Range, true))
        {
            return false;
        }

        FaceTarget(currentTarget.transform);

        bool didSomething = false;

        if (ability.DealsDamage)
        {
            int finalDamage = CalculateAbilityDamage(ability);
            currentTarget.TakeDamage(finalDamage, gameObject);

            string targetName = GetTargetDisplayName(currentTarget.gameObject);
            Debug.Log($"Player uses {ability.DisplayName} on {targetName} for {finalDamage}");
            PostSystem($"You use {ability.DisplayName} on {targetName} for {finalDamage} damage.");

            didSomething = true;
        }

        if (currentEnemyTarget != null)
        {
            currentEnemyTarget.SetTarget(transform);
        }

        return didSomething;
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

        if (!Physics.Raycast(ray, out RaycastHit hit, 100f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide))
        {
            ClearTarget();
            return;
        }

        EnemyLoot enemyLoot = hit.collider.GetComponentInParent<EnemyLoot>();
        Health clickedHealth = hit.collider.GetComponentInParent<Health>();

        if (enemyLoot != null && clickedHealth != null && clickedHealth.IsDead && enemyLoot.CanBeLooted)
        {
            ClearTarget();

            if (LootWindowUI.Instance != null)
            {
                LootWindowUI.Instance.OpenLoot(enemyLoot);
            }

            return;
        }

        Health targetHealth = hit.collider.GetComponentInParent<Health>();
        if (targetHealth == null || targetHealth.IsDead)
        {
            ClearTarget();
            return;
        }

        EnemyController enemy = hit.collider.GetComponentInParent<EnemyController>();
        if (enemy == null)
        {
            ClearTarget();
            return;
        }

        currentTarget = targetHealth;
        currentEnemyTarget = enemy;

        string targetName = GetTargetDisplayName(targetHealth.gameObject);

        Debug.Log($"Target selected: {targetName}");
        PostSystem($"Target selected: {targetName}.");
    }

    private void HandleAutoAttack()
    {
        if (currentTarget == null)
        {
            return;
        }

        if (currentTarget.IsDead)
        {
            ClearTarget();
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);
        float currentRange = GetCurrentAttackRange();

        FaceTarget(currentTarget.transform);

        if (distanceToTarget > currentRange)
        {
            return;
        }

        if (Time.time - lastAttackTime < attackCooldown)
        {
            return;
        }

        lastAttackTime = Time.time;

        string attackType = GetCurrentAutoAttackIsMelee() ? "melee attacks" : "ranged attacks";
        int finalDamage = CalculateAutoAttackDamage();

        Debug.Log($"Player {attackType} {currentTarget.name} for {finalDamage}");
        currentTarget.TakeDamage(finalDamage, gameObject);

        if (playerResource != null)
        {
            playerResource.GenerateAngerFromAutoAttack();
        }

        if (currentEnemyTarget != null)
        {
            currentEnemyTarget.SetTarget(transform);
        }
    }

    private int CalculateAutoAttackDamage()
    {
        int baseDamage = damage + equipmentBonusDamage;

        if (playerStats == null)
        {
            return Mathf.Max(0, baseDamage);
        }

        return playerStats.ApplyPrimaryStatDamageScaling(baseDamage);
    }

    private float GetCurrentAttackRange()
    {
        ItemData equippedWeapon = playerEquipment != null
            ? playerEquipment.GetEquippedWeapon()
            : null;

        if (equippedWeapon != null && equippedWeapon.IsWeapon)
        {
            return equippedWeapon.WeaponAttackRange;
        }

        return Mathf.Max(0.5f, attackRange);
    }

    private bool GetCurrentAutoAttackIsMelee()
    {
        ItemData equippedWeapon = playerEquipment != null
            ? playerEquipment.GetEquippedWeapon()
            : null;

        if (equippedWeapon != null && equippedWeapon.IsWeapon)
        {
            return equippedWeapon.IsMeleeWeapon;
        }

        return true;
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

    private string GetTargetDisplayName(GameObject targetObject)
    {
        if (targetObject == null)
        {
            return "target";
        }

        DisplayName displayName = targetObject.GetComponent<DisplayName>();
        return displayName != null ? displayName.Display : targetObject.name;
    }

    private void ClearTarget()
    {
        currentTarget = null;
        currentEnemyTarget = null;
    }

    private void PostSystem(string message)
    {
        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem(message);
        }
    }
}