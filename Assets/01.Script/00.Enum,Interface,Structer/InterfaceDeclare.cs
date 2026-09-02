using System;

public interface IBootStrapper
{


    /// <summary>
    /// 폴더 번호에 해당하는 BootLayer값 반환해주세요. public int BootOrder => (int)BootLayer.초기화필요한매니저;
    /// </summary>
    int BootOrder { get; }

    /// <summary>
    /// 필요한 다른 파사드를 context에서 받아 필드에 저장만 합니다.
    /// </summary>
    void IBootStrapperInject(BootstrapContext context);

    /// <summary>
    /// 초기화
    /// </summary>
    void IBootStrapperInitialize(); //인자는 적당하게 구조체 만들어서 선언해주세요.

}

public interface IPoolable
{
    void InitializePoolObj(Action returnAction); //인자 이게 적당해서 넣어놨는데, 꼭 콜백일 필요 없습니다.
    void OnSpawn();
    void OnDespawn();
}