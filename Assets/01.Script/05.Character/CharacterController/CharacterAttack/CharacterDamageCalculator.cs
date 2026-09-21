using System;

public static class CharacterDamageCalculator
{
    public const double BaseCriticalMultiplier = 1.5;
    public const double DefenceScale = 100.0;
    public const double AccuracyScale = 100.0;
    public static CharacterDamageResult Calculate(CharacterDamageAttackData attacker, double skillMultiplier, CharacterDamageTargetData target, double hitRoll, double criticalRoll)
    {
        double attack = NonNegative(attacker.Attack);
        double skill = NonNegative(skillMultiplier);

        if (attack == 0 || skill == 0)
            return new CharacterDamageResult(0, false, false);

        double hitChance = GetHitChance(attacker, target);

        if (hitChance < 1 && Roll(hitRoll) >= hitChance)
            return new CharacterDamageResult(0, false, true);

        double criticalChance = Rate(attacker.CriticalRate);
        bool critical = criticalChance >= 1 || (criticalChance > 0 && Roll(criticalRoll) < criticalChance);
        double damage = attack
            * (1 + NonNegative(attacker.MainStat) * NonNegative(attacker.DamageByMainStat))
            * skill
            * (critical ? BaseCriticalMultiplier + NonNegative(attacker.CriticalDamage) : 1)
            * (1 + NonNegative(attacker.FinalDamage))
            * GetTargetMultiplier(attacker, target)
            * GetDefenceMultiplier(attacker, target);

        if (double.IsNaN(damage))
            return new CharacterDamageResult(0, false, false);

        int amount = damage >= int.MaxValue ? int.MaxValue : (int)Math.Max(1, Math.Floor(damage));
        return new CharacterDamageResult(amount, critical, false);
    }

    private static double GetTargetMultiplier(CharacterDamageAttackData a, CharacterDamageTargetData t)
    {
        switch (t.Kind)
        {
            case CharacterDamageTargetKind.Normal:
                return 1 + NonNegative(a.DamageOnNormal);
            case CharacterDamageTargetKind.Boss:
                return 1 + NonNegative(a.DamageOnBoss);
            default:
                return 1;
        }
    }

    private static double GetDefenceMultiplier(CharacterDamageAttackData a, CharacterDamageTargetData t)
    {
        if (!t.Defence.HasValue)
            return 1;

        double effectiveDefence = NonNegative(t.Defence.Value) * (1 - Rate(a.ArmorPenetration));
        return DefenceScale / (DefenceScale + effectiveDefence);
    }

    private static double GetHitChance(CharacterDamageAttackData a, CharacterDamageTargetData t)
    {
        if (!t.DodgeRating.HasValue)
            return 1;

        double difference = Math.Max(0, NonNegative(t.DodgeRating.Value) - NonNegative(a.HitRating));
        return AccuracyScale / (AccuracyScale + difference);
    }

    private static double NonNegative(double value)
    {
        return double.IsNaN(value) || double.IsInfinity(value) ? 0 : Math.Max(0, value);
    }

    private static double Rate(double value)
    {
        return Math.Min(1, NonNegative(value));
    }

    private static double Roll(double value)
    {
        return Math.Min(0.9999999999999999, NonNegative(value));
    }
}
