using UnityEngine;

public sealed class MonsterStatus
{
    public int Id { get; private set; }
    public MonsterState State { get; private set; }
    public Color PaletteColor { get; private set; }
    public int MaxHp { get; private set; }
    public int CurrentHp { get; private set; }
    public int AttackDamage { get; private set; }
    public float MoveSpeed { get; private set; }
    public float TraceRange { get; private set; }
    public float AttackRange { get; private set; }
    public float AttackCooldown { get; private set; }
    public int DropItemId { get; private set; }
    public float DropChance { get; private set; }

    public bool IsDead => State == MonsterState.Dead || CurrentHp <= 0;

    public void Reset(
        int id,
        int maxHp,
        int attackDamage,
        float moveSpeed,
        float traceRange,
        float attackRange,
        float attackCooldown,
        int dropItemId,
        float dropChance,
        Color paletteColor)
    {
        Id = id;
        MaxHp = Mathf.Max(1, maxHp);
        CurrentHp = MaxHp;
        AttackDamage = Mathf.Max(1, attackDamage);
        MoveSpeed = moveSpeed;
        TraceRange = traceRange;
        AttackRange = attackRange;
        AttackCooldown = Mathf.Max(0.1f, attackCooldown);
        DropItemId = dropItemId;
        DropChance = Mathf.Clamp01(dropChance);
        PaletteColor = paletteColor;
        State = MonsterState.Idle;
    }

    public void ApplyStageMultiplier(float multiplier)
    {
        var scale = Mathf.Max(0.01f, multiplier);
        MaxHp = Mathf.Max(1, Mathf.RoundToInt(MaxHp * scale));
        CurrentHp = MaxHp;
        AttackDamage = Mathf.Max(1, Mathf.RoundToInt(AttackDamage * scale));
        MoveSpeed *= scale;
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
        DropItemId = 0;
        DropChance = 0f;
    }
}
