using System;
using UnityEngine;

public readonly struct BootstrapContext
{
    private readonly IBootStrapper[] targets;

    public BootstrapContext(IBootStrapper[] targets)
    {
        this.targets = targets ?? throw new ArgumentNullException(nameof(targets));
    }

    public T Get<T>() where T : class
    {
        foreach (IBootStrapper target in targets)
        {
            if (target is T match)
            {
                return match;
            }
        }

        throw new InvalidOperationException($"[BootstrapContext] {typeof(T).Name}을(를) 씬에서 찾지 못했습니다");
    }

    public bool TryGet<T>(out T match) where T : class
    {
        foreach (IBootStrapper target in targets)
        {
            if (target is T found)
            {
                match = found;
                return true;
            }
        }

        match = null;
        return false;
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
