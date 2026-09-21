using System;

public class CharacterLevelUpProvider
{
    private const long BaseRequiredExp = 100;
    private const double ExpGrowthRate = 1.15;
    public const int StatPointsPerLevel = 5;

    public long GetRequiredExp(int level)
    {
        double required = BaseRequiredExp * Math.Pow(ExpGrowthRate, Math.Max(1, level) - 1);

        if (level == int.MaxValue || double.IsInfinity(required) || required >= long.MaxValue)
            return 0;

        return (long)Math.Round(required);
    }

    public long GetMaxHpGrowth(int level) => 10;
    public int GetAttackGrowth(int level) => 1;
    public int GetDefenceGrowth(int level) => 1;
    public int GetMainStatGrowth(int level) => 1;
    public int GetMaxMpGrowth(int level) => 2;

    internal bool TryLevelUp(CharacterStatus status, CharacterDamageMainStat mainStat)
    {
        if (status == null)
            return false;

        long required = GetRequiredExp(status.Level);

        if (required <= 0 || status.Exp < required)
            return false;

        int nextLevel = status.Level + 1;

        if (!status.TryApplyGrowth(mainStat, GetMainStatGrowth(nextLevel), GetAttackGrowth(nextLevel), GetDefenceGrowth(nextLevel), GetMaxHpGrowth(nextLevel), GetMaxMpGrowth(nextLevel), true))
            return false;
        
        status.UseExp(required);
        status.IncreaseLevel();
        return true;
    }
}
