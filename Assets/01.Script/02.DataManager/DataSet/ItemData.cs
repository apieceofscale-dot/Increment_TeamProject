using System.Collections.Generic;
using UnityEngine;

public class ItemData : BaseData
{
    public ItemType type = ItemType.Currency;
    public int value = 1;
    public int upgradeStep = 1;

    public override BaseData Clone()
    {
        return (ItemData)MemberwiseClone();
    }
}

[CreateAssetMenu(fileName = "ItemData", menuName = "GameData/ItemData")]
public class ItemList : ScriptableObject
{
    public List<ItemData> itemList = new List<ItemData>();
}
