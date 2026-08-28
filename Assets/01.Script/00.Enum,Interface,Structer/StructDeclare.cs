using System;

public readonly struct BootstrapContext
{

    // 해당 매니저 초기화 완료 후 호출해야 하는 콜백
    public readonly Action OnStepCompleted;

    public BootstrapContext(Action onStepCompleted)
    {
        OnStepCompleted = onStepCompleted;
    }

}