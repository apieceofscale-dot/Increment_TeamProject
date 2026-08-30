using System.Collections;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour, IBootStrapper
{
    [SerializeField] MonsterFactory monsterFactory;
    [SerializeField] int monsterId = 1;
    [SerializeField] int spawnCount = 3;
    [SerializeField] float spacing = 1.5f;
    [SerializeField] Transform[] spawnPoints;

    bool _bootReady;

    public void IBootStrapperInitialize(BootstrapContext context)
    {
        if (monsterFactory == null)
        {
            monsterFactory = MonsterFactory.Instance;
        }

        _bootReady = true;
        context.OnStepCompleted?.Invoke();
    }

    public int SpawnWave(int stageIndex = 1)
    {
        if (!_bootReady && FindFirstObjectByType<BootStrapper>() is BootStrapper boot && !boot.IsBootCompleted)
        {
            return 0;
        }

        if (monsterFactory == null)
        {
            monsterFactory = MonsterFactory.Instance;
        }

        if (monsterFactory == null)
        {
            return 0;
        }

        var spawned = 0;
        for (var i = 0; i < spawnCount; i++)
        {
            var position = ResolvePosition(i);
            if (monsterFactory.Spawn(monsterId, position, Quaternion.identity, stageIndex) != null)
            {
                spawned++;
            }
        }

        return spawned;
    }

    public IEnumerator SpawnAfterBoot(int stageIndex = 1)
    {
        var boot = FindFirstObjectByType<BootStrapper>();
        while (boot != null && !boot.IsBootCompleted)
        {
            yield return null;
        }

        SpawnWave(stageIndex);
    }

    Vector3 ResolvePosition(int index)
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            var point = spawnPoints[index % spawnPoints.Length];
            if (point != null)
            {
                return point.position;
            }
        }

        return transform.position + Vector3.right * (index * spacing);
    }
}
