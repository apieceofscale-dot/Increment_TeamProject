using System;
using UnityEngine;
[Serializable]
public class MonsterData : BaseData
{
    //필드선언
    public override BaseData Clone()
    {
        MonsterData clone = new MonsterData();

        //clone.id =this.id;

        return clone;
    }
}

[CreateAssetMenu(fileName = "MonsterData", menuName = "GameData/MonsterData")]
public class MonsterList: BaseList<MonsterData>
{

}