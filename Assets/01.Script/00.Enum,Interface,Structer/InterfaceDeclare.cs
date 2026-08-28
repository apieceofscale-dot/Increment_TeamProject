using System;

interface IBootStrapper
{
    void IBootStrapperInitialize(); //인자는 적당하게 구조체 만들어서 선언해주세요.
}

public interface IPoolable
{
    void InitializePoolObj(Action returnAction); //인자 이게 적당해서 넣어놨는데, 꼭 콜백일 필요 없습니다.
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
