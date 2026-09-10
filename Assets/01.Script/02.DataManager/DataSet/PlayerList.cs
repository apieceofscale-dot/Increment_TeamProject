using System;
using UnityEngine;

[Serializable]
public class PlayerData : BaseData
{
    // 기본 상태
    public int level;
    public long exp;
    public long maxHp;
    public long currentHp;
    public int maxMp;
    public int currentMp;
    public int recoverMpPerSec;
    public float moveSpeed;

    // 주 스탯
    public int strength;
    public int dexterity;
    public int intelligence;
    public int luck;

    // 공격 관련
    public int attack;
    public float attackSpeedRate;
    public int hitRate;
    public float criticalRate;
    public float criticalDamage;
    public float damageByMainStat;
    public float damageOnBoss;
    public float damageOnNormal;
    public float armorPenetration;
    public float finalDamage;

    // 방어 관련
    public long defence;
    public int dodgeRate;

    public override BaseData Clone()
    {
        PlayerData clone = new PlayerData();

        // 기본 데이터
        clone.id = this.id;
        clone.codeName = this.codeName;
        clone.description = this.description;
        clone.displayName = this.displayName;

        // 기본 상태
        clone.level = this.level;
        clone.exp = this.exp;
        clone.maxHp = this.maxHp;
        clone.currentHp = this.currentHp;
        clone.maxMp = this.maxMp;
        clone.currentMp = this.currentMp;
        clone.recoverMpPerSec = this.recoverMpPerSec;
        clone.moveSpeed = this.moveSpeed;

        // 주 스탯
        clone.strength = this.strength;
        clone.dexterity = this.dexterity;
        clone.intelligence = this.intelligence;
        clone.luck = this.luck;

        // 공격 관련
        clone.attack = this.attack;
        clone.attackSpeedRate = this.attackSpeedRate;
        clone.hitRate = this.hitRate;
        clone.criticalRate = this.criticalRate;
        clone.criticalDamage = this.criticalDamage;
        clone.damageByMainStat = this.damageByMainStat;
        clone.damageOnBoss = this.damageOnBoss;
        clone.damageOnNormal = this.damageOnNormal;
        clone.armorPenetration = this.armorPenetration;
        clone.finalDamage = this.finalDamage;

        // 방어 관련
        clone.defence = this.defence;
        clone.dodgeRate = this.dodgeRate;

        return clone;
    }
}

[CreateAssetMenu(fileName = "PlayerData", menuName = "GameData/PlayerData")]
public class PlayerList : BaseList<PlayerData>
{

}