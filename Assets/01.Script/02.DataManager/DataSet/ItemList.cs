using System;
using UnityEngine;

/// <summary>
/// 아이템 SO. 전투/장착 스탯은 itemType, value, upgradeStep.
/// UI 초상화 icon, 필드 외형 worldSprite.
/// </summary>
[Serializable]
public class ItemData : BaseData
{
    public ItemType itemType = ItemType.Consumable;
    public int value = 1;
    public int upgradeStep = 1;

    public Sprite icon;
    public Sprite worldSprite;

    public override BaseData Clone()
    {
        ItemData clone = new ItemData();

        clone.id = id;
        clone.codeName = codeName;
        clone.description = description;
        clone.displayName = displayName;

        clone.itemType = itemType;
        clone.value = value;
        clone.upgradeStep = upgradeStep;

        clone.icon = icon;
        clone.worldSprite = worldSprite;

        return clone;
    }
}

[CreateAssetMenu(fileName = "ItemData", menuName = "GameData/ItemData")]
public class ItemList : BaseList<ItemData>
{
}
