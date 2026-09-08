using System;
using UnityEngine;

public class ItemFacade : MonoBehaviour, IBootStrapper
{
    public static event Action<ItemPickedUpInfo> ItemPickedUp;

    [SerializeField] ItemController prefab;

    public void IBootStrapperInitialize(BootstrapContext context)
    {
        ItemPickedUp -= HandlePickedUp;
        ItemPickedUp += HandlePickedUp;
        context.OnStepCompleted?.Invoke();
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

    public void Despawn(ItemController item)
    {
        if (item == null)
        {
            return;
        }

        item.ReturnToPool();
    }

    public void DropFromMonster(in MonsterDiedInfo info)
    {
        if (info.Source == null)
        {
            return;
        }

        var status = info.Source.Status;
        if (status.DropItemId <= 0 || UnityEngine.Random.value > status.DropChance)
        {
            return;
        }

        Spawn(status.DropItemId, info.Position, Quaternion.identity);
    }

    void HandlePickedUp(ItemPickedUpInfo info)
    {
        if (info.Source != null)
        {
            Despawn(info.Source);
        }
    }
}
