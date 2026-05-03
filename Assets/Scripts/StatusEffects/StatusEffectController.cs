using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class StatusEffectController : MonoBehaviour
{
    private class ActiveStatusEffect
    {
        public StatusEffectData Data;
        public GameObject Source;
        public float RemainingTime;
        public float Duration;
        public float TickTimer;
        public int TickDamage;
    }

    private readonly List<ActiveStatusEffect> activeEffects = new List<ActiveStatusEffect>();

    private Health health;

    public event Action OnStatusEffectsChanged;

    public bool IsStunned
    {
        get
        {
            for (int i = 0; i < activeEffects.Count; i++)
            {
                if (activeEffects[i].Data.EffectType == StatusEffectType.Stun)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public float MovementSpeedMultiplier
    {
        get
        {
            float multiplier = 1f;

            for (int i = 0; i < activeEffects.Count; i++)
            {
                StatusEffectData effect = activeEffects[i].Data;

                if (effect.EffectType == StatusEffectType.Slow)
                {
                    multiplier = Mathf.Min(multiplier, 1f - effect.SlowAmount);
                }
            }

            return Mathf.Clamp(multiplier, 0f, 1f);
        }
    }

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void Update()
    {
        UpdateEffects();
    }

    public IReadOnlyList<StatusEffectUIInfo> GetStatusEffectUIInfos()
    {
        List<StatusEffectUIInfo> infos = new List<StatusEffectUIInfo>();

        for (int i = 0; i < activeEffects.Count; i++)
        {
            ActiveStatusEffect effect = activeEffects[i];

            infos.Add(new StatusEffectUIInfo(
                effect.Data,
                effect.RemainingTime,
                effect.Duration));
        }

        return infos;
    }

    public void ApplyEffect(StatusEffectData effectData, GameObject source, int sourceBaseDamage)
    {
        if (effectData == null || health == null || health.IsDead)
        {
            return;
        }

        ActiveStatusEffect existingEffect = FindExistingEffect(effectData.EffectType, effectData.DisplayName);

        int tickDamage = Mathf.Max(1, Mathf.RoundToInt(sourceBaseDamage * effectData.TickDamageMultiplier));

        if (existingEffect != null)
        {
            existingEffect.RemainingTime = effectData.DurationSeconds;
            existingEffect.Duration = effectData.DurationSeconds;
            existingEffect.Source = source;
            existingEffect.TickDamage = tickDamage;
            existingEffect.TickTimer = effectData.TickIntervalSeconds;

            OnStatusEffectsChanged?.Invoke();
            return;
        }

        ActiveStatusEffect activeEffect = new ActiveStatusEffect
        {
            Data = effectData,
            Source = source,
            RemainingTime = effectData.DurationSeconds,
            Duration = effectData.DurationSeconds,
            TickTimer = effectData.TickIntervalSeconds,
            TickDamage = tickDamage
        };

        activeEffects.Add(activeEffect);

        PostSystem($"{GetDisplayName()} is affected by {effectData.DisplayName}.");
        OnStatusEffectsChanged?.Invoke();
    }

    private void UpdateEffects()
    {
        if (health == null || health.IsDead)
        {
            if (activeEffects.Count > 0)
            {
                activeEffects.Clear();
                OnStatusEffectsChanged?.Invoke();
            }

            return;
        }

        bool changed = false;

        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            ActiveStatusEffect effect = activeEffects[i];

            effect.RemainingTime -= Time.deltaTime;

            if (effect.Data.EffectType == StatusEffectType.DamageOverTime)
            {
                UpdateDamageOverTime(effect);
            }

            if (effect.RemainingTime <= 0f)
            {
                activeEffects.RemoveAt(i);
                changed = true;
            }
        }

        if (changed)
        {
            OnStatusEffectsChanged?.Invoke();
        }
    }

    private void UpdateDamageOverTime(ActiveStatusEffect effect)
    {
        effect.TickTimer -= Time.deltaTime;

        if (effect.TickTimer > 0f)
        {
            return;
        }

        effect.TickTimer = effect.Data.TickIntervalSeconds;

        if (health != null && !health.IsDead)
        {
            health.TakeDamage(effect.TickDamage, effect.Source);
        }
    }

    private ActiveStatusEffect FindExistingEffect(StatusEffectType effectType, string displayName)
    {
        for (int i = 0; i < activeEffects.Count; i++)
        {
            ActiveStatusEffect effect = activeEffects[i];

            if (effect.Data.EffectType == effectType &&
                effect.Data.DisplayName == displayName)
            {
                return effect;
            }
        }

        return null;
    }

    private string GetDisplayName()
    {
        DisplayName displayName = GetComponent<DisplayName>();
        return displayName != null ? displayName.Display : gameObject.name;
    }

    private void PostSystem(string message)
    {
        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem(message);
        }
    }
}