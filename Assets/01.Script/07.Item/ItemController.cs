using System;
using UnityEngine;

public class ItemController : MonoBehaviour, IPoolable
{
    [SerializeField] int itemId = 1;
    [SerializeField] ItemType type = ItemType.Currency;
    [SerializeField] int value = 1;
    [SerializeField] int upgradeStep = 1;
    [SerializeField] int upgradeLevel;
    [SerializeField] int starForce;
    [SerializeField] int quantity = 1;
    [SerializeField] string collectorTag = "Player";

    readonly ItemStatus _status = new ItemStatus();
    readonly ItemStatusProvider _statusProvider = ItemStatusProvider.Default;

    Action _returnToPool;
    SpriteRenderer _spriteRenderer;
    bool _spawned;
    ItemData _runtimeData;

    public ItemStatus Status => _status;

    void Awake()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void BindSpawn(int id, int upgrade, int star, int stackAmount = 1)
    {
        itemId = id;
        upgradeLevel = upgrade;
        starForce = star;
        quantity = Mathf.Max(1, stackAmount);
    }

    public void Initialize(ItemData data, int stackAmount = 1)
    {
        if (data == null)
        {
            return;
        }

        _runtimeData = data;
        itemId = data.id;
        type = data.itemType;
        value = data.value;
        upgradeStep = data.upgradeStep;
        quantity = Mathf.Max(1, stackAmount);
    }

    public void InitializePoolObj(Action returnAction)
    {
        _returnToPool = returnAction;
    }

    public void OnSpawn()
    {
        ApplyVisuals(_runtimeData);
        _spawned = true;
        _statusProvider.ApplyTo(_status, itemId, type, value, upgradeStep, upgradeLevel, starForce);
        _status.ApplyStack(quantity);
    }

    public void OnDespawn()
    {
        _spawned = false;
        _runtimeData = null;
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
        ItemPickedUpInfo pickedUp = _runtimeData != null
            ? ItemPickedUpInfo.From(_runtimeData, _status.EffectiveValue, collector, this)
            : new ItemPickedUpInfo
            {
                ItemId = (ItemId)_status.Id,
                Type = _status.Type,
                Value = _status.EffectiveValue,
                Collector = collector,
                Source = this,
            };
        ItemFacade.NotifyPickedUp(pickedUp);
    }

    public void ReturnToPool()
    {
        if (_returnToPool != null)
        {
            _returnToPool.Invoke();
            return;
        }

        if (ItemFactory.Instance != null)
        {
            ItemFactory.Instance.Release(this);
            return;
        }

        Destroy(gameObject);
    }

    void ApplyVisuals(ItemData data)
    {
        if (data == null || _spriteRenderer == null)
        {
            return;
        }

        if (data.worldSprite != null)
        {
            _spriteRenderer.sprite = data.worldSprite;
        }
    }
}
