using UnityEngine;

[RequireComponent(typeof(PlayerTargetingController))]
public class PlayerAutoAttackController : MonoBehaviour
{
    [Header("Auto Attack")]
    [SerializeField] private float attackRange = 1.8f;
    [SerializeField] private int baseDamage = 20;
    [SerializeField] private float attackCooldown = 1f;

    private float lastAttackTime;
    private int equipmentBonusDamage;

    private PlayerEquipment playerEquipment;
    private PlayerStats playerStats;
    private PlayerResource playerResource;
    private PlayerTargetingController targetingController;
    private PlayerAnimationController playerAnimationController;

    public int Damage => CalculateAutoAttackDamage();
    public float CurrentAttackRange => GetCurrentAttackRange();
    public bool CurrentAutoAttackIsMelee => GetCurrentAutoAttackIsMelee();

    private void Awake()
    {
        playerEquipment = GetComponent<PlayerEquipment>();
        playerStats = GetComponent<PlayerStats>();
        playerResource = GetComponent<PlayerResource>();
        targetingController = GetComponent<PlayerTargetingController>();
        playerAnimationController = GetComponent<PlayerAnimationController>();
    }

    private void Update()
    {
        HandleAutoAttack();
    }

    public void SetBaseDamage(int amount)
    {
        baseDamage = Mathf.Max(0, amount);
        Debug.Log($"Player base damage set to {baseDamage}. Total damage: {Damage}");
    }

    public void IncreaseDamage(int amount)
    {
        baseDamage += amount;
        Debug.Log($"Player damage increased to {Damage}");
    }

    public void SetEquipmentBonusDamage(int amount)
    {
        equipmentBonusDamage = Mathf.Max(0, amount);
        Debug.Log($"Player damage updated to {Damage}");
    }

    public void ResetAttackTimer()
    {
        lastAttackTime = 0f;
    }

    private void HandleAutoAttack()
    {
        Health currentTarget = targetingController != null
            ? targetingController.CurrentTargetHealth
            : null;

        if (currentTarget == null)
        {
            return;
        }

        if (currentTarget.IsDead)
        {
            targetingController.ClearTarget();
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

        CombatFeedbackService.QueueImpactFeedback(currentTarget, null);

        if (playerAnimationController != null)
        {
            playerAnimationController.PlayAttack();
        }

        currentTarget.TakeDamage(finalDamage, gameObject);
        ThreatService.ApplyThreat(gameObject, currentTarget.gameObject, null, finalDamage);

        if (playerResource != null)
        {
            playerResource.GenerateAngerFromAutoAttack();
        }

        EnemyController currentEnemyTarget = targetingController.CurrentEnemyTarget;

        if (currentEnemyTarget != null)
        {
            currentEnemyTarget.SetTarget(transform);
        }
    }

    private int CalculateAutoAttackDamage()
    {
        int rawDamage = baseDamage + equipmentBonusDamage;

        if (playerStats == null)
        {
            return Mathf.Max(0, rawDamage);
        }

        return playerStats.ApplyPrimaryStatDamageScaling(rawDamage);
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
}