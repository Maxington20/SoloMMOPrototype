using System;
using UnityEngine;

[Serializable]
public class StatBlock
{
    [SerializeField] private int strength;
    [SerializeField] private int agility;
    [SerializeField] private int intellect;
    [SerializeField] private int stamina;
    [SerializeField] private int armor;

    public int Strength => strength;
    public int Agility => agility;
    public int Intellect => intellect;
    public int Stamina => stamina;
    public int Armor => armor;

    public StatBlock()
    {
    }

    public StatBlock(int strength, int agility, int intellect, int stamina, int armor)
    {
        this.strength = strength;
        this.agility = agility;
        this.intellect = intellect;
        this.stamina = stamina;
        this.armor = armor;
    }

    public static StatBlock Zero => new StatBlock(0, 0, 0, 0, 0);

    public StatBlock Add(StatBlock other)
    {
        if (other == null)
        {
            return Clone();
        }

        return new StatBlock(
            strength + other.Strength,
            agility + other.Agility,
            intellect + other.Intellect,
            stamina + other.Stamina,
            armor + other.Armor);
    }

    public StatBlock Multiply(int amount)
    {
        return new StatBlock(
            strength * amount,
            agility * amount,
            intellect * amount,
            stamina * amount,
            armor * amount);
    }

    public StatBlock Clone()
    {
        return new StatBlock(strength, agility, intellect, stamina, armor);
    }

    public int GetPrimaryValue(PrimaryStatType primaryStatType)
    {
        return primaryStatType switch
        {
            PrimaryStatType.Strength => strength,
            PrimaryStatType.Agility => agility,
            PrimaryStatType.Intellect => intellect,
            _ => 0
        };
    }
}