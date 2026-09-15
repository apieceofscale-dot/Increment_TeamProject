using System;
using UnityEngine;

public class ItemFacade : MonoBehaviour
{
    public static event Action<ItemPickedUpInfo> ItemPickedUp;

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

    public bool TryGetEquipStat(int itemId, out ItemEquipStat stat)
    {
        if (DataManager.instance != null && DataManager.instance.TryGetItemData(itemId, out ItemData data))
        {
            stat = ItemEquipStat.FromData(data);
            return true;
        }

        stat = default;
        return false;
    }

    public ItemController Spawn(int itemId, Vector3 position, Quaternion rotation, int upgradeLevel = 0, int starForce = 0)
    {
        if (ItemFactory.Instance == null)
        {
            Debug.LogWarning("[ItemFacade] ItemFactory is missing.");
            return null;
        }

        return ItemFactory.Instance.Create(itemId, position, rotation, 1, upgradeLevel, starForce);
    }

    public ItemController Spawn(int itemId, int amount, Vector3 position)
    {
        if (ItemFactory.Instance == null)
        {
            Debug.LogWarning("[ItemFacade] ItemFactory is missing.");
            return null;
        }

        return ItemFactory.Instance.Create(itemId, position, Quaternion.identity, amount);
    }

    public ItemController Spawn(ItemData data, Vector3 position, int stackAmount = 1)
    {
        if (ItemFactory.Instance == null)
        {
            Debug.LogWarning("[ItemFacade] ItemFactory is missing.");
            return null;
        }

        if (data == null)
        {
            Debug.LogWarning("[ItemFacade] item data is missing.");
            return null;
        }

        return ItemFactory.Instance.Create(data.id, position, Quaternion.identity, stackAmount);
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
