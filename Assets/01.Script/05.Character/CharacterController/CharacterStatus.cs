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
    public long Attack { get; private set; } //기본 공격력
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
    public long Defense { get; private set; } //기본 방어도
    public int DodgeRate { get; private set; } //회피


    public CharacterStatus()
    {
        Level = 1;
        Exp = 0;

        MaxHp = 100;
        CurrentHp = MaxHp;
        MaxMp = 100;
        CurrentMp = MaxMp;
        RecoverMpPerSec = 5;

        MoveSpeed = 5f;

        Strength = 10;
        Dexterity = 10;
        Intelligence = 10;
        Luck = 10;

        Attack = 10;
        AttackSpeedRate = 0f; // 1+0f배의 공격속도
        HitRate = 10;
        CriticalRate = 0.1f; // 10%확률로 치명타
        CriticalDamage = 0.5f; // 치명타 발생 시 1+0.5f배 피해

        DamageByMainStat = 0.01f; //주 스탯의 1당 피해 증가율 1%증가
        DamageOnBoss = 0f; // 보스 몬스터에게 1+0f배의 피해
        DamageOnNormal = 0f; // 일반 몬스터에게 1+0f배의 피해

        ArmorPenetration = 0f; // 상대 방어도 0f*100%의 비율 만큼 무시
        FinalDamage = 0f; // 임의상 단순히 0f로 작성. 각 데미지 증가 수치를 곱연산(혹은 합연산)후 최종 데미지 계산

        Defense = 0; // 0만큼 피해 감소
        DodgeRate = 0; // 상대의 명중보다 높으면 회피확률 발생
    }

    public void AddExp(int amount)
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

    public void RecoverHp(int amount)
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

    public void IncreaseAttack(long amount)
    {
        if (amount <= 0)
            return;

        Attack += amount;
    }

    public void IncreaseDefense(long amount)
    {
        if (amount <= 0)
            return;

        Defense += amount;
    }
}
