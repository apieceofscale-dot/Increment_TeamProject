using UnityEngine;

public class MonsterFactory : MonoBehaviour
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

    public MonsterController Spawn(int monsterId, Vector3 position, Quaternion rotation, int stageIndex = 0)
    {
        var pool = MonsterObjectPoolManager.instance;
        if (pool == null || prefab == null)
        {
            Debug.LogWarning("[MonsterFactory] pool or prefab is missing.");
            return null;
        }

        var monster = pool.GetObject(prefab);
        if (!monster.gameObject.TryGetComponent<IPoolable>(out _))
        {
            return monster;
        }

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
}
