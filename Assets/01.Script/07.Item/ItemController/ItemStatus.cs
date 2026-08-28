public sealed class ItemStatus
{
    public int Id { get; private set; }
    public ItemType Type { get; private set; }
    public ItemData Data { get; private set; }
    public ItemUpgrade Upgrade { get; } = new ItemUpgrade();
    public ItemEnchant Enchant { get; } = new ItemEnchant();
    public int EffectiveValue { get; private set; }
    public bool PickedUp { get; private set; }

    public void Reset(ItemData data, int upgradeLevel, int starForce, int effectiveValue)
    {
        Data = data;
        Id = data != null ? data.id : 0;
        Type = data != null ? data.type : ItemType.None;
        EffectiveValue = effectiveValue;
        PickedUp = false;
        Upgrade.Set(upgradeLevel, UnityEngine.Mathf.Max(0, effectiveValue - (data != null ? data.value : 0)));
        Enchant.Set(starForce);
    }

    public void MarkPickedUp()
    {
        PickedUp = true;
    }

    public void Clear()
    {
        PickedUp = false;
        EffectiveValue = 0;
        Data = null;
        Upgrade.Clear();
        Enchant.Clear();
    }
}
