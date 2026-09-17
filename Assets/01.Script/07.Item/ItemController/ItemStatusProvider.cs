public sealed class ItemStatusProvider
{
    public static readonly ItemStatusProvider Default = new ItemStatusProvider();

    readonly ItemUpgradeProvider _upgradeProvider = ItemUpgradeProvider.Default;
    readonly ItemEnchantProvider _enchantProvider = ItemEnchantProvider.Default;

    public void ApplyTo(ItemStatus status, int id, ItemType type, int value, int upgradeStep, int upgradeLevel, int starForce)
    {
        int effective = ItemValueEvaluator.Evaluate(type, value, upgradeStep, upgradeLevel, starForce);
        status.Reset(id, type, value, upgradeStep, upgradeLevel, starForce, effective);
    }
}
