using System;
using UnityEngine;

[RequireComponent(typeof(PlayerClassController))]
[RequireComponent(typeof(PlayerEquipment))]
[RequireComponent(typeof(PlayerProgression))]
[RequireComponent(typeof(Health))]
public class PlayerStats : MonoBehaviour, ICombatStatsProvider
{
    private PlayerClassController classController;
    private PlayerEquipment equipment;
    private PlayerProgression progression;
    private Health health;

    public event Action OnStatsChanged;

    public StatBlock TotalStats => CalculateTotalStats();
    public StatBlock BaseStats => CalculateBaseStats();
    public StatBlock LevelBonusStats => CalculateLevelBonusStats();
    public StatBlock GearStats => GetEquipmentStats();

    public CharacterClassData SelectedClass => classController != null ? classController.SelectedClass : null;

    public ClassCombatTuning CombatTuning
    {
        get
        {
            if (SelectedClass != null)
            {
                return SelectedClass.CombatTuning;
            }

            return new ClassCombatTuning();
        }
    }

    public PrimaryStatType PrimaryStat
    {
        get
        {
            return SelectedClass != null
                ? SelectedClass.PrimaryStat
                : PrimaryStatType.Strength;
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
        classController = GetComponent<PlayerClassController>();
        equipment = GetComponent<PlayerEquipment>();
        progression = GetComponent<PlayerProgression>();
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        if (equipment != null)
        {
            equipment.OnEquipmentChanged += RecalculateAndApplyStats;
        }

        if (progression != null)
        {
            progression.OnLevelChanged += RecalculateAndApplyStats;
        }
    }

    private void OnDisable()
    {
        if (equipment != null)
        {
            equipment.OnEquipmentChanged -= RecalculateAndApplyStats;
        }

        if (progression != null)
        {
            progression.OnLevelChanged -= RecalculateAndApplyStats;
        }
    }

    private void Start()
    {
        RecalculateAndApplyStats();
    }

    public void RecalculateAndApplyStats()
    {
        if (health != null)
        {
            health.SetStatBonusHealth(MaxHealthFromStamina);
        }

        OnStatsChanged?.Invoke();
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

    public StatBlock CalculateTotalStats()
    {
        return CalculateBaseStats()
            .Add(CalculateLevelBonusStats())
            .Add(GetEquipmentStats());
    }

    public StatBlock CalculateBaseStats()
    {
        if (SelectedClass == null)
        {
            return StatBlock.Zero;
        }

        return SelectedClass.StartingStats;
    }

    public StatBlock CalculateLevelBonusStats()
    {
        if (SelectedClass == null)
        {
            return StatBlock.Zero;
        }

        int level = progression != null ? progression.Level : 1;
        return SelectedClass.GetLevelBonusStats(level);
    }

    public StatBlock GetEquipmentStats()
    {
        if (equipment == null)
        {
            return StatBlock.Zero;
        }

        StatBlock total = StatBlock.Zero;

        total = total.Add(GetStatsFromSlot(EquipmentSlotType.Head));
        total = total.Add(GetStatsFromSlot(EquipmentSlotType.Chest));
        total = total.Add(GetStatsFromSlot(EquipmentSlotType.Legs));
        total = total.Add(GetStatsFromSlot(EquipmentSlotType.Feet));
        total = total.Add(GetStatsFromSlot(EquipmentSlotType.Weapon));
        total = total.Add(GetStatsFromSlot(EquipmentSlotType.Offhand));

        return total;
    }

    private StatBlock GetStatsFromSlot(EquipmentSlotType slotType)
    {
        ItemData item = equipment.GetEquippedItem(slotType);
        return item != null ? item.StatBonuses : StatBlock.Zero;
    }
}