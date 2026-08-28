using UnityEngine;

public sealed class TraceState : IMonsterFsmState
{
    public MonsterState State => MonsterState.Trace;

    public void Enter(MonsterController monster) { }

    public void Tick(MonsterController monster, float deltaTime)
    {
        var target = monster.FindTarget();
        var data = monster.Status.Data;
        if (target == null)
        {
            monster.AI.ChangeState(MonsterState.Idle);
            return;
        }

        var distance = Vector3.Distance(monster.transform.position, target.position);
        if (distance > data.traceRange)
        {
            monster.AI.ChangeState(MonsterState.Idle);
            return;
        }

        if (distance <= data.attackRange)
        {
            monster.AI.ChangeState(MonsterState.Attack);
            return;
        }

        monster.MoveTowards(target.position, deltaTime);
    }

    public void Exit(MonsterController monster) { }
}
