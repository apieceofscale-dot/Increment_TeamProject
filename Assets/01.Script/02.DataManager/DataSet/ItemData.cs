using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class ItemData : BaseData
{

    //스탯만들기

    //생성자 만들기

    public override BaseData Clone()
    {
        //생성자 반환하기.
        throw new System.NotImplementedException();
    }
}

[CreateAssetMenu(fileName = "CharacterData", menuName = "GameData/CharacterData")]
public class ItemList : ScriptableObject
{
    public List<ItemData> itemList = new List<ItemData>();    //그냥 구색 맞추기로 넣은 거 아무 역할도 안함.
}