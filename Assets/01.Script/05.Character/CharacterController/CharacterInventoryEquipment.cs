using System;

public class CharacterInventoryEquipment
{
    public Guid InstanceId { get; }
    public int ItemId { get; }
    public ItemType ItemType => ItemType.Equipment;
    public int BaseValue { get; }

    internal CharacterInventoryEquipment(Guid instanceId, int itemId, int baseValue)
    {
        InstanceId = instanceId;
        ItemId = itemId;
        BaseValue = baseValue;
    }
}
