using System;

using UnityEngine;



/// <summary>
/// 몬스터 SO. CSV→MonsterId enum·필드 자동 반영. Load 시 Clone 복제본이 런타임 단일 소스.
/// 이벤트 구조체는 <see cref="MonsterDiedInfo.From"/> 등으로 SO에서 id·dropTableId만 넘긴다.
/// </summary>

[Serializable]

public class MonsterData : BaseData

{

    public int maxHp = 10;

    public int attackDamage = 1;

    public float moveSpeed = 1.5f;

    public float traceRange = 6f;

    public float attackRange = 1.4f;

    public float attackCooldown = 1f;

    /// <summary>ItemDropManager가 조회하는 드랍 풀 id. 몬스터는 판정하지 않고 id만 전달.</summary>
    public int dropTableId;



    public Sprite icon;

    public Sprite worldSprite;



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



        clone.icon = icon;

        clone.worldSprite = worldSprite;



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


