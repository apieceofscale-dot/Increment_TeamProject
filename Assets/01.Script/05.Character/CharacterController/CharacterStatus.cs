using System;

public class CharacterStatus
{
    public int Level { get; private set; }
    public long Exp { get; private set; }
    public long CurrentHp { get; private set; }
    public int CurrentMp { get; private set; }

    public long BaseMaxHp { get; private set; }
    public long EquipMaxHp { get; private set; }
    public long MaxHp => BaseMaxHp + EquipMaxHp;

    public int BaseMaxMp { get; private set; }
    public int EquipMaxMp { get; private set; }
    public int MaxMp => BaseMaxMp + EquipMaxMp;

    public int BaseRecoverMpPerSec { get; private set; }
    public int EquipRecoverMpPerSec { get; private set; }
    public int RecoverMpPerSec => BaseRecoverMpPerSec + EquipRecoverMpPerSec;

    public float BaseMoveSpeed { get; private set; }
    public float EquipMoveSpeed { get; private set; }
    public float MoveSpeed => BaseMoveSpeed + EquipMoveSpeed;

    // 직업 별 주 스탯
    public int BaseStrength { get; private set; }
    public int EquipStrength { get; private set; }
    public int Strength => BaseStrength + EquipStrength;

    public int BaseDexterity { get; private set; }
    public int EquipDexterity { get; private set; }
    public int Dexterity => BaseDexterity + EquipDexterity;

    public int BaseIntelligence { get; private set; }
    public int EquipIntelligence { get; private set; }
    public int Intelligence => BaseIntelligence + EquipIntelligence;

    public int BaseLuck { get; private set; }
    public int EquipLuck { get; private set; }
    public int Luck => BaseLuck + EquipLuck;

    // 피해 관련
    public int BaseAttack { get; private set; }
    public int EquipAttack { get; private set; }
    public int Attack => BaseAttack + EquipAttack; //기본 공격력

    public float BaseAttackSpeedRate { get; private set; }
    public float EquipAttackSpeedRate { get; private set; }
    public float AttackSpeedRate => BaseAttackSpeedRate + EquipAttackSpeedRate; //공격 속도 증가율 최대 +150%(1+1.5f)

    public int BaseHitRate { get; private set; }
    public int EquipHitRate { get; private set; }
    public int HitRate => BaseHitRate + EquipHitRate; //명중

    public float BaseCriticalRate { get; private set; }
    public float EquipCriticalRate { get; private set; }
    public float CriticalRate => BaseCriticalRate + EquipCriticalRate; //치명타 확률

    public float BaseCriticalDamage { get; private set; }
    public float EquipCriticalDamage { get; private set; }
    public float CriticalDamage => BaseCriticalDamage + EquipCriticalDamage; //치명타 피해 증가율

    public float BaseDamageByMainStat { get; private set; }
    public float EquipDamageByMainStat { get; private set; }
    public float DamageByMainStat => BaseDamageByMainStat + EquipDamageByMainStat; //주 스탯당 데미지 증가율

    public float BaseDamageOnBoss { get; private set; }
    public float EquipDamageOnBoss { get; private set; }
    public float DamageOnBoss => BaseDamageOnBoss + EquipDamageOnBoss; //보스에게 주는 데미지 비율

    public float BaseDamageOnNormal { get; private set; }
    public float EquipDamageOnNormal { get; private set; }
    public float DamageOnNormal => BaseDamageOnNormal + EquipDamageOnNormal; //일반몹에게 주는 데미지 비율

    public float BaseArmorPenetration { get; private set; }
    public float EquipArmorPenetration { get; private set; }
    public float ArmorPenetration => BaseArmorPenetration + EquipArmorPenetration; //방어 관통

    public float BaseFinalDamage { get; private set; }
    public float EquipFinalDamage { get; private set; }
    public float FinalDamage => BaseFinalDamage + EquipFinalDamage; //최종 데미지 증가율

    // 방어 관련
    public long BaseDefence { get; private set; }
    public long EquipDefence { get; private set; }
    public long Defence => BaseDefence + EquipDefence; //기본 방어도

    public int BaseDodgeRate { get; private set; }
    public int EquipDodgeRate { get; private set; }
    public int DodgeRate => BaseDodgeRate + EquipDodgeRate; // 회피

    public void Initialize(PlayerData data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        Level = data.level;
        Exp = data.exp;

        BaseMaxHp = data.maxHp;
        BaseMaxMp = data.maxMp;
        BaseRecoverMpPerSec = data.recoverMpPerSec;

        BaseMoveSpeed = data.moveSpeed;

        BaseStrength = data.strength;
        BaseDexterity = data.dexterity;
        BaseIntelligence = data.intelligence;
        BaseLuck = data.luck;

        BaseAttack = data.attack;
        BaseAttackSpeedRate = data.attackSpeedRate;
        BaseHitRate = data.hitRate;

        BaseCriticalRate = data.criticalRate;
        BaseCriticalDamage = data.criticalDamage;

        BaseDamageByMainStat = data.damageByMainStat;
        BaseDamageOnBoss = data.damageOnBoss;
        BaseDamageOnNormal = data.damageOnNormal;

        BaseArmorPenetration = data.armorPenetration;
        BaseFinalDamage = data.finalDamage;

        BaseDefence = data.defence;
        BaseDodgeRate = data.dodgeRate;

        ClearEquipmentStats();

        CurrentHp = MaxHp;
        CurrentMp = MaxMp;
    }

