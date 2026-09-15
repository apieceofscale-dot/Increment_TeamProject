public sealed class IdleState : IMonsterFsmState
{
    public MonsterState State => MonsterState.Idle;

    public void Enter(MonsterController monster) { }

    public void Tick(MonsterController monster, float deltaTime)
    {
        MonsterState next = monster.AI.EvaluateFromIdle();
        if (next != MonsterState.Idle)
        {
            monster.AI.ChangeState(next);
        }
    }

    public void Exit(MonsterController monster) { }
}
