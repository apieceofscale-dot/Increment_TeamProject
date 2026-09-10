public class CharacterStatus
{
    public int Level { get; private set; }
    public long Exp { get; private set; }
    public long MaxHp { get; private set; }
    public long CurrentHp { get; private set; }
    public int MaxMp { get; private set; }
    public int CurrentMp { get; private set; }
    public int RecoverMpPerSec { get; private set; }
    public float MoveSpeed { get; private set; }

    // 직업 별 주 스탯
    public int Strength { get; private set; } //STR 힘
    public int Dexterity { get; private set; } //DEX 민첩
    public int Intelligence { get; private set; } //INT 지능
    public int Luck { get; private set; } //LUK 운

    // 피해 관련
    public int Attack { get; private set; } //기본 공격력
    public float AttackSpeedRate { get; private set; } //공격 속도 증가율 최대 +150%(1+1.5f)
    public int HitRate { get; private set; } //명중
    public float CriticalRate { get; private set; } //치명타 확률
    public float CriticalDamage { get; private set; } //치명타 피해 증가율
    public float DamageByMainStat { get; private set; } //주 스탯당 데미지 증가율
    public float DamageOnBoss { get; private set; } //보스에게 주는 데미지 비율
    public float DamageOnNormal { get; private set; } //일반몹에게 주는 데미지 비율
    public float ArmorPenetration { get; private set; } //방어 관통
    public float FinalDamage { get; private set; } //최종 데미지 증가율

    // 방어 관련
    public long Defence { get; private set; } //기본 방어도
    public int DodgeRate { get; private set; } //회피wwww

    public void Initialize(PlayerData data)
    {
        Level = data.level;
        Exp = data.exp;

        MaxHp = data.maxHp;
        CurrentHp = MaxHp;
        MaxMp = data.maxMp;
        CurrentMp = MaxMp;
        RecoverMpPerSec = data.recoverMpPerSec;

        MoveSpeed = data.moveSpeed;

        Strength = data.strength;
        Dexterity = data.dexterity;
        Intelligence = data.intelligence;
        Luck = data.luck;

        Attack = data.attack;
        AttackSpeedRate = data.attackSpeedRate;
        HitRate = data.hitRate;

        CriticalRate = data.criticalRate;
        CriticalDamage = data.criticalDamage;

        DamageByMainStat = data.damageByMainStat;
        DamageOnBoss = data.damageOnBoss;
        DamageOnNormal = data.damageOnNormal;

        ArmorPenetration = data.armorPenetration;
        FinalDamage = data.finalDamage;

        Defence = data.defence;
        DodgeRate = data.dodgeRate;

    }
    

    public void AddExp(long amount)
    {
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

        MaxHp += amount;
    }

    public void IncreaseAttack(int amount)
    {
        if (amount <= 0)
            return;

        Attack += amount;
    }

    public void DecreaseAttack(int amount)
    {
        if (amount <= 0)
            return;

        Attack -= amount;

        if (Attack < 0)
            Attack = 0;
    }

    public void IncreaseDefence(long amount)
    {
        if (amount <= 0)
            return;

        Defence += amount;
    }
}
