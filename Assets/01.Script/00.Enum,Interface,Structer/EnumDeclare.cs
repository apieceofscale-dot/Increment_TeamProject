// 아까 enum값 중복되지 않게 하라 말씀주셨는데 일단 폴더 번호를 사용했으니 검토 부탁드려요
public enum BootLayer
{
    DataManager = 2,
    ObjectPool = 3,
    Factory = 4,
    Character = 5,
    Monster = 6,
    Item = 7,
    ItemManager = 8,
    StageManager = 9,
    SoundManager = 10,
    SaveManager = 11,
    UIManager = 12,
    Codex = 13,
}

// 추후 값 변경
public enum StageType
{
    Farm, // 하위스테이지
    //MidBoss, // 중간보스를 별도 스테이지로 빼야 할 지 고민입니다.
    Boss,
    Dungeon, // 경험치던전

}

public enum StageState
{
    None,
    Battle,
    Farming, // 다음 스테이지 도전 가능한 상태일때(무한파밍중)
    Cleared, // 보스 
    Failed
}