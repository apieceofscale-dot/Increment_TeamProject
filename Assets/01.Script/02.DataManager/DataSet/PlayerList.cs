using System;
using UnityEngine;

[Serializable]
public class PlayerData : BaseData
{

    //필드선언


    public override BaseData Clone()
    {
        PlayerData clone = new PlayerData();

        clone.id = this.id;
        clone.codeName = this.codeName;
        clone.description = this.description;
        clone.displayName = this.displayName;

        return clone;
    }
}

[CreateAssetMenu(fileName = "PlayerData", menuName = "GameData/PlayerData")]
public class PlayerList : BaseList<PlayerData>
{

}