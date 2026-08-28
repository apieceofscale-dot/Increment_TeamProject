using UnityEngine;

public sealed class MonsterStatus
{
    public int Id { get; private set; }
    public MonsterState State { get; private set; }
    public MonsterData Data { get; private set; }
    public Color PaletteColor { get; private set; }
    public int CurrentHp { get; private set; }

    public bool IsDead => State == MonsterState.Dead || CurrentHp <= 0;

    public void Reset(int id, MonsterData data, Color paletteColor)
    {
        Id = id;
        Data = data;
        PaletteColor = paletteColor;
        CurrentHp = Mathf.Max(1, data != null ? data.maxHp : 10);
        State = MonsterState.Idle;
    }

    public void SetState(MonsterState state)
    {
        State = state;
    }

    public bool ApplyDamage(int amount)
    {
        if (IsDead || amount <= 0)
        {
            return false;
        }

        CurrentHp = Mathf.Max(0, CurrentHp - amount);
        if (CurrentHp == 0)
        {
            State = MonsterState.Dead;
            return true;
        }

        return false;
    }

    public void Clear()
    {
        CurrentHp = 0;
        State = MonsterState.Idle;
        Data = null;
    }
}
