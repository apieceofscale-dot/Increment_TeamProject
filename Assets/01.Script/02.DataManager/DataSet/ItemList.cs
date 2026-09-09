using System;
using UnityEngine;

[Serializable]
public class ItemData : BaseData
{

    //필드선언



    public override BaseData Clone()
    {
        ItemData clone = new ItemData();

        clone.id = this.id;
        clone.codeName = this.codeName;
        clone.description = this.description;
        clone.displayName = this.displayName;

        return clone;
    }
}

[CreateAssetMenu(fileName = "ItemData", menuName = "GameData/ItemData")]
public class ItemList: BaseList<ItemData>
{
    
}