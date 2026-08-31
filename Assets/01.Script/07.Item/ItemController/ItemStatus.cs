using UnityEngine;

public sealed class ItemStatus
{
    public int Id { get; private set; }
    public ItemType Type { get; private set; }
    public int BaseValue { get; private set; }
    public int UpgradeStep { get; private set; }
    public ItemUpgrade Upgrade { get; } = new ItemUpgrade();
    public ItemEnchant Enchant { get; } = new ItemEnchant();
    public int EffectiveValue { get; private set; }
    public bool PickedUp { get; private set; }

    public void Reset(int id, ItemType type, int baseValue, int upgradeStep, int upgradeLevel, int starForce, int effectiveValue)
    {
        Id = id;
        Type = type;
        BaseValue = baseValue;
        UpgradeStep = Mathf.Max(1, upgradeStep);
        EffectiveValue = effectiveValue;
        PickedUp = false;
        Upgrade.Set(upgradeLevel, Mathf.Max(0, effectiveValue - baseValue));
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
        BaseValue = 0;
        Upgrade.Clear();
        Enchant.Clear();
    }
}
