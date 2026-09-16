public struct ItemEquipStat
{
    public int ItemId;
    public ItemType Type;
    public int BaseValue;
    public int UpgradeStep;
    public CharacterArmorPart ArmorPart;

    public static ItemEquipStat FromData(ItemData data)
    {
        ItemArmorPartTable.TryGetPart(data.id, out CharacterArmorPart part);

        return new ItemEquipStat
        {
            ItemId = data.id,
            Type = data.itemType,
            BaseValue = data.value,
            UpgradeStep = data.upgradeStep,
            ArmorPart = part
        };
    }
}
