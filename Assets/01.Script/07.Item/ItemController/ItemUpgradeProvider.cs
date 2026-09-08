using UnityEngine;

public sealed class ItemUpgradeProvider
{
    public static readonly ItemUpgradeProvider Default = new ItemUpgradeProvider();

    public int Evaluate(ItemType type, int value, int upgradeStep, int upgradeLevel)
    {
        var level = Mathf.Max(0, upgradeLevel);
        var step = Mathf.Max(1, upgradeStep);
        return Mathf.Max(0, Mathf.RoundToInt(value + level * step * GetTypeExponent(type)));
    }

    public float GetTypeExponent(ItemType type)
    {
        switch (type)
        {
            case ItemType.Equipment:
                return 1.25f;
            case ItemType.Weapon:
                return 1.5f;
            default:
                return 1f;
        }
    }
}
