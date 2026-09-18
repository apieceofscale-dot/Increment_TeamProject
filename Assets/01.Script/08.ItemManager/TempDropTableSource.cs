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
                new DropTableEntry(ItemId.HpPotion, 1.00f, 1, 2),
                new DropTableEntry(ItemId.ManaPotion, 0.15f, 1, 1),
            },

            // 고블린(2001) — hp포션 + mp포션 + 10010번쯤 장비 드랍 추가
            [2001] = new[]
            {
                new DropTableEntry(ItemId.HpPotion, 1.00f, 1, 2),
                new DropTableEntry(ItemId.ManaPotion, 0.20f, 1, 1),
                new DropTableEntry(ItemId.Hat, 0.05f, 1, 1),
            },

            // 오크(2002) — 다중 드랍 확인용 확률
            [2002] = new[]
            {
                new DropTableEntry(ItemId.HpPotion, 1.00f, 1, 2),
                new DropTableEntry(ItemId.ManaPotion, 0.80f, 1, 1),
                new DropTableEntry(ItemId.Hat, 0.50f, 1, 1),
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
