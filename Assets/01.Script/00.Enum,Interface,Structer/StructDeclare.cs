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

        throw new InvalidOperationException($"[BootstrapContext] Can't found {typeof(T).Name}. Check its IBootStrapper, or BootLayer order.");
    }

    // 아직 구현 안 된 시스템 대기용, 완성되면 Get만
    public bool TryGet<T>(out T result) where T : class
    {
        foreach (IBootStrapper target in targets)
        {
            if (target is T match)
            {
                result = match;
                return true;
            }
        }

        result = null;
        return false;
    }

}

/// <summary>드랍 테이블 한 줄. 아이템 id는 Generated <see cref="ItemId"/> (SO id와 동일).</summary>
public readonly struct DropTableEntry
{
    public readonly ItemId ItemId;
    public readonly float Chance;
    public readonly int MinAmount;
    public readonly int MaxAmount;

    public DropTableEntry(ItemId itemId, float chance, int minAmount, int maxAmount)
    {
        ItemId = itemId;
        Chance = chance;
        MinAmount = minAmount;
        MaxAmount = maxAmount;
    }
}

/// <summary>몬스터 사망 이벤트. 스탯·드랍 풀은 <see cref="MonsterData"/>(Clone SO) 기준.</summary>
public struct MonsterDiedInfo
{
    public MonsterId MonsterId;
    public int DropTableId;
    public long ExpReward;
    public Vector3 Position;
    public MonsterController Source;

    /// <summary>DataManager에 로드된 MonsterData(복제본)에서 id·dropTableId만 전달. 드랍 판정은 ItemDropManager.</summary>
    public static MonsterDiedInfo From(MonsterData data, Vector3 position, MonsterController source, long expReward = 0)
    {
        if (data == null)
        {
            return default;
        }

        return new MonsterDiedInfo
        {
            MonsterId = (MonsterId)data.id,
            DropTableId = data.dropTableId > 0 ? data.dropTableId : data.id,
            ExpReward = expReward,
            Position = position,
            Source = source,
        };
    }
}

/// <summary>필드 줍기 이벤트. 타입·기본 스탯은 SO <see cref="ItemData"/>에서, Value는 런타임 적용값.</summary>
public struct ItemPickedUpInfo
{
    public ItemId ItemId;
    public ItemType Type;
    public int Value;
    public GameObject Collector;
    public ItemController Source;

    /// <summary>Initialize에 넣은 ItemData(Clone 경로) 기준. 별도 스탯 테이블 없음.</summary>
    public static ItemPickedUpInfo From(ItemData data, int effectiveValue, GameObject collector, ItemController source)
    {
        if (data == null)
        {
            return default;
        }

        return new ItemPickedUpInfo
        {
            ItemId = (ItemId)data.id,
            Type = data.itemType,
            Value = effectiveValue,
            Collector = collector,
            Source = source,
        };
    }
}

/// <summary>강화 UI 미리보기. ItemId·ItemType은 SO/Generated enum 기준.</summary>
public struct ItemUpgradePreview
{
    public ItemId ItemId;
    public int CurrentUpgradeLevel;
    public int NextUpgradeLevel;
    public int CurrentEffectiveValue;
    public int NextEffectiveValue;
    public long UpgradeCost;
}

