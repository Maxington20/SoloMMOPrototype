using System;
using UnityEngine;

[RequireComponent(typeof(PlayerClassController))]
[RequireComponent(typeof(PlayerEquipment))]
[RequireComponent(typeof(PlayerProgression))]
public class PlayerStats : MonoBehaviour
{
    [Header("Derived Stat Settings")]
    [SerializeField] private float dodgeChancePerAgility = 0.25f;
    [SerializeField] private float maxDodgeChance = 25f;

    private PlayerClassController classController;
    private PlayerEquipment equipment;
    private PlayerProgression progression;

    public event Action OnStatsChanged;

    public StatBlock TotalStats => CalculateTotalStats();

    public PrimaryStatType PrimaryStat
    {
        get
        {
            CharacterClassData selectedClass = GetSelectedClass();
            return selectedClass != null ? selectedClass.PrimaryStat : PrimaryStatType.Strength;
        }
    }

    public int PrimaryStatValue => TotalStats.GetPrimaryValue(PrimaryStat);

    public float DodgeChancePercent
    {
        get
        {
            float chance = TotalStats.Agility * dodgeChancePerAgility;
            return Mathf.Clamp(chance, 0f, maxDodgeChance);
        }
    }

    public int Armor => TotalStats.Armor;

    private void Awake()
    {
        classController = GetComponent<PlayerClassController>();
        equipment = GetComponent<PlayerEquipment>();
        progression = GetComponent<PlayerProgression>();
    }

    private void OnEnable()
    {
        if (equipment != null)
        {
            equipment.OnEquipmentChanged += NotifyStatsChanged;
        }
    }

    private void OnDisable()
    {
        if (equipment != null)
        {
            equipment.OnEquipmentChanged -= NotifyStatsChanged;
        }
    }

    public void NotifyStatsChanged()
    {
        OnStatsChanged?.Invoke();
    }

    public StatBlock CalculateTotalStats()
    {
        CharacterClassData selectedClass = GetSelectedClass();

        if (selectedClass == null)
        {
            return StatBlock.Zero;
        }

        int level = progression != null ? progression.Level : 1;

        StatBlock total = selectedClass.StartingStats
            .Add(selectedClass.GetLevelBonusStats(level))
            .Add(GetEquipmentStats());

        return total;
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

    private CharacterClassData GetSelectedClass()
    {
        return classController != null ? classController.SelectedClass : null;
    }
}