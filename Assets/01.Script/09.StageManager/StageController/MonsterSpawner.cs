using System;
using System.Collections.Generic;
using UnityEngine;

// 몬스터 팩토리가 없다는 전제로 진행합니다. 
// 인스펙터에서 몬스터 프리팹 하나를 직접 들고, 자체 Queue 풀로 생성/반납합니다

public sealed class MonsterSpawner : MonoBehaviour
{
    private const float SpawnRadius = 1.5f;
    private const int AliveListCapacity = 16;

    [Tooltip("모든 스테이지가 공용으로 쓰는 몬스터 프리팹(MonsterController 필수)")]
    [SerializeField] private MonsterController monsterPrefab;

    // private MonsterFactory monsterFactory;
    private StageMapParts map;
    private bool hasMap;
    private float spawnTimer;
    private bool warnedMissingData;



    private readonly List<MonsterController> activeMonsters = new List<MonsterController>(AliveListCapacity);

    // 비활성 대기 몬스터 풀
    private readonly Queue<MonsterController> pool = new Queue<MonsterController>(AliveListCapacity);
    // 풀 오브젝트 부모. StageManager 밑에 둘 것.(스테이지 씬 언로드때 같이 빠지는 것 방지)
    private Transform poolRoot;


    /// <summary>StageController가 부트 Initialize 단계에서 1회 호출</summary>
    public bool Initialize()
    {
        if (monsterPrefab == null)
        {
            Debug.LogError("[MonsterSpawner] monsterPrefab이 인스펙터에 연결되지 않았습니다.", this);
            return false;
        }

        if (poolRoot == null)
        {
            poolRoot = new GameObject("MonsterPool").transform;
            poolRoot.SetParent(transform, false);
        }
        return true;
    }




    // Call when map changed (by stageController)
    public void SetMap(in StageMapParts mapParts)
    {
        map = mapParts;
        hasMap = true;
    }

    public void ResetTimer() => spawnTimer = 0f;

    /// <summary>
    /// 스테이지 전환, 보스클리어시 전부회수
    /// </summary>
    public void DespawnAll()
    {
        for (int i = activeMonsters.Count - 1; i >= 0; i--)
        {
            MonsterController monster = activeMonsters[i];
            if (monster != null)
            {
                monster.ReturnToPool();
            }
        }

        activeMonsters.Clear();
        spawnTimer = 0f;
    }

    // 엘리트 도망 등 개별반납용
    public void Despawn(MonsterController monster)
    {
        if (monster == null) return;

        activeMonsters.Remove(monster);
        monster.ReturnToPool();
    }


    public void TickSpawn(in StageDefinition definition, float deltaTime)
    {
        if (!hasMap || monsterPrefab == null) Debug.LogError("맵 혹은 몬스터 프리팹이 null입니다.");

        spawnTimer -= deltaTime;
        if (spawnTimer > 0f) return;

        if (CountAlive() >= definition.MaxAliveMonster) return;

        spawnTimer = definition.SpawnInterval;

        // 보스맵은 지정된 보스 위치에서, 파밍은 스폰 포인트 중 랜덤 위치에서
        Vector3 position = definition.Type == StageType.Boss && map.HasBossSpawnPoint
            ? map.BossSpawnPosition
            : GetSpawnPosition(definition);

        SpawnInternal(definition.MonsterId, definition, position);
    }

    // 엘리트는 이번에 호출하지 않는다. 코드 복구만 해 둔다.
    public MonsterController SpawnElite(in StageDefinition definition)
    {
        if (!hasMap || !definition.HasElite) return null;

        Vector3 position = map.HasBossSpawnPoint ? map.BossSpawnPosition : GetSpawnPosition(definition);
        return SpawnInternal(definition.EliteMonsterId, definition, position);
    }



    //-------내부 --------

    // 타겟은 스폰할 때 한번 넣어주기
    private MonsterController SpawnInternal(int monsterId, in StageDefinition definition, Vector3 position)
    {

        if (monsterId <= 0) return null;

        MonsterController monster = Rent();
        monster.transform.SetPositionAndRotation(position, Quaternion.identity);

        // 데이터 주입: Factory가 하던 SO 조회 + 프리팹 결합을 임시로 여기서 수행
        if (DataManager.instance != null
            && DataManager.instance.TryGetMonsterData(monsterId, out MonsterData data))
        {
            monster.Initialize(data, definition.Chapter);
        }
        else
        {
            // 폴백: 프리팹 직렬화 스탯 사용, id만 바꿔 킬/드랍 id가 맞게
            if (!warnedMissingData)
            {
                Debug.LogWarning($"[MonsterSpawner] MonsterData 없음 → 프리팹 기본 스탯 사용. id={monsterId}", this);
                warnedMissingData = true;
            }
            monster.BindSpawn(monsterId, definition.Chapter);
        }

        // 활성화 → OnSpawn 순서: 데이터가 들어간 뒤 상태 리셋/AI 시작
        monster.gameObject.SetActive(true);
        monster.OnSpawn();

        activeMonsters.Add(monster);
        return monster;

    }

    /// <summary>풀에서 꺼내거나, 비어 있으면 새로 만든다(비활성 상태로 반환)</summary>
    private MonsterController Rent()
    {
        while (pool.Count > 0)
        {
            MonsterController pooled = pool.Dequeue();
            if (pooled != null) return pooled;   // 외부에서 파괴된 인스턴스는 건너뜀
        }

        MonsterController created = Instantiate(monsterPrefab, poolRoot);
        created.gameObject.SetActive(false);


        // 반납 콜백은 인스턴스당 1번만 등록 → 클로저 할당도 생성 시 1회뿐
        created.InitializePoolObj(() => Release(created));
        return created;

    }

    /// <summary>MonsterController.ReturnToPool()이 호출하는 반납 지점</summary>
    private void Release(MonsterController monster)
    {
        // 이중 반납 방지
        if (monster == null || !monster.gameObject.activeSelf) return;

        monster.gameObject.SetActive(false);
        monster.transform.SetParent(poolRoot, false);
        pool.Enqueue(monster);
    }




    private int CountAlive()
    {
        for (int i = activeMonsters.Count - 1; i >= 0; i--)
        {
            MonsterController monster = activeMonsters[i];
            if (monster == null || !monster.gameObject.activeInHierarchy)
                activeMonsters.RemoveAt(i);
        }

        return activeMonsters.Count;
    }


    // Spawn at random point
    private Vector3 GetSpawnPosition(in StageDefinition definition)
    {
        Transform[] points = map.MonsterSpawnPoints;
        Vector3 fallback = map.Root != null ? map.Root.position : Vector3.zero;

        if (points == null || points.Length == 0)
        {
            Debug.LogWarning($"[MonsterSpawner] 스폰 포인트가 없습니다. stageId={definition.StageId}");
            return fallback;
        }

        Transform point = points[UnityEngine.Random.Range(0, points.Length)];
        if (point == null) return fallback;

        Vector2 offset = UnityEngine.Random.insideUnitCircle * SpawnRadius;
        return point.position + new Vector3(offset.x, offset.y, 0f);
    }

}
