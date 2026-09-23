using System;
using UnityEngine;

[Serializable]
public class MonsterData : BaseData
{
    public int maxHp = 10;
    public int attackDamage = 1;
    public float moveSpeed = 1.5f;
    public float traceRange = 6f;
    public float attackRange = 1.4f;
    public float attackCooldown = 1f;
    public int dropTableId;
    // [추가] 처치 경험치. 기존 데이터는 기본 10이며 몬스터별로 조절한다.
    [Min(0)] public long expReward = 10;

    public RuntimeAnimatorController animatorController;
    public AnimationClip idleClip;
    public AnimationClip runClip;
    public AnimationClip attackClip;
    public AnimationClip hitClip;
    public AnimationClip deadClip;

    public AudioClip idleAudioClip;
    public AudioClip runAudioClip;
    public AudioClip attackAudioClip;
    public AudioClip hitAudioClip;
    public AudioClip deadAudioClip;

    public override BaseData Clone()
    {
        MonsterData clone = new MonsterData();

        clone.id = id;
        clone.codeName = codeName;
        clone.description = description;
        clone.displayName = displayName;

        clone.maxHp = maxHp;
        clone.attackDamage = attackDamage;
        clone.moveSpeed = moveSpeed;
        clone.traceRange = traceRange;
        clone.attackRange = attackRange;
        clone.attackCooldown = attackCooldown;
        clone.dropTableId = dropTableId;
        // [연결] 런타임 데이터 복제 시에도 경험치 보상을 유지한다.
        clone.expReward = expReward;

        clone.animatorController = animatorController;
        clone.idleClip = idleClip;
        clone.runClip = runClip;
        clone.attackClip = attackClip;
        clone.hitClip = hitClip;
        clone.deadClip = deadClip;

        clone.idleAudioClip = idleAudioClip;
        clone.runAudioClip = runAudioClip;
        clone.attackAudioClip = attackAudioClip;
        clone.hitAudioClip = hitAudioClip;
        clone.deadAudioClip = deadAudioClip;

        return clone;
    }
}

[CreateAssetMenu(fileName = "MonsterData", menuName = "GameData/MonsterData")]
public class MonsterList : BaseList<MonsterData>
{
}
