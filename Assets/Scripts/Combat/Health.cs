using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;

    private int currentHealth;
    private GameObject lastDamageSource;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => currentHealth <= 0;
    public float HealthPercent => maxHealth <= 0 ? 0f : (float)currentHealth / maxHealth;
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

        Debug.Log($"{gameObject.name} took {amount} damage. Health: {currentHealth}/{maxHealth}");

        OnDamaged?.Invoke(amount, source);
        OnHealthChanged?.Invoke();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        lastDamageSource = null;
        OnHealthChanged?.Invoke();
    }

    public void IncreaseMaxHealth(int amount, bool fullyHeal)
    {
        maxHealth += amount;

        if (fullyHeal)
        {
            currentHealth = maxHealth;
        }
        else
        {
            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        }

        OnHealthChanged?.Invoke();
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} died.");
        OnDied?.Invoke();
    }
}