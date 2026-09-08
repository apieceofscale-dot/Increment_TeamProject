//자동생성됨. 수정금지.
using UnityEngine;
public partial class DataManager
{
    [SerializeField] private ItemList itemList;
    [SerializeField] private MonsterList monsterList;
    [SerializeField] private PlayerList playerList;
    [SerializeField] private StageList stageList;
    [SerializeField] private TestList testList;

    private readonly DataRepositary<ItemData> itemRepository =  new DataRepositary<ItemData>();

    private readonly DataRepositary<MonsterData> monsterRepository =  new DataRepositary<MonsterData>();

    private readonly DataRepositary<PlayerData> playerRepository =  new DataRepositary<PlayerData>();

    private readonly DataRepositary<StageData> stageRepository =  new DataRepositary<StageData>();

    private readonly DataRepositary<TestData> testRepository =  new DataRepositary<TestData>();


    partial void LoadAllOfDataGenerated()
    {
        LoadData(itemRepository, itemList.baseList);
        LoadData(monsterRepository, monsterList.baseList);
        LoadData(playerRepository, playerList.baseList);
        LoadData(stageRepository, stageList.baseList);
        LoadData(testRepository, testList.baseList);
    }
}
