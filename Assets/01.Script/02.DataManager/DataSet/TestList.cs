using System;
using UnityEngine;
[Serializable]
public class TestData : BaseData
{
    public AnimationClip idleClip;


    public override BaseData Clone()
    {
        TestData clone = new TestData();

        clone.id = this.id;
        clone.codeName = this.codeName;
        clone.description = this.description;
        clone.displayName = this.displayName;
        clone.idleClip = this.idleClip;

        return clone;
    }

}

[CreateAssetMenu(fileName = "TestData", menuName = "GameData/TestData")]
public class TestList : BaseList<TestData>
{

}

