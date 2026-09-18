using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-10001)]
public class MonsterFactory : MonoBehaviour, IBootStrapper
{
    public static MonsterFactory Instance { get; private set; }

    [SerializeField] MonsterController defaultPrefab;
    [SerializeField] MonsterObjectPoolManager poolManager;

    public int BootOrder => (int)BootLayer.Factory;

    void Awake()
    {
        Instance = this;
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
        WarmUpPool();
    }

    public MonsterController Create(int monsterId, Vector3 position, Quaternion rotation, int stageIndex = 1)
    {
        if (defaultPrefab == null)
        {
            Debug.LogWarning("[MonsterFactory] defaultPrefab is missing.");
            return null;
        }

        if (!DataManager.instance.TryGetMonsterData(monsterId, out MonsterData data))
        {
            Debug.LogWarning($"[MonsterFactory] MonsterData not found. id={monsterId}");
            return null;
        }

        MonsterController monster = GetFromPool(position, rotation);
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

    void WarmUpPool()
    {
        if (poolManager == null)
        {
            Debug.LogWarning("[MonsterFactory] MonsterObjectPoolManager is missing.");
            return;
        }

        if (defaultPrefab != null)
        {
            poolManager.MakeFirstPools(new List<MonsterController> { defaultPrefab });
        }
    }

    MonsterController GetFromPool(Vector3 position, Quaternion rotation)
    {
        if (poolManager == null)
        {
            MonsterController created = Instantiate(defaultPrefab, position, rotation);
            if (created is IPoolable poolable)
            {
                poolable.InitializePoolObj(() => Destroy(created.gameObject));
            }

            return created;
        }

        return poolManager.GetObject(defaultPrefab, position, rotation);
    }
}
