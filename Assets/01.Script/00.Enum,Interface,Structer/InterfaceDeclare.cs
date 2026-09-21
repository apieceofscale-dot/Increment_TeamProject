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
    //ex)
    // poolable.poolable.InitializePoolObj(() => ReturnObject(go));
    //
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



public interface IUIViewInitialize
{
    public void InitializeView();
}

public interface ICharacterDamageTarget // 피해 계산에 필요한 정보 요청 인터페이스
{
    CharacterDamageTargetData GetDamageTargetData();
}