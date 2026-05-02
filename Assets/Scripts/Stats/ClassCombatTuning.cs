using System;
using UnityEngine;

[Serializable]
public class ClassCombatTuning
{
    public const float FallbackHitChance = 95f;

    [Header("Health")]
    [SerializeField] private int healthPerStamina = 10;

    [Header("Class Resource")]
    [SerializeField] private PlayerResourceType resourceType = PlayerResourceType.None;
    [SerializeField] private int baseResource = 0;
    [SerializeField] private int resourcePerPrimaryStat = 0;
    [SerializeField] private float resourceRegenPerSecond = 0f;
    [SerializeField] private float resourceDecayPerSecond = 0f;
    [SerializeField] private bool startsFull = true;

    [Header("Anger Generation")]
    [SerializeField] private int angerGeneratedWhenTakingDamage = 8;
    [SerializeField] private int angerGeneratedByAutoAttack = 5;

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

    public PlayerResourceType ResourceType => resourceType;
    public int BaseResource => Mathf.Max(0, baseResource);
    public int ResourcePerPrimaryStat => Mathf.Max(0, resourcePerPrimaryStat);
    public float ResourceRegenPerSecond => Mathf.Max(0f, resourceRegenPerSecond);
    public float ResourceDecayPerSecond => Mathf.Max(0f, resourceDecayPerSecond);
    public bool StartsFull => startsFull;

    public int AngerGeneratedWhenTakingDamage => Mathf.Max(0, angerGeneratedWhenTakingDamage);
    public int AngerGeneratedByAutoAttack => Mathf.Max(0, angerGeneratedByAutoAttack);

    public float PrimaryStatDamageMultiplier => Mathf.Max(0f, primaryStatDamageMultiplier);
    public float PrimaryStatHealingMultiplier => Mathf.Max(0f, primaryStatHealingMultiplier);

    public float BaseHitChance => Mathf.Clamp(baseHitChance, 0f, 100f);
    public float MaxHitChance => Mathf.Clamp(maxHitChance, 0f, 100f);

    public float DodgeChancePerAgility => Mathf.Max(0f, dodgeChancePerAgility);
    public float MaxDodgeChance => Mathf.Clamp(maxDodgeChance, 0f, 100f);

    public float ArmourMitigationBase => Mathf.Max(1f, armourMitigationBase);
}