using UnityEngine;

public class MonsterData : BaseData
{
    public int maxHp = 10;
    public int attackDamage = 1;
    public float moveSpeed = 1.5f;
    public float traceRange = 6f;
    public float attackRange = 1.4f;
    public float attackCooldown = 1f;
    public int dropItemId;
    public float dropChance = 1f;

    public override BaseData Clone()
    {
        return (MonsterData)MemberwiseClone();
    }
}

[CreateAssetMenu(fileName = "MonsterData", menuName = "GameData/MonsterData")]
public class MonsterList : ScriptableObject
{
    public System.Collections.Generic.List<MonsterData> monsterList = new System.Collections.Generic.List<MonsterData>();
}
