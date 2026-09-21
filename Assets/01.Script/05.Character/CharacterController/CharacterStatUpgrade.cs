using System;

public sealed class CharacterStatUpgrade
{
    public const int UpgradeCost = 1;
    public const int MainStatIncrease = 2;
    public const int DefenceIncrease = 2;
    public const int MaxHpIncrease = 10;
    public const int MainStatLimit = 10;
    public const int DefenceLimit = 5;
    public const int MaxHpLimit = 5;
    public const int SpecialPointsPerRank = 10;
    private readonly CharacterStatus status;
    private int points;
    private int specialPoints;
    private int mainCount;
    private int defenceCount;
    private int hpCount;
    private int rank = 1;

    public CharacterStatUpgrade(CharacterStatus status)
    {
        this.status = status ?? throw new ArgumentNullException(nameof(status));
    }

    public CharacterStatUpgradeInfo GetInfo() => new CharacterStatUpgradeInfo(points, specialPoints, rank, mainCount, defenceCount, hpCount);

    internal bool CanRewardLevelUp => points <= int.MaxValue - CharacterLevelUpProvider.StatPointsPerLevel;
    internal void RewardLevelUp()
    {
        if (!CanRewardLevelUp)
            throw new InvalidOperationException("능력치 포인트 한도 도달");

        points += CharacterLevelUpProvider.StatPointsPerLevel;
    }

    public CharacterStatUpgradeOption GetOption(CharacterStatUpgradeType type, CharacterDamageMainStat mainStat, bool canRun = true)
    {
        int count;
        int limit;
        int increase;
        int main = 0;
        long defence = 0;
        long hp = 0;

        switch (type)
        {
            case CharacterStatUpgradeType.MainStat:
                count = mainCount;
                limit = MainStatLimit;
                increase = MainStatIncrease;
                main = increase;
                break;
            case CharacterStatUpgradeType.Defence:
                count = defenceCount;
                limit = DefenceLimit;
                increase = DefenceIncrease;
                defence = increase;
                break;
            case CharacterStatUpgradeType.MaxHp:
                count = hpCount;
                limit = MaxHpLimit;
                increase = MaxHpIncrease;
                hp = increase;
                break;
            default:
                return new CharacterStatUpgradeOption(type, 0, 0, 0, 0, false);
        }

        bool rankUp = mainCount + (main > 0 ? 1 : 0) == MainStatLimit && defenceCount + (defence > 0 ? 1 : 0) == DefenceLimit && hpCount + (hp > 0 ? 1 : 0) == MaxHpLimit;
        bool canUpgrade = canRun && points >= UpgradeCost && count < limit && (!rankUp || (rank < int.MaxValue && specialPoints <= int.MaxValue - SpecialPointsPerRank)) && status.TryApplyGrowth(mainStat, main, 0, defence, hp, 0, false, apply: false);
        return new CharacterStatUpgradeOption(type, UpgradeCost, increase, count, limit, canUpgrade);
    }

    internal bool TryUpgrade(CharacterStatUpgradeType type, CharacterDamageMainStat mainStat)
    {
        if (points <= 0)
            return false;

        int nextMain = mainCount;
        int nextDefence = defenceCount;
        int nextHp = hpCount;
        int main = 0;
        long defence = 0;
        long hp = 0;

        switch (type)
        {
            case CharacterStatUpgradeType.MainStat: // 포인트당 주스탯 2상승
                if (mainCount >= MainStatLimit)
                    return false;

                nextMain++; main = 2; break;
            case CharacterStatUpgradeType.Defence: // 포인트당 방어력 2상승
                if (defenceCount >= DefenceLimit)
                    return false;

                nextDefence++; defence = 2; break;
            case CharacterStatUpgradeType.MaxHp: // 포인트당 최대 체력 10 상승
                if (hpCount >= MaxHpLimit)
                    return false;

                nextHp++; hp = 10; break;
            default:
                return false;
        }
        bool rankUp = nextMain == MainStatLimit && nextDefence == DefenceLimit && nextHp == MaxHpLimit;

        if (rankUp && (rank == int.MaxValue || specialPoints > int.MaxValue - SpecialPointsPerRank))
            return false;

        if (!status.TryApplyGrowth(mainStat, main, 0, defence, hp, 0, false))
            return false;

        points--;
        mainCount = nextMain;
        defenceCount = nextDefence;
        hpCount = nextHp;

        if (rankUp)
        {
            rank++;
            specialPoints += SpecialPointsPerRank;
            mainCount = defenceCount = hpCount = 0;
        }

        return true;
    }
}
