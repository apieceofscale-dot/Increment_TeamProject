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
        return SpawnInternal(definition.EliteMonsterId, definition, GetSpawnPosition(definition));
    }

    private MonsterController SpawnInternal(int monsterId, in StageDefinition definition, Vector3 position)
    {

        _ = new MonsterSpawnRequest(
            monsterId, definition.Chapter, definition.StatMultiplier, definition.DropTableId, position);

        if (monsterFactory == null)
        {
            Debug.LogWarning("[MonsterSpawner] MonsterFactory is missing.");
            return null;
        }

        MonsterController monster = monsterFactory.Create(
            monsterId,
            position,
            Quaternion.identity,
            Mathf.Max(1, definition.Chapter));

        if (monster == null)
        {
            return null;
        }

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
        // Transform[] points = map.MonsterSpawnPoints;

        // // 스폰 위치 지정 필요할지말지 고민입니다
        // if (points == null || points.Length == 0)
        // {
        //     Debug.LogWarning($"[MonsterSpawner] 스폰 포인트가 없습니다. stageId={definition.StageId}");
        //     return map.Root != null ? map.Root.position : Vector3.zero;
        // }

        Transform[] points = map.MonsterSpawnPoints;
        if (points == null || points.Length == 0)
        {
            Debug.LogWarning($"[MonsterSpawner] 스폰 포인트가 없습니다. stageId={definition.StageId}");
            return map.Root != null ? map.Root.position : Vector3.zero;
        }

        Transform point = points[UnityEngine.Random.Range(0, points.Length)];
        if (point == null)
        {
            return map.Root != null ? map.Root.position : Vector3.zero;
        }

        Vector2 offset = UnityEngine.Random.insideUnitCircle * SpawnRadius;
        return point.position + new Vector3(offset.x, offset.y, 0f);


    }

}
