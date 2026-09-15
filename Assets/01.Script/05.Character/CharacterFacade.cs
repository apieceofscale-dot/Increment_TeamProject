using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterFacade : MonoBehaviour
{
    
    [SerializeField] private CharacterControllers characterControllers;
    private CharacterControllers Controller
    {
        get
        {
            if (characterControllers == null)
                characterControllers = GetComponent<CharacterControllers>();

            return characterControllers;
        }
    }

    private void Awake()
    {
        if (characterControllers == null)
            characterControllers = GetComponent<CharacterControllers>();

        if (characterControllers == null)
            Debug.LogError("캐릭터 컨트롤러 없음", this);
    }

    #region 공용 상세 객체 접근 용
    public CharacterStatus Status => Controller.Status;
    public CharacterInventory Inventory => Controller.Inventory;
    public CharacterEquipment Equipment => Controller.Equipment;
    #endregion

    #region 캐릭터 생성 및 초기화
    public PlayerData Data => Controller.Data;
    public void Initialize(PlayerData playerData)
    {
        Controller.Initialize(playerData);
    }
    #endregion

    #region UI용 현재값 조회
    public long CurrentHp => Controller.CurrentHp; // 현재 체력
    public long MaxHp => Controller.MaxHp; // 최대 체력
    public int CurrentMp => Controller.CurrentMp; // 현재 마나
    public int MaxMp => Controller.MaxMp; // 최대 마나
    public int Level => Controller.Level; // 현재 레벨
    public long Money => Controller.Money; // 현재 재화
    public int CombatPower => Controller.CombatPower; // 현재 최종 공격력
    public string JobName => Controller.JobName; // 직업 이름

    public Sprite GetCharacterPortrait()
    {
        return Controller.GetCharacterPortrait(); // Controller에 초상화 조회 요청 전달
    }
    #endregion

    #region UI용 변경 이벤트 구독
    public event Action<long, long> HpChanged // 현재 체력, 최대 체력 전달 이벤트
    {
        add { Controller.HpChanged += value; }
        remove
        {
            if (Controller != null)
                Controller.HpChanged -= value;
        }
    }

    public event Action<int, int> MpChanged // 현재마나, 최대 마나 전달 이벤트
    {
        add { Controller.MpChanged += value; }
        remove
        {
            if (Controller != null)
                Controller.MpChanged -= value;
        }
    }

    public event Action<int> LevelChanged // 레벨 변경
    {
        add { Controller.LevelChanged += value; }
        remove
        {
            if (Controller != null)
                Controller.LevelChanged -= value;
        }
    }

    public event Action<long> MoneyChanged // 재화 변경
    {
        add { Controller.MoneyChanged += value; }
        remove
        {
            if (Controller != null)
                Controller.MoneyChanged -= value;
        }
    }

    public event Action<int> CombatPowerChanged // 최종 공격력 변경
    {
        add { Controller.CombatPowerChanged += value; }
        remove
        {
            if (Controller != null)
                Controller.CombatPowerChanged -= value;
        }
    }

    public event Action<string> JobNameChanged // 직업 이름 변경
    {
        add { Controller.JobNameChanged += value; }
        remove
        {
            if (Controller != null)
                Controller.JobNameChanged -= value;
        }
    }
    public void RefreshUiEvents()
    {
        Controller.RefreshUiEvents(); // 모든 현재 값 알리도록 변경
    }
    #endregion

    #region 몬스터 및 전투용
    public void TakeDamage(long damage)
    {
        Controller.Status.TakeDamage(damage);
    }

    public void RecoverHp(long amount)
    {
        Controller.Status.RecoverHp(amount);
    }

    public bool UseMp(int amount)
    {
        return Controller.Status.UseMp(amount);
    }

    public void RecoverMp(int amount)
    {
        Controller.Status.RecoverMp(amount);
    }
    #endregion

    #region 보상 및 재화 용
    public void GainExp(long amount)
    {
        Controller.GainExp(amount);
    }

    public void SetMoney(long amount)
    {
        Controller.SetMoney(amount); // 재화 잔액 전달
    }
    #endregion

    #region 아이템 및 인벤토리 용
    public bool AddEquipment(ItemStatus status, out Guid instanceId) // 방어구 1개 추가, 성공 여부와 개별 ID 반환
    {
        return Controller.AddEquipment(status, out instanceId);
    }

    public bool RemoveEquipment(Guid instanceId) // 개별 방어구 제거
    {
        return Controller.RemoveEquipment(instanceId);
    }

    public bool TryGetEquipment(Guid instanceId, out CharacterInventoryEquipment equipment) // 개별 방어구 조회
    {
        return Controller.TryGetEquipment(instanceId, out equipment);
    }

    public int GetEquipmentCount(int itemId) // 같은 아이템 ID의 보유 갯수
    {
        return Controller.GetEquipmentCount(itemId);
    }

    public IReadOnlyList<CharacterInventoryEquipment> GetInventoryEquipment() // 읽기 전용 목록 복사본
    {
        return Controller.GetInventoryEquipment();
    }
    #endregion

    #region 장비 및 장비 UI용
    public bool EquipEquipment(Guid instanceId, CharacterEquipmentSlot slot) // 방어구 착용
    {
        return Controller.EquipEquipment(instanceId, slot);
        // Hat(머리)=0, Top(상의)=1, Bottom(하의)=2, Gloves(장갑)=3, Cape(망토)=4, Shoulder(어깨)=5, Belt(허리)=6, Shoes(신발)=7, Ring1(반지1)=8, Ring2(반지2)=9, Necklace(목걸이)=10
    }

    public bool UnequipEquipment(CharacterEquipmentSlot slot) // 방어구 해제
    {
        return Controller.UnequipEquipment(slot);
    }

    public bool IsEquipmentEquipped(Guid instanceId) // 방어구 착용했는지
    {
        return Controller.IsEquipmentEquipped(instanceId);
    }

    public bool TryGetEquippedItem(CharacterEquipmentSlot slot, out CharacterInventoryEquipment item) // 착용한 방어구 조회
    {
        return Controller.TryGetEquippedItem(slot, out item);
    }

    public IReadOnlyDictionary<CharacterEquipmentSlot, Guid> GetEquipmentSlots()
    {
        return Controller.GetEquipmentSlots();
    }
    #endregion

    #region 아이템 스탯 연동 용
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
        Controller.SetEquipmentStats(
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

    public bool RefreshEquipmentStats()
    {
        return Controller.RefreshEquipmentStats(); // 조회·합산·적용 요청을 전달
    }

    public void ClearEquipmentStats()
    {
        Controller.ClearEquipmentStats();
    }
    #endregion

    #region 캐릭터 조작 및 자동전투용
    public void SetMoveInput(float input)
    {
        Controller.SetMoveInput(input);
    }

    public void Jump()
    {
        Controller.Jump();
    }

    public bool Attack()
    {
        return Controller.TryAttack();
    }

    public bool SetBasicAttackReplacement(CharacterSkillBase skill)
    {
        return Controller.SetBasicAttackReplacement(skill);
    }

    public void RestoreBasicAttack()
    {
        Controller.RestoreBasicAttack();
    }
    #endregion

    #region 직업 및 성장 용
    public bool ChangeJob(int id)
    {
        return Controller.ChangeJob(id);
    }
    #endregion

    #region 개발 중 테스트 용
    public bool ChangeNextJob()
    {
        return Controller.ChangeNextJob();
    }

    public bool SetSlashBasicAttack()
    {
        return Controller.SetSlashBasicAttack();
    }

    public bool SetProjectileBasicAttack()
    {
        return Controller.SetProjectileBasicAttack();
    }

    public bool UseSkillSlash()
    {
        return Controller.UseSkillSlash();
    }

    public bool UseSkillProjectile()
    {
        return Controller.UseSkillProjectile();
    }

    public bool UseSkillAttackBuff()
    {
        return Controller.UseSkillAttackBuff();
    }

    public void TestSkillLevelUp()
    {
        Controller.TestSkillLevelUp();
    }
    #endregion
}