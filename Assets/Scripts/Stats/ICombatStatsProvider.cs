public interface ICombatStatsProvider
{
    float HitChancePercent { get; }
    float DodgeChancePercent { get; }
    int Armour { get; }
    int Armor { get; }

    int ApplyPrimaryStatDamageScaling(int baseAmount);
    int ApplyPrimaryStatHealingScaling(int baseAmount);
    int ReduceIncomingDamageByArmour(int incomingDamage);
    bool RollDodge();
}