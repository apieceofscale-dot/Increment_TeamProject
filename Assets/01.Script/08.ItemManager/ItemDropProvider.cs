using System;
using System.Collections.Generic;

public class ItemDropProvider
{
    /// <summary>
    /// 테이블의 각 줄 확률을 독립적으로 굴려 드랍 결과를 buffer에 채운다
    /// "확정 드랍"은 Chance = 1f로 표현
    /// </summary>
    /// <param name="entries">드랍 테이블 리스트</param>
    /// <param name="random">난수기, 호출자가 소유하고 계속 재사용합니다</param>
    /// <param name="buffer">결과를 담을 리스트 내부에서 Clear() 후 채웁니다.</param>
    public void Resolve(IReadOnlyList<DropTableEntry> entries, Random random, List<DropResult> buffer)
    {
        if (buffer == null)
        {
            throw new ArgumentNullException(nameof(buffer));
        }

        buffer.Clear();

        if (entries == null || random == null)
        {
            return;
        }


        for (int i = 0; i < entries.Count; i++)
        {
            DropTableEntry entry = entries[i];

            if (entry.Chance <= 0f)
            {
                continue;   // 0% 줄은 안굴림
            }

            // Chance 1 이상도 안굴림
            if (entry.Chance < 1f && random.NextDouble() >= entry.Chance)
            {
                continue;
            }

            int amount = RollAmount(entry, random);
            if (amount <= 0)
            {
                continue;   // 데이터가 잘못돼 0개가 나오면 결과에 넣지 않는다
            }

            buffer.Add(new DropResult(entry.ItemId, amount));
        }
    }

    //확률 굴리기
    private int RollAmount(DropTableEntry entry, Random random)
    {
        int min = Math.Max(0, entry.MinAmount);
        int max = Math.Max(min, entry.MaxAmount);

        if (min == max)
        {
            return min;
        }

        return random.Next(min, max + 1); //min, max값 둘다 포함값임
    }
}


//드랍 판정 결과 1건
public readonly struct DropResult
{
    public readonly int ItemId;
    public readonly int Amount;

    public DropResult(int itemId, int amount)
    {
        ItemId = itemId;
        Amount = amount;
    }
}
