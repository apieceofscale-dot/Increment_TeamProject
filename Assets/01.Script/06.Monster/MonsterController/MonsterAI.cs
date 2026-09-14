using UnityEngine;

public sealed class MonsterAI
{
    readonly IdleState _idle = new IdleState();
    readonly TraceState _trace = new TraceState();
    readonly AttackState _attack = new AttackState();
    readonly DeadState _dead = new DeadState();

    IMonsterFsmState _current;
    MonsterController _monster;
    float _attackTimer;

    public MonsterState State => _current != null ? _current.State : MonsterState.Idle;

    public void Bind(MonsterController monster)
    {
        _monster = monster;
    }

    public void Reset()
    {
        _attackTimer = 0f;
        ChangeState(MonsterState.Idle);
    }

    public void ForceDead()
    {
        ChangeState(MonsterState.Dead);
    }

    public void Tick(float deltaTime)
    {
        if (_monster == null || _monster.Status.IsDead)
        {
            ForceDead();
            return;
        }

        _current?.Tick(_monster, deltaTime);
    }

    public void ChangeState(MonsterState next)
    {
        if (_current != null && _current.State == next)
        {
            return;
        }

        _current?.Exit(_monster);
        _current = Resolve(next);
        _monster?.Status.SetState(next);
        _current?.Enter(_monster);
    }

    public Transform GetTarget()
    {
        return _monster != null ? _monster.FindTarget() : null;
    }

    public bool HasTarget()
    {
        return GetTarget() != null;
    }

    public float GetDistanceToTarget()
    {
        Transform target = GetTarget();
        if (target == null || _monster == null)
        {
            return float.MaxValue;
        }

        return Vector3.Distance(_monster.transform.position, target.position);
    }

    public bool IsWithinTraceRange()
    {
        return GetDistanceToTarget() <= _monster.Status.TraceRange;
    }

    public bool IsWithinAttackRange()
    {
        return GetDistanceToTarget() <= _monster.Status.AttackRange;
    }

    public bool ShouldEnterTrace()
    {
        return HasTarget() && IsWithinTraceRange();
    }

    public MonsterState EvaluateFromIdle()
    {
        if (!HasTarget())
        {
            return MonsterState.Idle;
        }

        if (IsWithinAttackRange())
        {
            return MonsterState.Attack;
        }

        if (IsWithinTraceRange())
        {
            return MonsterState.Trace;
        }

        return MonsterState.Idle;
    }

    public MonsterState EvaluateFromTrace()
    {
        if (!HasTarget())
        {
            return MonsterState.Idle;
        }

        if (IsWithinAttackRange())
        {
            return MonsterState.Attack;
        }

        if (!IsWithinTraceRange())
        {
            return MonsterState.Idle;
        }

        return MonsterState.Trace;
    }

    public MonsterState EvaluateFromAttack()
    {
        if (!HasTarget())
        {
            return MonsterState.Idle;
        }

        if (!IsWithinAttackRange())
        {
            return IsWithinTraceRange() ? MonsterState.Trace : MonsterState.Idle;
        }

        return MonsterState.Attack;
    }

    public void ExecuteTrace()
    {
        Transform target = GetTarget();
        if (target == null || _monster == null)
        {
            return;
        }

        _monster.TraceTowards(target.position);
    }

    public void ExecuteAttack(float deltaTime)
    {
        Transform target = GetTarget();
        if (target == null || _monster == null)
        {
            return;
        }

        _attackTimer -= deltaTime;
        if (_attackTimer > 0f)
        {
            return;
        }

        _monster.PerformAttack(target);
        _attackTimer = _monster.Status.AttackCooldown;
    }

    public void ResetAttackTimer()
    {
        _attackTimer = 0f;
    }

    IMonsterFsmState Resolve(MonsterState state)
    {
        switch (state)
        {
            case MonsterState.Trace:
                return _trace;
            case MonsterState.Attack:
                return _attack;
            case MonsterState.Dead:
                return _dead;
            default:
                return _idle;
        }
    }
}
