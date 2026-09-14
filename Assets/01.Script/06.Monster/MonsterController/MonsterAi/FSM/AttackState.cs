public sealed class AttackState : IMonsterFsmState
{
    public MonsterState State => MonsterState.Attack;

    public void Enter(MonsterController monster)
    {
        monster.AI.ResetAttackTimer();
    }

    public void Tick(MonsterController monster, float deltaTime)
    {
        MonsterState next = monster.AI.EvaluateFromAttack();
        if (next != MonsterState.Attack)
        {
            monster.AI.ChangeState(next);
            return;
        }

        monster.AI.ExecuteAttack(deltaTime);
    }

    public void Exit(MonsterController monster) { }
}
