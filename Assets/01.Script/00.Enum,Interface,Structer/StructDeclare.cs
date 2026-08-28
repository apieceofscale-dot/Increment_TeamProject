using UnityEngine;

public struct MonsterDiedInfo
{
    public int MonsterId;
    public Vector3 Position;
    public MonsterController Source;
}

public struct ItemPickedUpInfo
{
    public int ItemId;
    public ItemType Type;
    public int Value;
    public GameObject Collector;
    public ItemController Source;
}
