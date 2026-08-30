using System;
using UnityEngine;

public class ItemController : MonoBehaviour, IPoolable
{
    [SerializeField] int itemId = 1;
    [SerializeField] int upgradeLevel;
    [SerializeField] int starForce;
    [SerializeField] string collectorTag = "Player";

    readonly ItemStatus _status = new ItemStatus();
    readonly ItemStatusProvider _statusProvider = ItemStatusProvider.Default;

    Action _returnToPool;
    bool _spawned;

    public ItemStatus Status => _status;

    public void BindSpawn(int id, int upgrade, int star)
    {
        itemId = id;
        upgradeLevel = upgrade;
        starForce = star;
    }

    public void InitializePoolObj(Action returnAction)
    {
        _returnToPool = returnAction;
    }

    public void OnSpawn()
    {
        _spawned = true;
        _statusProvider.ApplyTo(_status, ResolveData(), upgradeLevel, starForce);
    }

    public void OnDespawn()
    {
        _spawned = false;
        _status.Clear();
    }

    void OnTriggerEnter(Collider other)
    {
        TryPickUp(other.gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        TryPickUp(other.gameObject);
    }

    public void TryPickUp(GameObject collector)
    {
        if (!_spawned || _status.PickedUp || collector == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(collectorTag) && !collector.CompareTag(collectorTag))
        {
            return;
        }

        _status.MarkPickedUp();
        ItemFacade.NotifyPickedUp(new ItemPickedUpInfo
        {
            ItemId = _status.Id,
            Type = _status.Type,
            Value = _status.EffectiveValue,
            Collector = collector,
            Source = this
        });
    }

    public void ReturnToPool()
    {
        OnDespawn();
        _returnToPool?.Invoke();
    }

    ItemData ResolveData()
    {
        if (DataManager.instance != null &&
            DataManager.instance.TryGetItemData(itemId, out var data) &&
            data != null)
        {
            return data;
        }

        return new ItemData
        {
            id = itemId,
            type = ItemType.Currency,
            value = 1,
            upgradeStep = 1
        };
    }
}
