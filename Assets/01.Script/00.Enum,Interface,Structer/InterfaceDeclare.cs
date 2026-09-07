using System;
using System.Collections.Generic;

public interface IBootStrapper
{
    int BootOrder { get; }
    void IBootStrapperInject(BootstrapContext context);
    void IBootStrapperInitialize();
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

public interface IDropTableSource
{
    bool TryGetEntries(int dropTableId, out IReadOnlyList<DropTableEntry> entries);
}
