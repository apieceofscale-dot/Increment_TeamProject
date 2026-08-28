using UnityEngine;

public sealed class IdleState : IMonsterFsmState
{
    public MonsterState State => MonsterState.Idle;

    public void Enter(MonsterController monster) { }

    public void Tick(MonsterController monster, float deltaTime)
    {
        var target = monster.FindTarget();
        if (target == null)
        {
            return;
        }

        var distance = Vector3.Distance(monster.transform.position, target.position);
        if (distance <= monster.Status.Data.traceRange)
        {
            monster.AI.ChangeState(MonsterState.Trace);
        }
    }

    public void Exit(MonsterController monster) { }
}
