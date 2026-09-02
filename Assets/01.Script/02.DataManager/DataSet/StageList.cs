using System;
using UnityEngine;
[Serializable]
public class StageData : BaseData
{
    //필드선언




    public override BaseData Clone()
    {
        StageData clone = new StageData();

        //clone.id =this.id;

        return clone;
    }
}

[CreateAssetMenu(fileName = "StageData", menuName = "GameData/StageData")]
public class StageList : BaseList<StageData>
{

}

