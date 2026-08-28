public sealed class ItemStatusProvider
{
    public static readonly ItemStatusProvider Default = new ItemStatusProvider();

    readonly ItemUpgradeProvider _upgradeProvider = ItemUpgradeProvider.Default;
    readonly ItemEnchantProvider _enchantProvider = ItemEnchantProvider.Default;

    public void ApplyTo(ItemStatus status, ItemData data, int upgradeLevel, int starForce)
    {
        var upgraded = _upgradeProvider.Evaluate(data, upgradeLevel);
        var effective = data != null && data.type == ItemType.Weapon
            ? _enchantProvider.Apply(upgraded, starForce)
            : upgraded;
        status.Reset(data, upgradeLevel, starForce, effective);
    }
}
