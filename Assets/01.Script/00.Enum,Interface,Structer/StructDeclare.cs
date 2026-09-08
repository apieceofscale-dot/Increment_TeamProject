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

        throw new InvalidOperationException($"[BootstrapContext] {typeof(T).Name}��(��) ������ ã�� ���߽��ϴ�");
    }
}

public readonly struct DropTableEntry
{
    public readonly int ItemId;
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

    // 일반몹
    public readonly int MonsterId; // 스테이지별 드랍 테이블
    public readonly int DropTableId;

    // 1000^((Chapter-1) + (IndexInChapter-1)/StagePerChapter)
    public readonly float StatMultiplier;

    public readonly int ClearKillCount;
    public readonly int MaxAliveMonster;
    public readonly float SpawnInterval;
    public readonly float TimeLimit;


    // 엘리트(중간보스)
    public readonly int EliteMonsterId; // 0일시 엘리트없는던전(보스, 경험치)
    public readonly int EliteDropTableId;   // 장비위주 드롭 테이블
    public readonly float EliteTimeLimit; // 소환 후 제한시간?


    public readonly int NextStageId;
    public readonly int FailStageId;

    public bool HasTimeLimit => TimeLimit > 0f;


    /// <summary>
    /// 복잡하니 호출부에서 named argument 쓰기 권장
    /// </summary>

    public StageDefinition(
        int stageId, int chapter, int indexInChapter, string stageCodeName, string displayName, StageType type,
        int monsterId, int dropTableId, float statMultiplier,
        int clearKillCount, int maxAliveMonster, float spawnInterval, float timeLimit,
        int eliteMonsterId, int eliteDropTableId, float eliteTimeLimit,
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
        StatMultiplier = statMultiplier;
        ClearKillCount = clearKillCount;
        MaxAliveMonster = maxAliveMonster;
        SpawnInterval = spawnInterval;
        TimeLimit = timeLimit;
        EliteMonsterId = eliteMonsterId;
        EliteDropTableId = eliteDropTableId;
        EliteTimeLimit = eliteTimeLimit;
        NextStageId = nextStageId;
        FailStageId = failStageId;
    }

    public bool HasElite => EliteMonsterId > 0;

}


public readonly struct MonsterSpawnRequest
{
    public readonly int MonsterId;
    public readonly int Chapter; // 팔레트 스왑용
    public readonly float StatMultiplier;  // MonsterStageStatusProvider가 base 스탯에 곱함
    public readonly int DropTableId;
    public readonly Vector3 Position;

    public MonsterSpawnRequest(int monsterId, int chapter, float statMultiplier, int dropTableId, Vector3 position)
    {
        MonsterId = monsterId;
        Chapter = chapter;
        StatMultiplier = statMultiplier;
        DropTableId = dropTableId;
        Position = position;
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
    public readonly float RemainTime;

    public readonly bool IsClearConditionMet;




    public StageProgressInfo(
        int stageId, int chapter, int indexInChapter, string displayName, StageType type, StageState state,
        int killCount, int clearKillCount, float remainTime, bool isClearConditionMet)
    {
        StageId = stageId;
        Chapter = chapter;
        IndexInChapter = indexInChapter;
        DisplayName = displayName;
        Type = type;
        State = state;
        KillCount = killCount;
        ClearKillCount = clearKillCount;
        RemainTime = remainTime;
        IsClearConditionMet = isClearConditionMet;
    }
}

// 엘리트 진행상황 분리 구독용
public readonly struct EliteProgressInfo
{
    public readonly bool IsActive;
    public readonly bool CanSummon; // 소환 버튼 활성 조건 (스테이지가 엘리트를 가지고, 지금 없고, 전투 중)
    public readonly float RemainTime; // IsActive일 때만 의미 있음
    public readonly float TimeLimit;// 게이지 비율 계산용

    public EliteProgressInfo(bool isActive, bool canSummon, float remainTime, float timeLimit)
    {
        IsActive = isActive;
        CanSummon = canSummon;
        RemainTime = remainTime;
        TimeLimit = timeLimit;
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
