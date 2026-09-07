public enum MonsterState
{
    Idle = 0,
    Trace = 1,
    Attack = 2,
    Dead = 3
}

public enum ItemType
{
    None = 0,
    Consumable = 1,
    Equipment = 2,
    Currency = 3,
    Weapon = 4
}

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
