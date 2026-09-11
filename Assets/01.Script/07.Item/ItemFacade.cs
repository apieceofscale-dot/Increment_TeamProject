using System;
using UnityEngine;

public class ItemFacade : MonoBehaviour
{
    public static event Action<ItemPickedUpInfo> ItemPickedUp;

    [SerializeField] ItemController prefab;

   // public int BootOrder => (int)BootLayer.Item;

    public void IBootStrapperInject(BootstrapContext context)
    {
    }

    public void IBootStrapperInitialize()
    {
        ItemPickedUp -= HandlePickedUp;
        ItemPickedUp += HandlePickedUp;
    }

    void OnEnable()
    {
        ItemPickedUp -= HandlePickedUp;
        ItemPickedUp += HandlePickedUp;
    }

    void OnDisable()
    {
        ItemPickedUp -= HandlePickedUp;
    }

    public static void NotifyPickedUp(in ItemPickedUpInfo info)
    {
        ItemPickedUp?.Invoke(info);
    }

    public ItemController Spawn(int itemId, Vector3 position, Quaternion rotation, int upgradeLevel = 0, int starForce = 0)
    {
        if (prefab == null)
        {
            Debug.LogWarning("[ItemFacade] prefab is missing.");
            return null;
        }

        var item = Instantiate(prefab, position, rotation);
        item.BindSpawn(itemId, upgradeLevel, starForce);
        item.OnSpawn();
        return item;
    }

    public ItemController Spawn(int itemId, int amount, Vector3 position)
    {
        if (prefab == null)
        {
            Debug.LogWarning("[ItemFacade] prefab is missing.");
            return null;
        }

        var item = Instantiate(prefab, position, Quaternion.identity);
        item.BindSpawn(itemId, 0, 0, amount);
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

    void HandlePickedUp(ItemPickedUpInfo info)
    {
        if (info.Source != null)
        {
            Despawn(info.Source);
        }
    }
}
