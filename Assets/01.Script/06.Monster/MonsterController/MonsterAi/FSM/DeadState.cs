public sealed class DeadState : IMonsterFsmState
{
    public MonsterState State => MonsterState.Dead;

    public void Enter(MonsterController monster) { }

    public void Tick(MonsterController monster, float deltaTime) { }

    public void Exit(MonsterController monster) { }
}
