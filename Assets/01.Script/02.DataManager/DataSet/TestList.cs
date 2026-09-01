using System;
using UnityEngine;
[Serializable]
public class TestData : BaseData
{
    

  
    public override BaseData Clone()
    {
        //생성자 반환하기.
        return new TestData();
    }

}

[CreateAssetMenu(fileName = "TestData", menuName = "GameData/TestData")]
public class TestList : BaseList<TestData>
{

}

