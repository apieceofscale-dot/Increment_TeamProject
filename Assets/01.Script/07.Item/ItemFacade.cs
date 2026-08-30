using System;
using UnityEngine;

public class ItemFacade : MonoBehaviour, IBootStrapper
{
    public static event Action<ItemPickedUpInfo> ItemPickedUp;

    [SerializeField] ItemFactory itemFactory;

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

    public void DropFromMonster(in MonsterDiedInfo info)
    {
        if (itemFactory == null || info.Source == null || info.Source.Status.Data == null)
        {
            return;
        }

        var data = info.Source.Status.Data;
        if (data.dropItemId <= 0 || UnityEngine.Random.value > Mathf.Clamp01(data.dropChance))
        {
            return;
        }

        itemFactory.Spawn(data.dropItemId, info.Position, Quaternion.identity);
    }

    void HandlePickedUp(ItemPickedUpInfo info)
    {
        if (info.Source == null)
        {
            return;
        }

        if (itemFactory != null)
        {
            itemFactory.Despawn(info.Source);
        }
        else
        {
            info.Source.ReturnToPool();
        }
    }
}
