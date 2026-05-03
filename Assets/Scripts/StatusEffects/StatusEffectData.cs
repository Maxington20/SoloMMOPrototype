using System;
using UnityEngine;

[Serializable]
public class StatusEffectData
{
    [SerializeField] private StatusEffectType effectType;
    [SerializeField] private string displayName = "Status Effect";
    [SerializeField] private Sprite icon;
    [SerializeField] private float durationSeconds = 3f;

    [Header("Damage Over Time")]
    [SerializeField] private float tickIntervalSeconds = 1f;
    [SerializeField] private float tickDamageMultiplier = 0.25f;

    [Header("Slow")]
    [SerializeField, Range(0f, 1f)] private float slowAmount = 0.5f;

    public StatusEffectType EffectType => effectType;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public float DurationSeconds => Mathf.Max(0f, durationSeconds);

    public float TickIntervalSeconds => Mathf.Max(0.1f, tickIntervalSeconds);
    public float TickDamageMultiplier => Mathf.Max(0f, tickDamageMultiplier);

    public float SlowAmount => Mathf.Clamp01(slowAmount);
}