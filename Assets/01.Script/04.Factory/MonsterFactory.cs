using System;
using System.Collections.Generic;
using UnityEngine;

public class MonsterFactory : MonoBehaviour, IBootStrapper
{
    public static MonsterFactory Instance { get; private set; }

    [Serializable]
    struct PrefabEntry
    {
        public int id;
        public MonsterController prefab;
    }

    [SerializeField] PrefabEntry[] prefabEntries;
    [SerializeField] MonsterObjectPoolManager poolManager;

    readonly Dictionary<int, MonsterController> prefabById = new Dictionary<int, MonsterController>();

    public int BootOrder => (int)BootLayer.Factory;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void IBootStrapperInject(BootstrapContext context)
    {
        if (poolManager == null)
        {
            context.TryGet(out poolManager);
        }
    }

    public void IBootStrapperInitialize()
    {
        BuildPrefabMap();
        WarmUpPools();
    }

    public MonsterController Create(int monsterId, Vector3 position, Quaternion rotation, int stageIndex = 1)
    {
        if (!DataManager.instance.TryGetMonsterData(monsterId, out MonsterData data))
        {
            Debug.LogWarning($"[MonsterFactory] MonsterData not found. id={monsterId}");
            return null;
        }

        if (!prefabById.TryGetValue(monsterId, out MonsterController prefab) || prefab == null)
        {
            Debug.LogWarning($"[MonsterFactory] Prefab not found. id={monsterId}");
            return null;
        }

        MonsterController monster = GetFromPool(prefab, position, rotation);
        if (monster == null)
        {
            return null;
        }

        monster.Initialize(data, stageIndex);
        monster.OnSpawn();
        return monster;
    }

    public void Release(MonsterController monster)
    {
        if (monster == null || poolManager == null)
        {
            return;
        }

        poolManager.ReturnObject(monster);
    }

    void BuildPrefabMap()
    {
        prefabById.Clear();

        if (prefabEntries == null)
        {
            return;
        }

        foreach (PrefabEntry entry in prefabEntries)
        {
            if (entry.prefab == null)
            {
                continue;
            }

            prefabById[entry.id] = entry.prefab;
        }
    }

    void WarmUpPools()
    {
        if (poolManager == null)
        {
            Debug.LogWarning("[MonsterFactory] MonsterObjectPoolManager is missing.");
            return;
        }

        var prefabs = new List<MonsterController>(prefabById.Values);
        if (prefabs.Count > 0)
        {
            poolManager.MakeFirstPools(prefabs);
        }
    }

    MonsterController GetFromPool(MonsterController prefab, Vector3 position, Quaternion rotation)
    {
        if (poolManager == null)
        {
            MonsterController created = Instantiate(prefab, position, rotation);
            if (created is IPoolable poolable)
            {
                poolable.InitializePoolObj(() => Destroy(created.gameObject));
            }

            return created;
        }

        return poolManager.GetObject(prefab, position, rotation);
    }
}
