using System;
using UnityEngine;
[Serializable]
public class TestData : BaseData
{
    public RuntimeAnimatorController animatorController;
    public AnimationClip idleClip;
    public AnimationClip runClip;
    public AnimationClip attackClip;
    public AnimationClip hitClip;
    public AnimationClip deadClip;

    public AudioClip idleAudioClip;
    public AudioClip runAudioClip;
    public AudioClip attackAudioClip;
    public AudioClip hitAudioClip;
    public AudioClip deadAudioClip;





    public override BaseData Clone()
    {
        TestData clone = new TestData();

        clone.id = this.id;
        clone.codeName = this.codeName;
        clone.description = this.description;
        clone.displayName = this.displayName;

        clone.animatorController = this.animatorController;
        clone.idleClip = this.idleClip;
        clone.runClip = this.runClip;
        clone.attackClip = this.attackClip;
        clone.hitClip = this.hitClip;
        clone.deadClip = this.deadClip;


        return clone;
    }

}

[CreateAssetMenu(fileName = "TestData", menuName = "GameData/TestData")]
public class TestList : BaseList<TestData>
{

}

