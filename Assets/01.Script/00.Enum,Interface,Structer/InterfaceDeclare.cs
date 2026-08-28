using System;

interface IBootStrapper
{
    /// 각 구현체는 초기화가 끝나는 시점에 context.OnStepCompleted 호출할 것
    void IBootStrapperInitialize(BootstrapContext context); //인자는 적당하게 구조체 만들어서 선언해주세요.
}

public interface IPoolable 
{
    void InitializePoolObj(Action returnAction); //인자 이게 적당해서 넣어놨는데, 꼭 콜백일 필요 없습니다.
    void OnSpawn();
    void OnDespawn();
}