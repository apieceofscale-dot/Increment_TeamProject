//자동생성됨. 수정금지.
using UnityEngine;

// 지금 호출하는 함수가 2개고, 반드시 첫 함수가 끝나고, 컴파일이 끝난 뒤 두 번째 함수가 로드 되어야 하니
// SessionState + [DidReloadScripts]를 쓰면 더할 나위 없이 좋겠으나 생략함. 시간 남으면 하기. 
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
