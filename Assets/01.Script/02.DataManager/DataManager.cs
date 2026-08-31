using System.Collections.Generic;
using UnityEngine;

public partial class DataManager : MonoBehaviour
{
    public static DataManager instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        LoadAllOfDataGenerated();
    }

    partial void LoadAllOfDataGenerated();

    private void LoadData<T>(DataRepositary<T> repositary, List<T> dataList) where T : BaseData
    {
        repositary.Clear();

        if (dataList == null)
        {
            Debug.Log($"{typeof(T).Name} 데이터가 Null");
        }

        repositary.Load(dataList);
    }

    /*
    public bool TryGetMonsterData(int id, out MonsterData monsterData)//아래tryget을 내가 작성중이라 비워둠. 여기에 out 넣거나 구현 안됨.
    {
        return monsters.TryGet(id, out monsterData);
    }
    public bool TryGetPlayerData(int id, out PlayerData playerData)
    {
        return players.TryGet(id, out playerData);
    }
    public bool TryGetWeaponData(int id, out StageData stageData)
    {
        return stages.TryGet(id, out stageData);
    }
    public bool TryGetItemData(int id, out ItemData itemData)
    {
        return items.TryGet(id, out itemData);
    }
    */
}
