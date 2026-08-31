using System;
using UnityEngine;
[Serializable]
public class TestData : BaseData
{
    //런타임과 똑같은 필드 생성

    public int test;

    public TestData() { }   

    public TestData(int test, int id, string name, string description, string displayname) : base(id, name, description, displayname)
    {

        this.test = test;
        
    }


    public override BaseData Clone()
    {
        //생성자 반환하기.
        return new TestData(test,id,codeName,description,displayName);
    }

}

[CreateAssetMenu(fileName = "TestData", menuName = "GameData/TestData")]
public class TestList : BaseList<TestData>
{

}

