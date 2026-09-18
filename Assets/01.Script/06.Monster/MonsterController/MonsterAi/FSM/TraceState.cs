public sealed class TraceState : IMonsterFsmState
{
    public MonsterState State => MonsterState.Trace;

    public void Enter(MonsterController monster) { }

    public void Tick(MonsterController monster, float deltaTime)
    {
        MonsterState next = monster.AI.EvaluateFromTrace();
        if (next != MonsterState.Trace)
        {
            monster.AI.ChangeState(next);
            return;
        }

        monster.AI.ExecuteTrace();
    }

    public void Exit(MonsterController monster)
    {
        monster.StopNavigation();
    }
}
