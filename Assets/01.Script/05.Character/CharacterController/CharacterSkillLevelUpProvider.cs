using System;

public class CharacterSkillLevelUpProvider
{
    // 일단 임시로 수치 지정, 추후 스킬 데이터 테이블을 따라가는 것으로 할 예정
    private const long BaseUpgradeCost = 100;
    private const double UpgradeCostGrowthRate = 1.2;
    private const float BaseDamageMultiplier = 1.0f;
    private const float DamageGrowthPerLevel = 0.1f;
    private const int BaseMpCost = 10;
    private const int MpCostIncreaseInterval = 5;
    private const int MpCostIncreaseAmount = 1;
    private const float BaseCooldown = 5f;
    private const float CooldownReductionPerLevel = 0.05f;
    private const float MinimumCooldown = 1f;

    public long GetRequiredUpgradeCost(int level) // 스킬 레벨에 따라 강화비용 증가
    {
        if (level < 1)
            level = 1;
        double requiredCost = BaseUpgradeCost * Math.Pow(UpgradeCostGrowthRate, level - 1);

        return (long)Math.Round(requiredCost);

    }

    public float GetDamageMultiplier(int level)
    {
        if (level < 1)
            level = 1;

        return BaseDamageMultiplier + DamageGrowthPerLevel * (level - 1);
    }

    public int GetMpCost(int level) // MP코스트 증가용, 기획에 따라 안쓸지도?
    {
        if (level < 1)
            level = 1;
        int increaseCount = (level - 1) / MpCostIncreaseInterval;

        return BaseMpCost + increaseCount * MpCostIncreaseAmount;

    }

    public float GetCooldown(int level) // 쿨다운 계산용, 마찬가지로 기획에 따라 안쓸 수도 있을듯
    {
        if (level < 1)
            level = 1;
        float cooldown = BaseCooldown - CooldownReductionPerLevel * (level - 1);

        if (cooldown < MinimumCooldown)
            cooldown = MinimumCooldown;

        return cooldown;
    }
}
