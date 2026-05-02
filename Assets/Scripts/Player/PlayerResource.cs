using System;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class PlayerResource : MonoBehaviour
{
    private PlayerResourceType resourceType = PlayerResourceType.None;
    private int maxResource;
    private float currentResource;
    private float regenPerSecond;
    private float decayPerSecond;
    private bool startsFull = true;

    private Health health;
    private PlayerStats playerStats;

    public PlayerResourceType ResourceType => resourceType;
    public int MaxResource => maxResource;
    public int CurrentResource => Mathf.FloorToInt(currentResource);
    public float CurrentResourceFloat => currentResource;
    public float ResourcePercent => maxResource <= 0 ? 0f : currentResource / maxResource;
    public bool HasResource => resourceType != PlayerResourceType.None && maxResource > 0;

    // Backward-compatible aliases in case any existing UI still references mana.
    public int MaxMana => MaxResource;
    public int CurrentMana => CurrentResource;
    public float CurrentManaFloat => CurrentResourceFloat;
    public float ManaPercent => ResourcePercent;
    public bool HasManaResource => HasResource && resourceType == PlayerResourceType.Mana;

    public string ResourceDisplayName
    {
        get
        {
            return resourceType switch
            {
                PlayerResourceType.Mana => "Mana",
                PlayerResourceType.Energy => "Energy",
                PlayerResourceType.Anger => "Anger",
                _ => "Resource"
            };
        }
    }

    public event Action OnResourceChanged;

    // Backward-compatible event alias.
    public event Action OnManaChanged;

    private void Awake()
    {
        health = GetComponent<Health>();
        playerStats = GetComponent<PlayerStats>();
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.OnDamaged += HandleDamaged;
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnDamaged -= HandleDamaged;
        }
    }

    private void Update()
    {
        UpdateResourceOverTime();
    }

    public void ApplyResourceStats(
        PlayerResourceType newResourceType,
        int newMaxResource,
        float newRegenPerSecond,
        float newDecayPerSecond,
        bool newStartsFull)
    {
        PlayerResourceType oldResourceType = resourceType;
        int oldMaxResource = maxResource;

        resourceType = newResourceType;
        maxResource = Mathf.Max(0, newMaxResource);
        regenPerSecond = Mathf.Max(0f, newRegenPerSecond);
        decayPerSecond = Mathf.Max(0f, newDecayPerSecond);
        startsFull = newStartsFull;

        if (resourceType == PlayerResourceType.None || maxResource <= 0)
        {
            currentResource = 0f;
            NotifyResourceChanged();
            return;
        }

        if (oldResourceType != resourceType)
        {
            currentResource = startsFull ? maxResource : 0f;
            NotifyResourceChanged();
            return;
        }

        if (oldMaxResource <= 0)
        {
            currentResource = startsFull ? maxResource : 0f;
        }
        else
        {
            int delta = maxResource - oldMaxResource;
            currentResource += delta;
            currentResource = Mathf.Clamp(currentResource, 0f, maxResource);
        }

        NotifyResourceChanged();
    }

    // Backward-compatible method in case anything still calls ApplyManaStats.
    public void ApplyManaStats(int newMaxMana, float newManaRegenPerSecond)
    {
        ApplyResourceStats(
            PlayerResourceType.Mana,
            newMaxMana,
            newManaRegenPerSecond,
            0f,
            true);
    }

    public bool HasEnoughResource(int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        return HasResource && CurrentResource >= amount;
    }

    public bool TrySpendResource(int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        if (!HasEnoughResource(amount))
        {
            return false;
        }

        currentResource -= amount;
        currentResource = Mathf.Clamp(currentResource, 0f, maxResource);

        NotifyResourceChanged();
        return true;
    }

    public void GenerateResource(int amount)
    {
        if (amount <= 0 || !HasResource)
        {
            return;
        }

        currentResource += amount;
        currentResource = Mathf.Clamp(currentResource, 0f, maxResource);

        NotifyResourceChanged();
    }

    public void RestoreResource(int amount)
    {
        GenerateResource(amount);
    }

    public void RestoreResourceToFull()
    {
        if (!HasResource)
        {
            currentResource = 0f;
            NotifyResourceChanged();
            return;
        }

        currentResource = maxResource;
        NotifyResourceChanged();
    }

    public void GenerateAngerFromAutoAttack()
    {
        if (resourceType != PlayerResourceType.Anger || playerStats == null)
        {
            return;
        }

        GenerateResource(playerStats.CombatTuning.AngerGeneratedByAutoAttack);
    }

    private void HandleDamaged(int amount, GameObject source)
    {
        if (resourceType != PlayerResourceType.Anger || playerStats == null)
        {
            return;
        }

        GenerateResource(playerStats.CombatTuning.AngerGeneratedWhenTakingDamage);
    }

    private void UpdateResourceOverTime()
    {
        if (!HasResource)
        {
            return;
        }

        float oldValue = currentResource;

        if (regenPerSecond > 0f)
        {
            currentResource += regenPerSecond * Time.deltaTime;
        }

        if (decayPerSecond > 0f)
        {
            currentResource -= decayPerSecond * Time.deltaTime;
        }

        currentResource = Mathf.Clamp(currentResource, 0f, maxResource);

        if (!Mathf.Approximately(oldValue, currentResource))
        {
            NotifyResourceChanged();
        }
    }

    private void NotifyResourceChanged()
    {
        OnResourceChanged?.Invoke();
        OnManaChanged?.Invoke();
    }
}