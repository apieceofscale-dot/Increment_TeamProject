using System.Collections.Generic;




// 몬스터 - 아이템 간의 관계를 임시로 지정합니다. 일단 MonsterId값을 그대로 사용 하드코딩
public sealed class TempDropTableSource : IDropTableSource
{

    private readonly Dictionary<int, DropTableEntry[]> tables;

    public TempDropTableSource()
    {
        tables = new Dictionary<int, DropTableEntry[]>
        {
            // 슬라임(2000) — hp포션 확정~최대2개 + mp포션 저확률
            [2000] = new[]
            {
                new DropTableEntry(itemId: 10000, chance: 1.00f, minAmount: 1, maxAmount: 2),
                new DropTableEntry(itemId: 10001, chance: 0.15f, minAmount: 1, maxAmount: 1),
            },

            // 고블린(2001) — hp포션 + mp포션 + 10010번쯤 장비 드랍 추가
            [2001] = new[]
            {
                new DropTableEntry(itemId: 10000, chance: 1.00f, minAmount: 1, maxAmount: 2),
                new DropTableEntry(itemId: 10001, chance: 0.20f, minAmount: 1, maxAmount: 1),
                new DropTableEntry(itemId: 10010, chance: 0.05f, minAmount: 1, maxAmount: 1),
            },

            // 오크(2002) — 다중 드랍 확인용 확률
            [2002] = new[]
            {
                new DropTableEntry(itemId: 10000, chance: 1.00f, minAmount: 1, maxAmount: 2),
                new DropTableEntry(itemId: 10001, chance: 0.80f, minAmount: 1, maxAmount: 1),
                new DropTableEntry(itemId: 10010, chance: 0.50f, minAmount: 1, maxAmount: 1),
            },
        };
    }

    public bool TryGetEntries(int dropTableId, out IReadOnlyList<DropTableEntry> entries)
    {
        if (tables.TryGetValue(dropTableId, out DropTableEntry[] found))
        {
            entries = found;
            return true;
        }

        entries = null;
        return false;
    }


}
