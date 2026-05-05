using System;
using UnityEngine;

[Serializable]
public class CharacterStatModifier
{
    [SerializeField] private StatBlock statBonuses = StatBlock.Zero;

    public StatBlock StatBonuses => statBonuses ?? StatBlock.Zero;
}