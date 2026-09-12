using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class MonsterSpawner : MonoBehaviour
{
    private const float SpawnRadius = 1.5f;

    private const int AliveListCapacity = 16;

    private MonsterFacade monsterFacade;
    private StageMapParts map;
    private float spawnTimer;

    private readonly List<MonsterController> activeMonsters = new List<MonsterController>(AliveListCapacity);

    public void Initialize(MonsterFacade facade)
            => monsterFacade = facade ?? throw new ArgumentNullException(nameof(facade));

    // Call when map changed
    public void SetMap(in StageMapParts mapParts) => map = mapParts;

    public void ResetTimer() => spawnTimer = 0f;

    /// <summary>
    /// Call when stage renewals
    /// </summary>
    public void DespawnAll()
    {

    }



    public void TickSpawn(in StageDefinition definition, float deltaTime)
    {
        spawnTimer -= deltaTime;

        spawnTimer = definition.SpawnInterval;

        // MonsterFacade spawn here

        SpawnInternal(definition.MonsterId, definition, GetSpawnPosition(definition));

    }

    public MonsterController SpawnElite(in StageDefinition definition)
    {
        return SpawnInternal(definition.EliteMonsterId, definition, GetSpawnPosition(definition));
    }

    private MonsterController SpawnInternal(int monsterId, in StageDefinition definition, Vector3 position)
    {

        MonsterController monster = monsterFacade.Spawn(
            monsterId,
            position,
            Quaternion.identity,        // 2D 프로젝트라 회전은 쓰지 않습니다
            definition.Chapter);

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

        Transform point = map.MonsterSpawnPoints[UnityEngine.Random.Range(0, map.MonsterSpawnPoints.Length)];

        Vector2 offset = UnityEngine.Random.insideUnitCircle * SpawnRadius;
        return point.position + new Vector3(offset.x, offset.y, 0f);


    }

}
