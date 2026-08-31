using System;
using UnityEngine;
[Serializable]
public class StageData : BaseData
{
    //런타임과 똑같은 필드 생성

    public StageData(int id, string name, string description, string displayname) : base(id, name, description, displayname)
    {
        //생성자로 런타임 데이터에 위에 선언한 필드 대입
    }


    public override BaseData Clone()
    {
        //생성자 반환하기.
        return new StageData(id, codeName, description, displayName);
    }
}

[CreateAssetMenu(fileName = "StageData", menuName = "GameData/StageData")]
public class StageList : BaseList<StageData>
{

}

