public struct ItemEquipStat
{
    public int ItemId;
    public ItemType Type;
    public int BaseValue;
    public int UpgradeStep;

    public static ItemEquipStat FromData(ItemData data)
    {
        return new ItemEquipStat
        {
            ItemId = data.id,
            Type = data.itemType,
            BaseValue = data.value,
            UpgradeStep = data.upgradeStep
        };
    }
}
