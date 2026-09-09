using System;
using UnityEngine;
[Serializable]
public class StageData : BaseData
{
    //필드선언




    public override BaseData Clone()
    {
        StageData clone = new StageData();

        clone.id = this.id;
        clone.codeName = this.codeName;
        clone.description = this.description;
        clone.displayName = this.displayName;

        return clone;
    }
}

[CreateAssetMenu(fileName = "StageData", menuName = "GameData/StageData")]
public class StageList : BaseList<StageData>
{

}

