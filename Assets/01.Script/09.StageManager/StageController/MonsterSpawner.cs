using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class MonsterSpawner : MonoBehaviour
{
    private const float SpawnRadius = 1.5f;
    private const int AliveListCapacity = 16;

    private MonsterFactory monsterFactory;
    private StageMapParts map;
    private bool hasMap;
    private float spawnTimer;



    private readonly List<MonsterController> activeMonsters = new List<MonsterController>(AliveListCapacity);

    public void Initialize(MonsterFactory factory)
           => monsterFactory = factory ?? throw new ArgumentNullException(nameof(factory));






    // Call when map changed (by stageController)
    public void SetMap(in StageMapParts mapParts)
    {
        map = mapParts;
        hasMap = true;
    }

    public void ResetTimer() => spawnTimer = 0f;

    /// <summary>
    /// Call when stage renewals
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
        if (!hasMap) return;

        spawnTimer -= deltaTime;
        if (spawnTimer > 0f) return;

        if (CountAlive() >= definition.MaxAliveMonster) return;

        spawnTimer = definition.SpawnInterval;
        SpawnInternal(definition.MonsterId, definition, GetSpawnPosition(definition));
    }

    public MonsterController SpawnElite(in StageDefinition definition)
    {
        if (!hasMap) return null;
        if (!definition.HasElite) return null;

        // 보스맵 소환 포인트가 있으면 그자리에서, 없음 일반위치에서 ㄱ
        Vector3 position = map.HasBossSpawnPoint ? map.BossSpawnPosition : GetSpawnPosition(definition);

        return SpawnInternal(definition.EliteMonsterId, definition, position);


    }

    // 타겟은 스폰할 때 한번 넣어주기
    private MonsterController SpawnInternal(int monsterId, in StageDefinition definition, Vector3 position)
    {

        if (monsterId <= 0) return null;

        if (monsterFactory == null)
        {
            Debug.LogError("[MonsterSpawner] MonsterFactory가 주입되지 않았습니다.");
            return null;
        }


        // 팩토리가 풀 대여~데이터 주입까지. 
        MonsterController monster = monsterFactory.Create(
    monsterId, position, Quaternion.identity, definition.Chapter);

        if (monster == null) return null;


        activeMonsters.Add(monster);
        return monster;

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

        if (points == null || points.Length == 0)
        {
            Debug.LogWarning($"[MonsterSpawner] 스폰 포인트가 없습니다. stageId={definition.StageId}");
            return map.Root != null ? map.Root.position : Vector3.zero;
        }

        Transform point = map.MonsterSpawnPoints[UnityEngine.Random.Range(0, map.MonsterSpawnPoints.Length)];
        if (point == null) return map.Root != null ? map.Root.position : Vector3.zero;


        Vector2 offset = UnityEngine.Random.insideUnitCircle * SpawnRadius;
        return point.position + new Vector3(offset.x, offset.y, 0f);


    }

}
