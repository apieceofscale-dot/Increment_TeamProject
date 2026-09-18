public sealed class DeadState : IMonsterFsmState
{
    public MonsterState State => MonsterState.Dead;

    public void Enter(MonsterController monster)
    {
        monster.StopNavigation();
    }

    public void Tick(MonsterController monster, float deltaTime) { }

    public void Exit(MonsterController monster) { }
}
