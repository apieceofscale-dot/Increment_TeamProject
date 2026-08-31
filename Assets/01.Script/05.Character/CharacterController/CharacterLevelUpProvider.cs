using System;

public class CharacterLevelUpProvider
{
    private const long BaseRequiredExp = 100;
    private const double ExpGrowthRate = 1.15; // 임시로 1.15로 지정, 추후 기획에 따라 변경예정
    private const long BaseHpGrowth = 10;
    private const long BaseAttackGrowth = 2;
    private const long BaseDefenseGrowth = 1;

    public long GetRequireExp(int level)
    {
        if (level < 1)
            level = 1;
        double requiredExp = BaseRequiredExp * Math.Pow(ExpGrowthRate, level - 1);

        return (long)requiredExp;
    }

    // 아래는 임시 공식들

    public long GetMaxHpGrowth(int level)
    {
        if(level < 1)
            level = 1;

        return BaseHpGrowth + level;
    }

    public long GetAttackGrowth(int level)
    {
        if (level < 1)
            level = 1;

        return BaseAttackGrowth + level / 5;
    }

    public long GetDefenseGrowth(int level)
    {
        if (level < 1)
            level = 1;

        return BaseDefenseGrowth + level / 10;
    }
}
