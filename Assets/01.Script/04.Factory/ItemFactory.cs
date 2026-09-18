using System.Collections.Generic;
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

    public ItemController Create(int itemId, Vector3 position, Quaternion rotation, int stackAmount = 1, int upgradeLevel = 0, int starForce = 0)
    {
        if (defaultPrefab == null)
        {
            Debug.LogWarning("[ItemFactory] defaultPrefab is missing.");
            return null;
        }

        if (!DataManager.instance.TryGetItemData(itemId, out ItemData data))
        {
            Debug.LogWarning($"[ItemFactory] ItemData not found. id={itemId}");
            return null;
        }

        ItemController item = GetFromPool(position, rotation);
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

    void WarmUpPool()
    {
        if (poolManager == null)
        {
            Debug.LogWarning("[ItemFactory] ItemObjectPoolManager is missing.");
            return;
        }

        if (defaultPrefab != null)
        {
            poolManager.MakeFirstPools(new List<ItemController> { defaultPrefab });
        }
    }

    ItemController GetFromPool(Vector3 position, Quaternion rotation)
    {
        if (poolManager == null)
        {
            ItemController created = Instantiate(defaultPrefab, position, rotation);
            if (created is IPoolable poolable)
            {
                poolable.InitializePoolObj(() => Destroy(created.gameObject));
            }

            return created;
        }

        return poolManager.GetObject(defaultPrefab, position, rotation);
    }
}
