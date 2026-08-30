using UnityEngine;

public class MonsterFactory : MonoBehaviour, IBootStrapper
{
    public static MonsterFactory Instance { get; private set; }

    [SerializeField] MonsterController prefab;
    [SerializeField] int defaultStageIndex = 1;

    void Awake()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void IBootStrapperInitialize(BootstrapContext context)
    {
        Instance = this;
        context.OnStepCompleted?.Invoke();
    }

    public MonsterController Spawn(int monsterId, Vector3 position, Quaternion rotation, int stageIndex = 0)
    {
        if (!IsBootReady())
        {
            Debug.LogWarning("[MonsterFactory] BootStrapper is not completed yet.");
            return null;
        }

        var pool = MonsterObjectPoolManager.instance;
        if (pool == null || prefab == null)
        {
            Debug.LogWarning("[MonsterFactory] pool or prefab is missing.");
            return null;
        }

        var monster = pool.GetObject(prefab);
        monster.InitializePoolObj(() => pool.ReturnObject(monster));
        monster.transform.SetPositionAndRotation(position, rotation);
        monster.BindSpawn(monsterId, stageIndex > 0 ? stageIndex : defaultStageIndex);
        monster.OnSpawn();
        return monster;
    }

    public void Despawn(MonsterController monster)
    {
        if (monster == null)
        {
            return;
        }

        monster.ReturnToPool();
    }

    static bool IsBootReady()
    {
        var boot = FindFirstObjectByType<BootStrapper>();
        return boot == null || boot.IsBootCompleted;
    }
}
