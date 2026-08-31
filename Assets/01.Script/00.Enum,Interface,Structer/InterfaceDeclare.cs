using System;

public interface IBootStrapper
{
    /// 이 매니저 초기화가 끝나면 반드시 context.OnStepCompleted 호출할 것
    void IBootStrapperInitialize(BootstrapContext context);
}

public interface IPoolable
{
    void InitializePoolObj(Action returnAction);
    void OnSpawn();
    void OnDespawn();
}

public interface IDamageable
{
    void TakeDamage(int amount);
}

public interface IMonsterFsmState
{
    MonsterState State { get; }
    void Enter(MonsterController monster);
    void Tick(MonsterController monster, float deltaTime);
    void Exit(MonsterController monster);
}
