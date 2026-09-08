using System;
using UnityEngine;

[Serializable]
public class PlayerData : BaseData
{
    public long maxHp;
    public int maxMp;
    public int recoverMpPerSec;
    public float moveSpeed;

    public int strength;
    public int dexterity;
    public int intelligence;
    public int luck;

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

    public int defence;
    public int dodgeRate;

    public override BaseData Clone()
    {
        PlayerData clone = new PlayerData();

        clone.id = this.id;
        clone.codeName = this.codeName;
        clone.description = this.description;
        clone.displayName = this.displayName;

        clone.maxHp = this.maxHp;
        clone.maxMp = this.maxMp;
        clone.recoverMpPerSec = this.recoverMpPerSec;
        clone.moveSpeed = this.moveSpeed;

        clone.strength = this.strength;
        clone.dexterity = this.dexterity;
        clone.intelligence = this.intelligence;
        clone.luck = this.luck;

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

        clone.defence = this.defence;
        clone.dodgeRate = this.dodgeRate;

        return clone;
    }
}

[CreateAssetMenu(fileName = "PlayerData", menuName = "GameData/PlayerData")]
public class PlayerList : BaseList<PlayerData>
{

}