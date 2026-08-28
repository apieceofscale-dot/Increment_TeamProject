public sealed class MonsterAI
{
    readonly IdleState _idle = new IdleState();
    readonly TraceState _trace = new TraceState();
    readonly AttackState _attack = new AttackState();
    readonly DeadState _dead = new DeadState();

    IMonsterFsmState _current;
    MonsterController _monster;

    public MonsterState State => _current != null ? _current.State : MonsterState.Idle;

    public void Bind(MonsterController monster)
    {
        _monster = monster;
    }

    public void Reset()
    {
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
