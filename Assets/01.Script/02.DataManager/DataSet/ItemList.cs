using System;
using UnityEngine;

[Serializable]
public class ItemData : BaseData
{
    public ItemType itemType = ItemType.Consumable;
    public int value = 1;
    public int upgradeStep = 1;

    public Sprite icon;

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
        ItemData clone = new ItemData();

        clone.id = id;
        clone.codeName = codeName;
        clone.description = description;
        clone.displayName = displayName;

        clone.itemType = itemType;
        clone.value = value;
        clone.upgradeStep = upgradeStep;

        clone.icon = icon;

        clone.animatorController = animatorController;
        clone.idleClip = idleClip;
        clone.runClip = runClip;
        clone.attackClip = attackClip;
        clone.hitClip = hitClip;
        clone.deadClip = deadClip;

        clone.idleAudioClip = idleAudioClip;
        clone.runAudioClip = runAudioClip;
        clone.attackAudioClip = attackAudioClip;
        clone.hitAudioClip = hitAudioClip;
        clone.deadAudioClip = deadAudioClip;

        return clone;
    }
}

[CreateAssetMenu(fileName = "ItemData", menuName = "GameData/ItemData")]
public class ItemList : BaseList<ItemData>
{
}
