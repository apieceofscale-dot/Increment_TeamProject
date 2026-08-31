using System;

public interface IBootStrapper
{
    /// <summary>
    /// 폴더 번호에 해당하는 BootLayer를 반환해주세요. public int BootOrder => (int)BootLayer.초기화필요한매니저;
    /// </summary>
    int BootOrder { get; }
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
