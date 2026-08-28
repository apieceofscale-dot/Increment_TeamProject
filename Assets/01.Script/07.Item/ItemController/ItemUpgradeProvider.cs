using UnityEngine;

public sealed class ItemUpgradeProvider
{
    public static readonly ItemUpgradeProvider Default = new ItemUpgradeProvider();

    public int Evaluate(ItemData data, int upgradeLevel)
    {
        if (data == null)
        {
            return 0;
        }

        var level = Mathf.Max(0, upgradeLevel);
        var step = Mathf.Max(1, data.upgradeStep);
        return Mathf.Max(0, Mathf.RoundToInt(data.value + level * step * GetTypeExponent(data.type)));
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
