using System;
using UnityEngine;

[Serializable]
public class ClassCombatTuning
{
    public const float FallbackHitChance = 95f;

    [Header("Health")]
    [SerializeField] private int healthPerStamina = 10;

    [Header("Damage / Healing")]
    [SerializeField] private float primaryStatDamageMultiplier = 1f;
    [SerializeField] private float primaryStatHealingMultiplier = 1f;

    [Header("Hit Chance")]
    [SerializeField] private float baseHitChance = 95f;
    [SerializeField] private float maxHitChance = 100f;

    [Header("Dodge")]
    [SerializeField] private float dodgeChancePerAgility = 0.25f;
    [SerializeField] private float maxDodgeChance = 25f;

    [Header("Armour")]
    [SerializeField] private float armourMitigationBase = 100f;

    public int HealthPerStamina => Mathf.Max(1, healthPerStamina);
    public float PrimaryStatDamageMultiplier => Mathf.Max(0f, primaryStatDamageMultiplier);
    public float PrimaryStatHealingMultiplier => Mathf.Max(0f, primaryStatHealingMultiplier);
    public float BaseHitChance => Mathf.Clamp(baseHitChance, 0f, 100f);
    public float MaxHitChance => Mathf.Clamp(maxHitChance, 0f, 100f);
    public float DodgeChancePerAgility => Mathf.Max(0f, dodgeChancePerAgility);
    public float MaxDodgeChance => Mathf.Clamp(maxDodgeChance, 0f, 100f);
    public float ArmourMitigationBase => Mathf.Max(1f, armourMitigationBase);
}