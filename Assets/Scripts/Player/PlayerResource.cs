using System;
using UnityEngine;

public class PlayerResource : MonoBehaviour
{
    private int maxMana;
    private float currentMana;
    private float manaRegenPerSecond;

    public int MaxMana => maxMana;
    public int CurrentMana => Mathf.FloorToInt(currentMana);
    public float CurrentManaFloat => currentMana;
    public float ManaPercent => maxMana <= 0 ? 0f : currentMana / maxMana;
    public bool HasManaResource => maxMana > 0;

    public event Action OnManaChanged;

    private void Update()
    {
        RegenerateMana();
    }

    public void ApplyManaStats(int newMaxMana, float newManaRegenPerSecond)
    {
        int oldMaxMana = maxMana;

        maxMana = Mathf.Max(0, newMaxMana);
        manaRegenPerSecond = Mathf.Max(0f, newManaRegenPerSecond);

        if (maxMana <= 0)
        {
            currentMana = 0;
            OnManaChanged?.Invoke();
            return;
        }

        if (oldMaxMana <= 0)
        {
            currentMana = maxMana;
        }
        else
        {
            int delta = maxMana - oldMaxMana;
            currentMana += delta;
            currentMana = Mathf.Clamp(currentMana, 0f, maxMana);
        }

        OnManaChanged?.Invoke();
    }

    public bool HasEnoughMana(int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        return CurrentMana >= amount;
    }

    public bool TrySpendMana(int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        if (!HasEnoughMana(amount))
        {
            return false;
        }

        currentMana -= amount;
        currentMana = Mathf.Clamp(currentMana, 0f, maxMana);

        OnManaChanged?.Invoke();
        return true;
    }

    public void RestoreMana(int amount)
    {
        if (amount <= 0 || maxMana <= 0)
        {
            return;
        }

        currentMana = Mathf.Clamp(currentMana + amount, 0f, maxMana);
        OnManaChanged?.Invoke();
    }

    public void RestoreManaToFull()
    {
        if (maxMana <= 0)
        {
            currentMana = 0;
            OnManaChanged?.Invoke();
            return;
        }

        currentMana = maxMana;
        OnManaChanged?.Invoke();
    }

    private void RegenerateMana()
    {
        if (maxMana <= 0 || manaRegenPerSecond <= 0f)
        {
            return;
        }

        if (currentMana >= maxMana)
        {
            return;
        }

        currentMana += manaRegenPerSecond * Time.deltaTime;
        currentMana = Mathf.Clamp(currentMana, 0f, maxMana);

        OnManaChanged?.Invoke();
    }
}