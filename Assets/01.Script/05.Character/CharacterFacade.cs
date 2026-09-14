using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterFacade : MonoBehaviour
{
    [SerializeField] private CharacterControllers characterControllers;
    public CharacterStatus Status => characterControllers.Status;
    public CharacterInventory Inventory => characterControllers.Inventory;
    public CharacterEquipment Equipment => characterControllers.Equipment;
    public PlayerData Data { get; private set; }

    private void Awake()
    {
        if (characterControllers == null)
            characterControllers = GetComponent<CharacterControllers>();

        if (characterControllers == null)
            Debug.LogError("캐릭터 컨트롤러 없음", this);
    }

    public void Initialize(PlayerData playerData)
    {
        characterControllers.Initialize(playerData);
    }

    public void GainExp(long amount)
    {
        characterControllers.GainExp(amount);
    }

    public void TakeDamage(long damage)
    {
        characterControllers.Status.TakeDamage(damage);
    }

    public void RecoverHp(long amount)
    {
        characterControllers.Status.RecoverHp(amount);
    }

    public bool UseMp(int amount)
    {
        return characterControllers.Status.UseMp(amount);
    }

    public void SetMoveInput(float input)
    {
        characterControllers.SetMoveInput(input);
    }

    public void Jump()
    {
        characterControllers.Jump();
    }

    public bool Attack()
    {
        return characterControllers.TryAttack();
    }

    public bool ChangeJob(int id)
    {
        return characterControllers.ChangeJob(id);
    }

    public bool ChangeNextJob()
    {
        return characterControllers.ChangeNextJob();
    }

    public void RestoreBasicAttack()
    {
        characterControllers.RestoreBasicAttack();
    }

    public bool SetSlashBasicAttack()
    {
        return characterControllers.SetSlashBasicAttack();
    }

    public bool SetProjectileBasicAttack()
    {
        return characterControllers.SetProjectileBasicAttack();
    }

    public bool SetBasicAttackReplacement(CharacterSkillBase skill)
    {
        return characterControllers.SetBasicAttackReplacement(skill);
    }

    public bool UseSkillSlash()
    {
        return characterControllers.UseSkillSlash();
    }

    public bool UseSkillProjectile()
    {
        return characterControllers.UseSkillProjectile();
    }

    public bool UseSkillAttackBuff()
    {
        return characterControllers.UseSkillAttackBuff();
    }

    public void TestSkillLevelUp()
    {
        characterControllers.TestSkillLevelUp();
    }

    public void RecoverMp(int amount)
    {
        characterControllers.Status.RecoverMp(amount);
    }

    public bool AddEquipment(ItemStatus status, out Guid instanceId) // 방어구 1개 추가, 성공 여부와 개별 ID 반환
    {
        return characterControllers.AddEquipment(status, out instanceId);
    }

    public bool RemoveEquipment(Guid instanceId) // 개별 방어구 제거
    {
        return characterControllers.RemoveEquipment(instanceId);
    }

    public bool TryGetEquipment(Guid instanceId, out CharacterInventoryEquipment equipment) // 개별 방어구 조회
    {
        return characterControllers.TryGetEquipment(instanceId, out equipment);
    }

    public int GetEquipmentCount(int itemId) // 같은 아이템 ID의 보유 갯수
    {
        return characterControllers.GetEquipmentCount(itemId);
    }

    public IReadOnlyList<CharacterInventoryEquipment> GetInventoryEquipment() // 읽기 전용 목록 복사본
    {
        return characterControllers.GetInventoryEquipment();
    }

    public bool EquipEquipment(Guid instanceId, CharacterEquipmentSlot slot) // 방어구 착용
    {
        return characterControllers.EquipEquipment(instanceId, slot);
        // Hat(머리)=0, Top(상의)=1, Bottom(하의)=2, Gloves(장갑)=3, Cape(망토)=4, Shoulder(어깨)=5, Belt(허리)=6, Shoes(신발)=7, Ring1(반지1)=8, Ring2(반지2)=9, Necklace(목걸이)=10
    }

    public bool UnequipEquipment(CharacterEquipmentSlot slot) // 방어구 해제
    {
        return characterControllers.UnequipEquipment(slot);
    }

    public bool IsEquipmentEquipped(Guid instanceId) // 방어구 착용했는지
    {
        return characterControllers.IsEquipmentEquipped(instanceId);
    }

    public bool TryGetEquippedItem(CharacterEquipmentSlot slot, out CharacterInventoryEquipment item) // 착용한 방어구 조회
    {
        return characterControllers.TryGetEquippedItem(slot, out item);
    }

    public IReadOnlyDictionary<CharacterEquipmentSlot, Guid> GetEquipmentSlots()
    {
        return characterControllers.GetEquipmentSlots();
    }

    // 현재 착용 장비 전체의 합계를 교체, 생략한 항목은 0
    public void SetEquipmentStats(
        long maxHp = 0,
        int maxMp = 0,
        int recoverMpPerSec = 0,
        float moveSpeed = 0f,
        int strength = 0,
        int dexterity = 0,
        int intelligence = 0,
        int luck = 0,
        int attack = 0,
        float attackSpeedRate = 0f,
        int hitRate = 0,
        float criticalRate = 0f,
        float criticalDamage = 0f,
        float damageByMainStat = 0f,
        float damageOnBoss = 0f,
        float damageOnNormal = 0f,
        float armorPenetration = 0f,
        float finalDamage = 0f,
        long defence = 0,
        int dodgeRate = 0)
    {
        characterControllers.SetEquipmentStats(
            maxHp: maxHp,
            maxMp: maxMp,
            recoverMpPerSec: recoverMpPerSec,
            moveSpeed: moveSpeed,
            strength: strength,
            dexterity: dexterity,
            intelligence: intelligence,
            luck: luck,
            attack: attack,
            attackSpeedRate: attackSpeedRate,
            hitRate: hitRate,
            criticalRate: criticalRate,
            criticalDamage: criticalDamage,
            damageByMainStat: damageByMainStat,
            damageOnBoss: damageOnBoss,
            damageOnNormal: damageOnNormal,
            armorPenetration: armorPenetration,
            finalDamage: finalDamage,
            defence: defence,
            dodgeRate: dodgeRate);
    }

    public void ClearEquipmentStats()
    {
        characterControllers.ClearEquipmentStats();
    }
}
