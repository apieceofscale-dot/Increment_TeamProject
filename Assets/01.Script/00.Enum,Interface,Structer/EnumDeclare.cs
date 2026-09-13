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


// 부트 대상 목록 -> 주석에 들어간 매니저와 파사드 쪽에 구현
// -> 씬에 하나씩 존재하는 매니저만. 런타임 생성 객체는 대상x Character, Monster, Item은 제거
// -> 그 객체가 필요한 참조는 자기를 만든 팩토리가 생성 시점에 넣어준다.
// 같은 값끼리 순서는 보장되지 않는데, 필요해지면 분리할 예정

public enum BootLayer // 
{
    DataManager = 2, // DataManager
    ObjectPool = 3, // MonsterObjectPoolManager, ItemObjectPoolManager
    Navi2D = 4, // Navi2DGridData, Navi2DPathFinder
    Factory = 5, // CharacterFactory, MonsterFactory, ItemFactory
    ItemManager = 6,// ItemDropFacade
    StageManager = 7, // StageFacade
    SoundManager = 8,
    SaveManager = 9,
    UIManager = 10,
    Codex = 11,
}

public enum StageType
{
    Farm,
    Boss,
    Dungeon,
}

public enum StageState
{
    None,
    Battle,
    Cleared,
    Failed
}

public enum CharacterEquipmentSlot // 장비가 착용될 슬롯
{
    Hat = 0,
    Top = 1,
    Bottom = 2,
    Gloves = 3,
    Cape = 4,
    Shoulder = 5,
    Belt = 6,
    Shoes = 7,
    Ring1 = 8,
    Ring2 = 9,
    Necklace = 10
}

public enum CharacterArmorPart // 장비 부위 종류
{
    Hat = 0,
    Top = 1,
    Bottom = 2,
    Gloves = 3,
    Cape = 4,
    Shoulder = 5,
    Belt = 6,
    Shoes = 7,
    Ring = 8,
    Necklace = 9
}