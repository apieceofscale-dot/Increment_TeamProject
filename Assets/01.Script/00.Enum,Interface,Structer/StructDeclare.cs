using System;
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

        throw new InvalidOperationException($"[BootstrapContext] {typeof(T).Name}��(��) ������ ã�� ���߽��ϴ�");
    }
}

public readonly struct DropTableEntry
{
    /// <summary>드랍될 아이템 ID,ItemId enum 확정 후 타입 교체</summary>
    public readonly int ItemId;

    /// <summary>드랍 확률. 0f ~ 1f. 1f면 확정</summary>
    public readonly float Chance;
    public readonly int MinAmount;
    public readonly int MaxAmount;

    public DropTableEntry(int itemId, float chance, int minAmount, int maxAmount)
    {
        ItemId = itemId;
        Chance = chance;
        MinAmount = minAmount;
        MaxAmount = maxAmount;
    }
}
