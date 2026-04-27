using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;

    private int currentHealth;
    private int equipmentBonusMaxHealth;
    private int statBonusMaxHealth;
    private GameObject lastDamageSource;
    private ICombatStatsProvider combatStatsProvider;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth + equipmentBonusMaxHealth + statBonusMaxHealth;
    public bool IsDead => currentHealth <= 0;
    public float HealthPercent => MaxHealth <= 0 ? 0f : (float)currentHealth / MaxHealth;
    public GameObject LastDamageSource => lastDamageSource;

    public event Action OnHealthChanged;
    public event Action OnDied;
    public event Action<int, GameObject> OnDamaged;
    public event Action<int, GameObject> OnHealed;
    public event Action<GameObject> OnDodged;
    public event Action<GameObject> OnMissed;

    private void Awake()
    {
        combatStatsProvider = GetComponent<ICombatStatsProvider>();
        ResetHealth();
    }

    public void TakeDamage(int amount, GameObject source)
    {
        if (IsDead)
        {
            return;
        }

        lastDamageSource = source;

        if (!AttackHits(source))
        {
            OnMissed?.Invoke(source);
            return;
        }

        if (combatStatsProvider != null && combatStatsProvider.RollDodge())
        {
            OnDodged?.Invoke(source);
            return;
        }

        int finalDamage = combatStatsProvider != null
            ? combatStatsProvider.ReduceIncomingDamageByArmour(amount)
            : Mathf.Max(1, amount);

        currentHealth -= finalDamage;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log($"{gameObject.name} took {finalDamage} damage. Health: {currentHealth}/{MaxHealth}");

        OnDamaged?.Invoke(finalDamage, source);
        OnHealthChanged?.Invoke();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public int RestoreHealth(int amount)
    {
        if (amount <= 0 || IsDead)
        {
            return 0;
        }

        int oldHealth = currentHealth;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, MaxHealth);

        int restored = currentHealth - oldHealth;

        if (restored > 0)
        {
            OnHealed?.Invoke(restored, gameObject);
            OnHealthChanged?.Invoke();
        }

        return restored;
    }

    public void ResetHealth()
    {
        currentHealth = MaxHealth;
        lastDamageSource = null;
        OnHealthChanged?.Invoke();
    }

    public void SetBaseMaxHealth(int amount, bool fullyHeal)
    {
        int oldMaxHealth = MaxHealth;
        maxHealth = Mathf.Max(1, amount);
        ApplyMaxHealthChange(oldMaxHealth, fullyHeal);
    }

    public void IncreaseMaxHealth(int amount, bool fullyHeal)
    {
        int oldMaxHealth = MaxHealth;
        maxHealth += amount;
        maxHealth = Mathf.Max(1, maxHealth);
        ApplyMaxHealthChange(oldMaxHealth, fullyHeal);
    }

    public void SetEquipmentBonusHealth(int amount)
    {
        int oldMaxHealth = MaxHealth;
        equipmentBonusMaxHealth = Mathf.Max(0, amount);
        ApplyMaxHealthChange(oldMaxHealth, false);
    }

    public void SetStatBonusHealth(int amount)
    {
        int oldMaxHealth = MaxHealth;
        statBonusMaxHealth = Mathf.Max(0, amount);
        ApplyMaxHealthChange(oldMaxHealth, false);
    }

    private bool AttackHits(GameObject source)
    {
        float hitChance = ClassCombatTuning.FallbackHitChance;

        if (source != null)
        {
            ICombatStatsProvider sourceStats = source.GetComponent<ICombatStatsProvider>();
            if (sourceStats != null)
            {
                hitChance = sourceStats.HitChancePercent;
            }
        }

        float roll = UnityEngine.Random.Range(0f, 100f);
        return roll <= hitChance;
    }

    private void ApplyMaxHealthChange(int oldMaxHealth, bool fullyHeal)
    {
        int newMaxHealth = MaxHealth;

        if (fullyHeal)
        {
            currentHealth = newMaxHealth;
        }
        else
        {
            int delta = newMaxHealth - oldMaxHealth;
            currentHealth += delta;
            currentHealth = Mathf.Clamp(currentHealth, 0, newMaxHealth);
        }

        OnHealthChanged?.Invoke();
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} died.");
        OnDied?.Invoke();
    }
}