    public void SetEquipmentStats(
        long maxHp = 0,
        int maxMp = 0,
        int recoverMpPerSec = 0,
        float moveSpeed = 0f,
        int strength = 0,
        int dexterity = 0,
        int intelligence = 0,
        int luck = 0,
        int attack = 0,
        float attackSpeedRate = 0f,
        int hitRate = 0,
        float criticalRate = 0f,
        float criticalDamage = 0f,
        float damageByMainStat = 0f,
        float damageOnBoss = 0f,
        float damageOnNormal = 0f,
        float armorPenetration = 0f,
        float finalDamage = 0f,
        long defence = 0,
        int dodgeRate = 0)
    {
        EquipMaxHp = maxHp;
        EquipMaxMp = maxMp;
        EquipRecoverMpPerSec = recoverMpPerSec;
        EquipMoveSpeed = moveSpeed;

        EquipStrength = strength;
        EquipDexterity = dexterity;
        EquipIntelligence = intelligence;
        EquipLuck = luck;

        EquipAttack = attack;
        EquipAttackSpeedRate = attackSpeedRate;
        EquipHitRate = hitRate;
        EquipCriticalRate = criticalRate;
        EquipCriticalDamage = criticalDamage;

        EquipDamageByMainStat = damageByMainStat;
        EquipDamageOnBoss = damageOnBoss;
        EquipDamageOnNormal = damageOnNormal;
        EquipArmorPenetration = armorPenetration;
        EquipFinalDamage = finalDamage;

        EquipDefence = defence;
        EquipDodgeRate = dodgeRate;

        // 장비 해제나 교체로 최대치가 감소하면 초과분만 줄임
        // 최대치가 증가했다고 현재 HP/MP를 자동 회복하지는 않음
        if (CurrentHp > MaxHp)
            CurrentHp = MaxHp;

        if (CurrentMp > MaxMp)
            CurrentMp = MaxMp;
    }

    public void ClearEquipmentStats()
    {
        SetEquipmentStats();
    }

    public void AddExp(long amount)
    {
        if (amount <= 0)
            return;

        Exp += amount;
    }

    public void UseExp(long amount)
    {
        if (amount <= 0)
            return;

        Exp -= amount;

        if (Exp < 0)
            Exp = 0;
    }

    public void IncreaseLevel()
    {
        Level++;
    }

    public void TakeDamage(long damage)
    {
        if (damage <= 0)
            return;

        CurrentHp -= damage;
        if (CurrentHp < 0)
            CurrentHp = 0;
    }

    public void RecoverHp(long amount)
    {
        if (amount <= 0)
            return;

        CurrentHp += amount;
        if (CurrentHp > MaxHp)
            CurrentHp = MaxHp;
    }

    public bool UseMp(int amount)
    {
        if (amount <= 0)
            return true;
        if (CurrentMp < amount)
            return false;

        CurrentMp -= amount;
        return true;
    }

    public void RecoverMp(int amount)
    {
        if (amount <= 0)
            return;

        CurrentMp += amount;
        if (CurrentMp > MaxMp)
            CurrentMp = MaxMp;
    }

    public void IncreaseMaxHp(long amount)
    {
        if (amount <= 0)
            return;

        BaseMaxHp += amount;
    }

    public void IncreaseAttack(int amount)
    {
        if (amount <= 0)
            return;

        BaseAttack += amount;
    }

    public void DecreaseAttack(int amount)
    {
        if (amount <= 0)
            return;

        BaseAttack -= amount;

        if (BaseAttack < 0)
            BaseAttack = 0;
    }

    public void IncreaseDefence(long amount)
    {
        if (amount <= 0)
            return;

        BaseDefence += amount;
    }

    internal bool TryApplyGrowth(CharacterDamageMainStat mainStat, int main, int attack, long defence, long hp, int mp, bool fullyHeal, bool apply = true)
    {
        if (main < 0 || attack < 0 || defence < 0 || hp < 0 || mp < 0)
            return false;

        int strength = BaseStrength;
        int dexterity = BaseDexterity;
        int intelligence = BaseIntelligence;
        int luck = BaseLuck;
        int nextAttack;
        int nextMp;
        long nextDefence;
        long nextHp;
        long nextCurrentHp;

        try
        {
            checked
            {
                switch (mainStat)
                {
                    case CharacterDamageMainStat.Strength:
                        strength += main; break;
                    case CharacterDamageMainStat.Dexterity:
                        dexterity += main; break;
                    case CharacterDamageMainStat.Intelligence:
                        intelligence += main; break;
                    case CharacterDamageMainStat.Luck:
                        luck += main; break;
                    default:
                        return false;
                }

                nextAttack = BaseAttack + attack;
                nextDefence = BaseDefence + defence;
                nextHp = BaseMaxHp + hp;
                nextMp = BaseMaxMp + mp;
                
                int totalStrength = strength + EquipStrength;
                int totalDexterity = dexterity + EquipDexterity;
                int totalIntelligence = intelligence + EquipIntelligence;
                int totalLuck = luck + EquipLuck;
                int totalAttack = nextAttack + EquipAttack;
                long totalDefence = nextDefence + EquipDefence;
                long totalHp = nextHp + EquipMaxHp;
                int totalMp = nextMp + EquipMaxMp;
                nextCurrentHp = fullyHeal ? totalHp : Math.Min(totalHp, CurrentHp + hp);
            }
        }
        catch (OverflowException) 
        { 
            return false; 
        }

        if (!apply)
            return true;

        BaseStrength = strength;
        BaseDexterity = dexterity;
        BaseIntelligence = intelligence;
        BaseLuck = luck;
        BaseAttack = nextAttack;
        BaseDefence = nextDefence;
        BaseMaxHp = nextHp;
        BaseMaxMp = nextMp;
        CurrentHp = nextCurrentHp;
        return true;
    }
}