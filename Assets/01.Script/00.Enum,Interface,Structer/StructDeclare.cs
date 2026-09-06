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

// id,codeName,description,displayName
// 9000,St1,something,Stage 1
// 9001,St2,nothing,Stage 2
// 9002,St3,anyting,Stage 3


public readonly struct StageDefinition
{
    public readonly int StageId;
    public readonly int Chapter; // 크게 3챕터가 있고 그 각각에 3개의 하위 스테이지가 있는 기획으로 해석했습니다.
    public readonly int IndexInChapter;
    public readonly string StageCodeName;
    public readonly string DisplayName;
    public readonly StageType Type;

    public readonly int MonsterId; // 스테이지별 드랍 테이블
    public readonly int DropTableId;

    public readonly int MidBossMonsterId;
    //public readonly int MidBossDropTableId;   // 분리할지 고민
    public readonly float MidBossTimeLimit;

    public readonly float StatMultiplier;
    public readonly int ClearKillCount;
    public readonly int MaxAliveMonster;
    public readonly float SpawnInterval;

    public readonly float TimeLimit;

    public readonly int NextStageId;
    public readonly int FailStageId;

    public bool HasTimeLimit => TimeLimit > 0f;


    public StageDefinition(int stageId, int chapter, int indexInChapter, string stageCodeName, string displayName, StageType type,
        int monsterId, int dropTableId, int midBossMonsterId, float midBossTimeLimit, float statMultiplier, int clearKillCount,
        int maxAliveMonster, float spawnInterval, float timeLimit,
        int nextStageId, int failStageId)
    {
        StageId = stageId;
        Chapter = chapter;
        IndexInChapter = indexInChapter;
        StageCodeName = stageCodeName;
        DisplayName = displayName;
        Type = type;
        MonsterId = monsterId;
        DropTableId = dropTableId;
        MidBossMonsterId = midBossMonsterId;
        MidBossTimeLimit = midBossTimeLimit;
        StatMultiplier = statMultiplier;
        ClearKillCount = clearKillCount;
        MaxAliveMonster = maxAliveMonster;
        SpawnInterval = spawnInterval;
        TimeLimit = timeLimit;
        NextStageId = nextStageId;
        FailStageId = failStageId;
    }


}

// ui,세이브용 정보 전달용
public readonly struct StageProgressInfo
{
    public readonly int StageId;
    public readonly int Chapter;
    public readonly int IndexInChapter;
    public readonly string DisplayName;
    public readonly StageType Type;
    public readonly StageState State;

    public readonly int KillCount;
    public readonly int ClearKillCount; // 어떤 방식으로 다음 스테이지 넘길 지 반영

    public readonly bool HasTimeLimit;
    public readonly float RemainTime;

    public readonly int NextStageId;




    public StageProgressInfo(int stageId, int chapter, int indexInChapter, string displayName,
        StageType type, StageState state,
        int killCount, int clearKillCount,
        bool hasTimeLimit, float remainTime, int nextStageId)
    {
        StageId = stageId;
        Chapter = chapter;
        IndexInChapter = indexInChapter;
        DisplayName = displayName;
        Type = type;
        State = state;
        KillCount = killCount;
        ClearKillCount = clearKillCount;
        HasTimeLimit = hasTimeLimit;
        RemainTime = remainTime;
        NextStageId = nextStageId;
    }
}

public readonly struct MonsterSpawnRequest
{

}


