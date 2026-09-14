public partial class DataManager
{
    public bool TryGetMonsterData(int id, out MonsterData monsterData)
    {
        return monsterRepository.TryGet(id, out monsterData);
    }

    public bool TryGetItemData(int id, out ItemData itemData)
    {
        return itemRepository.TryGet(id, out itemData);
    }

    public bool TryGetPlayerData(int id, out PlayerData playerData)
    {
        return playerRepository.TryGet(id, out playerData);
    }

    public bool TryGetStageData(int id, out StageData stageData)
    {
        return stageRepository.TryGet(id, out stageData);
    }

    public bool TryGetTestData(int id, out TestData testData)
    {
        return testRepository.TryGet(id, out testData);
    }
}
