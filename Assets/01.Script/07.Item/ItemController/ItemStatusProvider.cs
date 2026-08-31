public sealed class ItemStatusProvider
{
    public static readonly ItemStatusProvider Default = new ItemStatusProvider();

    readonly ItemUpgradeProvider _upgradeProvider = ItemUpgradeProvider.Default;
    readonly ItemEnchantProvider _enchantProvider = ItemEnchantProvider.Default;

    public void ApplyTo(ItemStatus status, int id, ItemType type, int value, int upgradeStep, int upgradeLevel, int starForce)
    {
        var upgraded = _upgradeProvider.Evaluate(type, value, upgradeStep, upgradeLevel);
        var effective = type == ItemType.Weapon
            ? _enchantProvider.Apply(upgraded, starForce)
            : upgraded;
        status.Reset(id, type, value, upgradeStep, upgradeLevel, starForce, effective);
    }
}
