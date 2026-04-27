using UnityEngine;

[RequireComponent(typeof(EnemyData))]
[RequireComponent(typeof(Health))]
public class EnemyStats : MonoBehaviour, ICombatStatsProvider
{
    private EnemyData enemyData;
    private Health health;

    public StatBlock TotalStats => enemyData != null ? enemyData.Stats : StatBlock.Zero;

    public ClassCombatTuning CombatTuning
    {
        get
        {
            if (enemyData != null)
            {
                return enemyData.CombatTuning;
            }

            return new ClassCombatTuning();
        }
    }

    public PrimaryStatType PrimaryStat
    {
        get
        {
            if (enemyData != null)
            {
                return enemyData.PrimaryStat;
            }

            return PrimaryStatType.Strength;
        }
    }

    public int PrimaryStatValue => TotalStats.GetPrimaryValue(PrimaryStat);
    public int Armour => TotalStats.Armor;
    public int Armor => TotalStats.Armor;

    public float DodgeChancePercent
    {
        get
        {
            float chance = TotalStats.Agility * CombatTuning.DodgeChancePerAgility;
            return Mathf.Clamp(chance, 0f, CombatTuning.MaxDodgeChance);
        }
    }

    public float HitChancePercent
    {
        get
        {
            float chance = CombatTuning.BaseHitChance + TotalStats.HitChance;
            return Mathf.Clamp(chance, 0f, CombatTuning.MaxHitChance);
        }
    }

    public int MaxHealthFromStamina
    {
        get
        {
            return Mathf.Max(0, TotalStats.Stamina * CombatTuning.HealthPerStamina);
        }
    }

    private void Awake()
    {
        enemyData = GetComponent<EnemyData>();
        health = GetComponent<Health>();
    }

    private void Start()
    {
        RecalculateAndApplyStats(true);
    }

    public void RecalculateAndApplyStats(bool fullyHeal)
    {
        if (enemyData == null || health == null)
        {
            return;
        }

        health.SetBaseMaxHealth(enemyData.BaseMaxHealth, fullyHeal);
        health.SetStatBonusHealth(MaxHealthFromStamina);

        if (fullyHeal)
        {
            health.ResetHealth();
        }
    }

    public int GetScaledDamage()
    {
        if (enemyData == null)
        {
            return 0;
        }

        return ApplyPrimaryStatDamageScaling(enemyData.BaseDamage);
    }

    public int ApplyPrimaryStatDamageScaling(int baseAmount)
    {
        int statBonus = Mathf.RoundToInt(PrimaryStatValue * CombatTuning.PrimaryStatDamageMultiplier);
        return Mathf.Max(0, baseAmount + statBonus);
    }

    public int ApplyPrimaryStatHealingScaling(int baseAmount)
    {
        int statBonus = Mathf.RoundToInt(PrimaryStatValue * CombatTuning.PrimaryStatHealingMultiplier);
        return Mathf.Max(0, baseAmount + statBonus);
    }

    public int ReduceIncomingDamageByArmour(int incomingDamage)
    {
        int damage = Mathf.Max(0, incomingDamage);

        if (damage <= 0)
        {
            return 0;
        }

        float mitigationBase = CombatTuning.ArmourMitigationBase;
        float multiplier = mitigationBase / (mitigationBase + Mathf.Max(0, Armour));

        return Mathf.Max(1, Mathf.RoundToInt(damage * multiplier));
    }

    public bool RollDodge()
    {
        float roll = UnityEngine.Random.Range(0f, 100f);
        return roll < DodgeChancePercent;
    }
}