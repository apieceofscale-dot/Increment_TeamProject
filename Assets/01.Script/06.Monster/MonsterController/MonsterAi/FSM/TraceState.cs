using UnityEngine;

public sealed class TraceState : IMonsterFsmState
{
    public MonsterState State => MonsterState.Trace;

    public void Enter(MonsterController monster) { }

    public void Tick(MonsterController monster, float deltaTime)
    {
        var target = monster.FindTarget();
        if (target == null)
        {
            monster.AI.ChangeState(MonsterState.Idle);
            return;
        }

        var distance = Vector3.Distance(monster.transform.position, target.position);
        if (distance > monster.Status.TraceRange)
        {
            monster.AI.ChangeState(MonsterState.Idle);
            return;
        }

        if (distance <= monster.Status.AttackRange)
        {
            monster.AI.ChangeState(MonsterState.Attack);
            return;
        }

        monster.MoveTowards(target.position, deltaTime);
    }

    public void Exit(MonsterController monster) { }
}
