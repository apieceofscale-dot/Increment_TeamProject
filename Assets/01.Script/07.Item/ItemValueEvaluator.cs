/// <summary>
/// ItemStatusProvider와 동일한 공식으로 UI 미리보기용 수치를 계산한다.
/// </summary>
public static class ItemValueEvaluator
{
    static readonly ItemUpgradeProvider UpgradeProvider = ItemUpgradeProvider.Default;
    static readonly ItemEnchantProvider EnchantProvider = ItemEnchantProvider.Default;

    public static int Evaluate(ItemType type, int value, int upgradeStep, int upgradeLevel, int starForce)
    {
        int upgraded = UpgradeProvider.Evaluate(type, value, upgradeStep, upgradeLevel);
        if (type == ItemType.Weapon)
        {
            return EnchantProvider.Apply(upgraded, starForce);
        }

        return upgraded;
    }
}
