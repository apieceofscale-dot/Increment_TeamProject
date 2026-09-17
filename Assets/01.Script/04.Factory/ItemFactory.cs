using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemFactory : MonoBehaviour, IBootStrapper
{
    public static ItemFactory Instance { get; private set; }

    [Serializable]
    struct PrefabEntry
    {
        public int id;
        public ItemController prefab;
    }

    [SerializeField] PrefabEntry[] prefabEntries;
    [SerializeField] ItemObjectPoolManager poolManager;

    readonly Dictionary<int, ItemController> prefabById = new Dictionary<int, ItemController>();

    public int BootOrder => (int)BootLayer.Factory;   

    public void IBootStrapperInject(BootstrapContext context)
    {

        Instance = this;

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

    public ItemController Create(int itemId, Vector3 position, Quaternion rotation, int stackAmount = 1, int upgradeLevel = 0, int starForce = 0)
    {
        if (!DataManager.instance.TryGetItemData(itemId, out ItemData data))
        {
            Debug.LogWarning($"[ItemFactory] ItemData not found. id={itemId}");
            return null;
        }

        if (!prefabById.TryGetValue(itemId, out ItemController prefab) || prefab == null)
        {
            Debug.LogWarning($"[ItemFactory] Prefab not found. id={itemId}");
            return null;
        }

        ItemController item = GetFromPool(prefab, position, rotation);
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
            Debug.LogWarning("[ItemFactory] ItemObjectPoolManager is missing.");
            return;
        }

        var prefabs = new List<ItemController>(prefabById.Values);
        if (prefabs.Count > 0)
        {
            poolManager.MakeFirstPools(prefabs);
        }
    }

    ItemController GetFromPool(ItemController prefab, Vector3 position, Quaternion rotation)
    {
        if (poolManager == null)
        {
            ItemController created = Instantiate(prefab, position, rotation);
            if (created is IPoolable poolable)
            {
                poolable.InitializePoolObj(() => Destroy(created.gameObject));
            }

            return created;
        }

        return poolManager.GetObject(prefab, position, rotation);
    }
}
