using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;

    [Header("Stat Scaling")]
    [SerializeField] private int healthPerStamina = 10;

    [Header("Combat Resolution")]
    [SerializeField] private float defaultHitChanceForNonPlayerAttackers = 95f;

    private int currentHealth;
    private int equipmentBonusMaxHealth;
    private int statBonusMaxHealth;
    private GameObject lastDamageSource;
    private PlayerStats playerStats;

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
        playerStats = GetComponent<PlayerStats>();
        ResetHealth();
    }

    public void TakeDamage(int amount, GameObject source)
    {
        if (IsDead)
        {
            return;
        }

        lastDamageSource = source;

        if (AttackMisses(source))
        {
            Debug.Log($"{GetDisplayName(source)} missed {gameObject.name}.");
            OnMissed?.Invoke(source);
            return;
        }

        if (DodgesAttack())
        {
            Debug.Log($"{gameObject.name} dodged the attack.");
            OnDodged?.Invoke(source);
            return;
        }

        int finalDamage = CalculateFinalIncomingDamage(amount);

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

    public void SetStatBonusHealthFromStamina(int stamina)
    {
        int oldMaxHealth = MaxHealth;
        statBonusMaxHealth = Mathf.Max(0, stamina * healthPerStamina);
        ApplyMaxHealthChange(oldMaxHealth, false);
    }

    private bool AttackMisses(GameObject source)
    {
        float hitChance = defaultHitChanceForNonPlayerAttackers;

        if (source != null)
        {
            PlayerStats sourceStats = source.GetComponent<PlayerStats>();
            if (sourceStats != null)
            {
                hitChance = sourceStats.HitChancePercent;
            }
        }

        float roll = UnityEngine.Random.Range(0f, 100f);
        return roll > hitChance;
    }

    private bool DodgesAttack()
    {
        if (playerStats == null)
        {
            return false;
        }

        float dodgeRoll = UnityEngine.Random.Range(0f, 100f);
        return dodgeRoll < playerStats.DodgeChancePercent;
    }

    private int CalculateFinalIncomingDamage(int incomingDamage)
    {
        int damage = Mathf.Max(0, incomingDamage);

        if (damage <= 0)
        {
            return 0;
        }

        if (playerStats == null)
        {
            return damage;
        }

        int armour = Mathf.Max(0, playerStats.Armor);
        float damageMultiplier = 100f / (100f + armour);

        return Mathf.Max(1, Mathf.RoundToInt(damage * damageMultiplier));
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

    private string GetDisplayName(GameObject source)
    {
        if (source == null)
        {
            return "Attacker";
        }

        DisplayName displayName = source.GetComponent<DisplayName>();
        return displayName != null ? displayName.Display : source.name;
    }
}