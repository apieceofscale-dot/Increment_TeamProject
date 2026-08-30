using System;
using UnityEngine;

public readonly struct BootstrapContext
{
    // 해당 매니저 초기화 완료 후 호출해야 하는 콜백
    public readonly Action OnStepCompleted;

    public BootstrapContext(Action onStepCompleted)
    {
        OnStepCompleted = onStepCompleted;
    }
}

public struct MonsterDiedInfo
{
    public int MonsterId;
    public Vector3 Position;
    public MonsterController Source;
}

public struct ItemPickedUpInfo
{
    public int ItemId;
    public ItemType Type;
    public int Value;
    public GameObject Collector;
    public ItemController Source;
}
