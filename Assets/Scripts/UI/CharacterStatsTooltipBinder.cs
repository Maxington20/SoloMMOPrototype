using TMPro;
using UnityEngine;

public class CharacterStatsTooltipBinder : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private PlayerStats playerStats;

    [Header("Stat Texts")]
    [SerializeField] private TMP_Text armorText;
    [SerializeField] private TMP_Text dodgeText;
    [SerializeField] private TMP_Text hitText;
    [SerializeField] private TMP_Text strengthText;
    [SerializeField] private TMP_Text agilityText;
    [SerializeField] private TMP_Text intellectText;
    [SerializeField] private TMP_Text staminaText;

    private void Awake()
    {
        if (playerStats == null)
        {
            playerStats = FindFirstObjectByType<PlayerStats>();
        }
    }

    private void Update()
    {
        Refresh();
    }

    private void Refresh()
    {
        if (playerStats == null)
        {
            return;
        }

        StatBlock total = playerStats.TotalStats;
        StatBlock baseStats = playerStats.BaseStats;
        StatBlock raceStats = playerStats.RaceStats;
        StatBlock backgroundStats = playerStats.BackgroundStats;
        StatBlock levelStats = playerStats.LevelBonusStats;
        StatBlock gearStats = playerStats.GearStats;

        SetBreakdownStat(
            armorText,
            "Armour",
            total.Armor,
            baseStats.Armor,
            raceStats.Armor,
            backgroundStats.Armor,
            levelStats.Armor,
            gearStats.Armor);

        SetBreakdownStat(
            strengthText,
            "Strength",
            total.Strength,
            baseStats.Strength,
            raceStats.Strength,
            backgroundStats.Strength,
            levelStats.Strength,
            gearStats.Strength);

        SetBreakdownStat(
            agilityText,
            "Agility",
            total.Agility,
            baseStats.Agility,
            raceStats.Agility,
            backgroundStats.Agility,
            levelStats.Agility,
            gearStats.Agility);

        SetBreakdownStat(
            intellectText,
            "Intellect",
            total.Intellect,
            baseStats.Intellect,
            raceStats.Intellect,
            backgroundStats.Intellect,
            levelStats.Intellect,
            gearStats.Intellect);

        SetBreakdownStat(
            staminaText,
            "Stamina",
            total.Stamina,
            baseStats.Stamina,
            raceStats.Stamina,
            backgroundStats.Stamina,
            levelStats.Stamina,
            gearStats.Stamina);

        SetSimpleStat(
            dodgeText,
            "Dodge",
            $"{playerStats.DodgeChancePercent:0.#}%",
            $"Agility: {total.Agility}\nDodge per Agility: {playerStats.CombatTuning.DodgeChancePerAgility:0.##}%\nMax Dodge: {playerStats.CombatTuning.MaxDodgeChance:0.#}%\n\nCalculation:\n{total.Agility} × {playerStats.CombatTuning.DodgeChancePerAgility:0.##}% = {playerStats.DodgeChancePercent:0.#}%");

        SetSimpleStat(
            hitText,
            "Hit",
            $"{playerStats.HitChancePercent:0.#}%",
            $"Base Hit: {playerStats.CombatTuning.BaseHitChance:0.#}%\nBonus Hit from Stats/Gear: {total.HitChance}\nMax Hit: {playerStats.CombatTuning.MaxHitChance:0.#}%\n\nCalculation:\n{playerStats.CombatTuning.BaseHitChance:0.#}% + {total.HitChance} = {playerStats.HitChancePercent:0.#}%");
    }

    private void SetBreakdownStat(
        TMP_Text text,
        string label,
        int total,
        int baseValue,
        int raceValue,
        int backgroundValue,
        int levelValue,
        int gearValue)
    {
        if (text == null)
        {
            return;
        }

        text.text = $"{label}: {total}";

        string details =
            $"Total: {total}\n\n" +
            $"Base: {baseValue}\n" +
            $"Race: {raceValue}\n" +
            $"Background: {backgroundValue}\n" +
            $"Level: {levelValue}\n" +
            $"Gear: {gearValue}\n\n";
            //$"Calculation:\n" +
            //$"{baseValue} + {raceValue} + {backgroundValue} + {levelValue} + {gearValue} = {total}";

        SetTooltip(text, label, details);
    }

    private void SetSimpleStat(TMP_Text text, string label, string value, string details)
    {
        if (text == null)
        {
            return;
        }

        text.text = $"{label}: {value}";
        SetTooltip(text, label, details);
    }

    private void SetTooltip(TMP_Text text, string title, string details)
    {
        StatBreakdownTooltipTrigger trigger = text.GetComponent<StatBreakdownTooltipTrigger>();

        if (trigger == null)
        {
            trigger = text.gameObject.AddComponent<StatBreakdownTooltipTrigger>();
        }

        trigger.SetTooltip(title, details);
    }
}