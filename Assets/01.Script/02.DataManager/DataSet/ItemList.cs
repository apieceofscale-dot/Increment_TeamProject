using System;

using UnityEngine;



/// <summary>
/// 아이템 SO. CSV→ExcelImporter가 id/codeName(ItemId enum)·필드를 채움.
/// DataRepositary.Load 시 <see cref="Clone"/>으로 행마다 복제본을 들고 있음 — 스탯·이미지는 여기만 정의.
/// </summary>

[Serializable]

public class ItemData : BaseData

{

    public ItemType itemType = ItemType.Consumable;

    public int value = 1;

    public int upgradeStep = 1;

    public CharacterArmorPart armorPart;

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

        clone.armorPart = armorPart;

        clone.icon = icon;

        clone.worldSprite = worldSprite;



        return clone;

    }

}



[CreateAssetMenu(fileName = "ItemData", menuName = "GameData/ItemData")]

public class ItemList : BaseList<ItemData>

{

}


