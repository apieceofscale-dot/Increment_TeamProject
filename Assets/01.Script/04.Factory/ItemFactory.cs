using UnityEngine;

public class ItemFactory : MonoBehaviour
{
    public static ItemFactory Instance { get; private set; }

    [SerializeField] ItemController prefab;

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

    public ItemController Spawn(int itemId, Vector3 position, Quaternion rotation, int upgradeLevel = 0, int starForce = 0)
    {
        var pool = ItemObjectPoolManager.instance;
        if (pool == null || prefab == null)
        {
            Debug.LogWarning("[ItemFactory] pool or prefab is missing.");
            return null;
        }

        var item = pool.GetObject(prefab);
        item.InitializePoolObj(() => pool.ReturnObject(item));
        item.transform.SetPositionAndRotation(position, rotation);
        item.BindSpawn(itemId, upgradeLevel, starForce);
        item.OnSpawn();
        return item;
    }

    public void Despawn(ItemController item)
    {
        if (item == null)
        {
            return;
        }

        item.ReturnToPool();
    }
}
