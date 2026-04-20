using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;

    private int currentHealth;
    private int equipmentBonusMaxHealth;
    private GameObject lastDamageSource;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth + equipmentBonusMaxHealth;
    public bool IsDead => currentHealth <= 0;
    public float HealthPercent => MaxHealth <= 0 ? 0f : (float)currentHealth / MaxHealth;
    public GameObject LastDamageSource => lastDamageSource;

    public event Action OnHealthChanged;
    public event Action OnDied;
    public event Action<int, GameObject> OnDamaged;

    private void Awake()
    {
        ResetHealth();
    }

    public void TakeDamage(int amount, GameObject source)
    {
        if (IsDead)
        {
            return;
        }

        lastDamageSource = source;

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log($"{gameObject.name} took {amount} damage. Health: {currentHealth}/{MaxHealth}");

        OnDamaged?.Invoke(amount, source);
        OnHealthChanged?.Invoke();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void ResetHealth()
    {
        currentHealth = MaxHealth;
        lastDamageSource = null;
        OnHealthChanged?.Invoke();
    }

    public void IncreaseMaxHealth(int amount, bool fullyHeal)
    {
        maxHealth += amount;

        if (fullyHeal)
        {
            currentHealth = MaxHealth;
        }
        else
        {
            currentHealth = Mathf.Min(currentHealth + amount, MaxHealth);
        }

        OnHealthChanged?.Invoke();
    }

    public void SetEquipmentBonusHealth(int amount)
    {
        int oldMaxHealth = MaxHealth;

        equipmentBonusMaxHealth = Mathf.Max(0, amount);

        int newMaxHealth = MaxHealth;
        int delta = newMaxHealth - oldMaxHealth;

        currentHealth += delta;
        currentHealth = Mathf.Clamp(currentHealth, 0, newMaxHealth);

        OnHealthChanged?.Invoke();
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} died.");
        OnDied?.Invoke();
    }
}