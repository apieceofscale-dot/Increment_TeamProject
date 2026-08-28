using UnityEngine;

public sealed class AttackState : IMonsterFsmState
{
    public MonsterState State => MonsterState.Attack;

    float _attackTimer;

    public void Enter(MonsterController monster)
    {
        _attackTimer = 0f;
    }

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
        if (distance > data.attackRange)
        {
            monster.AI.ChangeState(MonsterState.Trace);
            return;
        }

        _attackTimer -= deltaTime;
        if (_attackTimer <= 0f)
        {
            monster.PerformAttack(target);
            _attackTimer = Mathf.Max(0.1f, data.attackCooldown);
        }
    }

    public void Exit(MonsterController monster) { }
}
