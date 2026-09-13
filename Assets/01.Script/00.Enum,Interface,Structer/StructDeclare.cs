using System;
using UnityEngine;
using UnityEngine.Tilemaps;


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

public readonly struct StageDefinition
{
    public readonly int StageId;
    public readonly int Chapter;
    public readonly int IndexInChapter;
    public readonly string StageCodeName;
    public readonly string DisplayName;
    public readonly StageType Type;
    public readonly int MonsterId;
    public readonly int DropTableId;
    public readonly float StatMultiplier;
    public readonly int ClearKillCount;
    public readonly int MaxAliveMonster;
    public readonly float SpawnInterval;
    public readonly float TimeLimit;
    public readonly int EliteMonsterId;
    public readonly int EliteDropTableId;
    public readonly float EliteTimeLimit;
    public readonly int NextStageId;
    public readonly int FailStageId;

    public bool HasTimeLimit => TimeLimit > 0f;

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
    public readonly int Chapter;
    public readonly float StatMultiplier;
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

public readonly struct StageProgressInfo
{
    public readonly int StageId;
    public readonly int Chapter;
    public readonly int IndexInChapter;
    public readonly string DisplayName;
    public readonly StageType Type;
    public readonly StageState State;
    public readonly int KillCount;
    public readonly int ClearKillCount;
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

public readonly struct EliteProgressInfo
{
    public readonly bool IsActive;
    public readonly bool CanSummon;
    public readonly float RemainTime;
    public readonly float TimeLimit;

    public EliteProgressInfo(bool isActive, bool canSummon, float remainTime, float timeLimit)
    {
        IsActive = isActive;
        CanSummon = canSummon;
        RemainTime = remainTime;
        TimeLimit = timeLimit;
    }
}

// 맵 인스턴스 중 밖에서 쓸 것이라 예상되는 요소 
//Elements to be used outside of the map instance (maybe)
public readonly struct StageMapParts
{

    public readonly Transform Root;
    // 맵을 어떻게 찍을 지 모르겠는데 타일맵 쓸 것으로 예상해서 포함했습니다.
    //included this because I expected to use a tile map.    
    public readonly Tilemap GroundTilemap;

    // 이 맵 전용 길찾기
    //Pathfinder specific to this map    
    public readonly Navi2DPathFinder PathFinder;

    public readonly Transform PlayerStart;
    public readonly Transform[] MonsterSpawnPoints;
    public readonly Transform BossSpawnPoint; // null if it is not a boss map

    public StageMapParts(Transform root, Tilemap groundTilemap, Navi2DPathFinder pathFinder,
            Transform playerStart, Transform[] monsterSpawnPoints, Transform bossSpawnPoint)
    {
        Root = root;
        GroundTilemap = groundTilemap;
        PathFinder = pathFinder;
        PlayerStart = playerStart;
        MonsterSpawnPoints = monsterSpawnPoints;
        BossSpawnPoint = bossSpawnPoint;
    }
    public bool HasBossSpawnPoint => BossSpawnPoint != null;

    // Character position when enter stage
    public Vector3 PlayerStartPosition => PlayerStart.position;

    // Concerning about boss spawn position...
    public Vector3 BossSpawnPosition => BossSpawnPoint != null ? BossSpawnPoint.position : PlayerStart.position;


}


/// <summary>
/// Result from StageFactory.Create 
/// </summary>
public readonly struct StageBuildResult
{
    public readonly StageDefinition Definition;
    public readonly StageMapParts Map;

    public readonly bool MapChanged;

    public StageBuildResult(in StageDefinition definition, in StageMapParts map, bool mapChanged)
    {
        Definition = definition;
        Map = map;
        MapChanged = mapChanged;
    }
}

