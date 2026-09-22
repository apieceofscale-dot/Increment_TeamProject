using UnityEngine;

[DefaultExecutionOrder(-10001)]
public class ItemFactory : MonoBehaviour, IBootStrapper
{
    public static ItemFactory Instance { get; private set; }

    [SerializeField] ItemController defaultPrefab;
    [SerializeField] ItemObjectPoolManager poolManager;

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
            poolManager = FindFirstObjectByType<ItemObjectPoolManager>();
        }
    }

    public void IBootStrapperInitialize()
    {
        if (poolManager == null)
        {
            Debug.LogWarning("[ItemFactory] ItemObjectPoolManager is missing.");
            return;
        }

        if (defaultPrefab == null)
        {
            Debug.LogWarning("[ItemFactory] defaultPrefab is missing.");
            return;
        }

        poolManager.MakeFirstPools(defaultPrefab);
    }

    public ItemController Create(int itemId, Vector3 position, Quaternion rotation, int stackAmount = 1, int upgradeLevel = 0, int starForce = 0)
    {
        if (defaultPrefab == null)
        {
            Debug.LogWarning("[ItemFactory] defaultPrefab is missing.");
            return null;
        }

        if (poolManager == null)
        {
            Debug.LogWarning("[ItemFactory] pool is not ready. Check MakeFirstPools / ObjectPool boot.");
            return null;
        }

        if (!DataManager.instance.TryGetItemData(itemId, out ItemData data))
        {
            Debug.LogWarning($"[ItemFactory] ItemData not found. id={itemId}");
            return null;
        }

        ItemController item = poolManager.GetObject(defaultPrefab, position, rotation);
        if (item == null)
        {
            return null;
        }

        item.Initialize(data, stackAmount);
        item.BindSpawn(itemId, upgradeLevel, starForce, stackAmount);
        item.OnSpawn();
        return item;
    }

    public void Release(ItemController item)
    {
        if (item == null || poolManager == null)
        {
            return;
        }

        poolManager.ReturnObject(item);
    }
}