public readonly struct StageDefinition
{
    public readonly int StageId;
    public readonly int Chapter;
    public readonly int IndexInChapter;
    public readonly string StageCodeName;
    public readonly string DisplayName;
    public readonly string SceneName;
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

    public bool HasElite => EliteMonsterId > 0;

    public StageDefinition(
        int stageId, int chapter, int indexInChapter, string stageCodeName, string displayName, string sceneName, StageType type,
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
        SceneName = sceneName;
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

public readonly struct StageChangedInfo
{
    public readonly int StageId;
    public readonly int Chapter;
    public readonly string MapName; // View.SetName
    public readonly int NowStageNum; // 챕터 안에서의 현재 번호
    public readonly int TotalStageNum; // 이 챕터의 전체 스테이지 수
    public readonly StageType Type;
    public readonly int ChallengeStageNum; // 도전(보스) 스테이지 번호. 없으면 0
    public readonly bool CanChallenge;

    public bool IsValid => StageId > 0; //스테이지 진입 전(기본값)인지


    public StageChangedInfo(
        int stageId, int chapter, string mapName,
        int nowStageNum, int totalStageNum, StageType type,
        int challengeStageNum, bool canChallenge)
    {
        StageId = stageId;
        Chapter = chapter;
        MapName = mapName;
        NowStageNum = nowStageNum;
        TotalStageNum = totalStageNum;
        Type = type;
        ChallengeStageNum = challengeStageNum;
        CanChallenge = canChallenge;
    }
}

public readonly struct SkillSlotInfo //UI용 복사 데이터
{
    public int SlotIndex { get; }
    public bool IsEquipped { get; }
    public Sprite Sprite { get; }
    public string SkillName { get; }
    public SkillSlotInfo(int slotIdnex, bool equipped, Sprite sprite, string name)
    {
        SlotIndex = slotIdnex;
        IsEquipped = equipped;
        Sprite = sprite;
        SkillName = name ?? string.Empty;
    }
}

public readonly struct SkillCooldownInfo // 스킬 UI용 남은 시간 및 시전 시 확정된 전체 시간
{
    public int SlotIndex { get; }
    public float TimeLeft { get; }
    public float TotalTime { get; }
    public SkillCooldownInfo(int slotIndex, float timeLeft, float totalTime)
    {
        SlotIndex = slotIndex;
        TimeLeft = timeLeft;
        TotalTime = totalTime;
    }
}

public readonly struct CharacterStatUpgradeInfo
{
    public int StatPoints { get; }
    public int SpecialStatPoints { get; }
    public int CharacterRank { get; }
    public int MainStatCount { get; }
    public int DefenceCount { get; }
    public int MaxHpCount { get; }

    internal CharacterStatUpgradeInfo(int points, int special, int rank, int main, int defence, int hp)
    {
        StatPoints = points;
        SpecialStatPoints = special;
        CharacterRank = rank;
        MainStatCount = main;
        DefenceCount = defence;
        MaxHpCount = hp;
    }
}

public readonly struct CharacterDamageTargetData
{
    public CharacterDamageTargetKind Kind { get; }
    public double? Defence { get; }
    public double? DodgeRating { get; }
    public CharacterDamageTargetData(CharacterDamageTargetKind kind = CharacterDamageTargetKind.Unknown,
        double? defence = null, double? dodgeRating = null)
    {
        Kind = kind;
        Defence = defence;
        DodgeRating = dodgeRating;
    }
}

public readonly struct CharacterDamageAttackData
{
    public double Attack { get; }
    public double MainStat { get; }
    public double DamageByMainStat { get; }
    public double CriticalRate { get; }
    public double CriticalDamage { get; }
    public double FinalDamage { get; }
    public double HitRating { get; }
    public double ArmorPenetration { get; }
    public double DamageOnNormal { get; }
    public double DamageOnBoss { get; }

    public CharacterDamageAttackData(double attack, double mainStat, double damageByMainStat, double criticalRate, double criticalDamage, double finalDamage, double hitRating = 0, double armorPenetration = 0, double damageOnNormal = 0, double damageOnBoss = 0)
    {
        Attack = attack;
        MainStat = mainStat;
        DamageByMainStat = damageByMainStat;
        CriticalRate = criticalRate;
        CriticalDamage = criticalDamage;
        FinalDamage = finalDamage;
        HitRating = hitRating;
        ArmorPenetration = armorPenetration;
        DamageOnNormal = damageOnNormal;
        DamageOnBoss = damageOnBoss;
    }
}

public readonly struct CharacterDamageResult
{
    public int Damage { get; }
    public bool IsCritical { get; }
    public bool IsMiss { get; }
    public CharacterDamageResult(int damage, bool isCritical, bool isMiss)
    {
        Damage = damage;
        IsCritical = isCritical;
        IsMiss = isMiss;
    }
}

public readonly struct CharacterStatUpgradeOption
{
    public CharacterStatUpgradeType Type { get; }
    public int Cost { get; }
    public int IncreaseAmount { get; }
    public int CurrentCount { get; }
    public int MaxCount { get; }
    public bool CanUpgrade { get; }

    public CharacterStatUpgradeOption(CharacterStatUpgradeType type, int cost, int increaseAmount, int currentCount, int maxCount, bool canUpgrade)
    {
        Type = type;
        Cost = cost;
        IncreaseAmount = increaseAmount;
        CurrentCount = currentCount;
        MaxCount = maxCount;
        CanUpgrade = canUpgrade;
    }
}