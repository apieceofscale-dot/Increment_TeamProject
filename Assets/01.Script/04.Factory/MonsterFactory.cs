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

    /// <summary>조립: 풀 매니저·프리팹 참조만 연결. 풀 생성은 Initialize 마지막 MakeFirstPools.</summary>
    public void IBootStrapperInject(BootstrapContext context)
    {
        if (poolManager == null)
        {
            context.TryGet(out poolManager);
        }

        if (poolManager == null)
        {
            poolManager = FindFirstObjectByType<MonsterObjectPoolManager>();
        }
    }

    public void IBootStrapperInitialize()
    {
        if (poolManager == null)
        {
            Debug.LogWarning("[MonsterFactory] MonsterObjectPoolManager is missing.");
            return;
        }

        if (defaultPrefab == null)
        {
            Debug.LogWarning("[MonsterFactory] defaultPrefab is missing.");
            return;
        }

        poolManager.MakeFirstPools(defaultPrefab);
    }

    public MonsterController Create(int monsterId, Vector3 position, Quaternion rotation, int stageIndex = 1)
    {
        if (defaultPrefab == null)
        {
            Debug.LogWarning("[MonsterFactory] defaultPrefab is missing.");
            return null;
        }

        if (poolManager == null)
        {
            Debug.LogWarning("[MonsterFactory] pool is not ready. Check MakeFirstPools / ObjectPool boot.");
            return null;
        }

        if (!DataManager.instance.TryGetMonsterData(monsterId, out MonsterData data))
        {
            Debug.LogWarning($"[MonsterFactory] MonsterData not found. id={monsterId}");
            return null;
        }

        MonsterController monster = poolManager.GetObject(defaultPrefab, position, rotation);
        if (monster == null)
        {
            return null;
        }

        // IPoolable은 풀 매니저가 붙이고, Factory는 SO 주입 + 스폰 준비만.
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
}
