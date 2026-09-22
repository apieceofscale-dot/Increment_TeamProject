using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterFacade : MonoBehaviour
{

    private CharacterControllers characterControllers;
    private CharacterControllers Controller => characterControllers;

    public bool IsInitialized => characterControllers != null && characterControllers.IsInitialized;
    public bool CanRun => characterControllers != null && characterControllers.CanRun;
    internal void Inject(CharacterControllers owner)
    {
        if (owner == null)
            throw new System.ArgumentNullException(nameof(owner));

        if (characterControllers != null && characterControllers != owner)
            throw new System.InvalidOperationException("연결된 캐릭터와 주입 대상 다름");

        characterControllers = owner;

        if (owner == CharacterControllers.Current && currentFacade == null)
            currentFacade = this;
    }

    #region 씬 전환용 외부 API
    private static CharacterFacade currentFacade;

    // 별도의 플레이어를 만들지 않고, 현재 DDOL 플레이어의 창구 반환
    public static CharacterFacade Current => currentFacade != null && currentFacade.characterControllers != null && currentFacade.characterControllers == CharacterControllers.Current ? currentFacade : null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetCurrentFacade() { currentFacade = null; }

    private void OnDestroy()
    {
        if (currentFacade == this)
            currentFacade = null;
    }

    public void PrepareForSceneChange() // 씬 전환 전에 캐릭터 이동·전투 준비 상태 정리
    {
        RequireSceneController().PrepareForSceneChange();
    }

    public void BindSceneBootstrap(BootStrapper sceneBootstrap) // 새 씬의 부트스트래퍼를 기존 캐릭터에 연결
    {
        RequireSceneController().BindSceneBootstrap(sceneBootstrap);
    }

    public void MoveToScenePosition(Vector3 spawnPosition) // 기존 캐릭터를 새 스테이지 시작 위치로 배치
    {
        RequireSceneController().MoveToScenePosition(spawnPosition);
    }

    private CharacterControllers RequireSceneController()
    {
        if (Controller == null || !Controller.IsInitialized || Controller != CharacterControllers.Current)
            throw new InvalidOperationException("초기화된 현재 플레이어의 Facade 필요");

        return Controller;
    }
    #endregion

    #region 공용 상세 객체 접근 용
    public CharacterStatus Status => Controller.Status; // 현재 기본 스탯·장비 보너스·합산 스탯 조회
    public CharacterInventory Inventory => Controller.Inventory; // 보유 장비 상세 조회용, 일반 UI 동작은 아래 장비 API 사용
    public CharacterEquipment Equipment => Controller.Equipment; // 착용 상태 상세 조회용, 일반 UI 동작은 아래 착용·해제 API 사용
    #endregion

    #region 캐릭터 생성 및 초기화
    public PlayerData Data => Controller.Data; // 캐릭터 생성에 사용한 원본 데이터 조회, 성장 후 수치는 Status 사용
    public void Initialize(PlayerData playerData) // Controller 참조 주입 후 초기 데이터 적용
    {
        Controller.Initialize(playerData);
    }
    #endregion

    #region 레벨 성장 및 능력치 포인트
    public CharacterStatUpgradeInfo GetStatUpgradeInfo() => Controller.GetStatUpgradeInfo(); // 일반·특별 포인트, 캐릭터 등급, 항목별 강화 횟수 조회
    public bool TryUpgradeStat(CharacterStatUpgradeType type) => Controller.TryUpgradeStat(type); // 강화 버튼: 선택한 항목을 1회 강화, 성공 여부 반환
    public long RequiredExp => Controller.RequiredExp; // 다음 레벨 요구 경험치 조회, 0이면 추가 레벨업 불가
    public event Action StatUpgradeChanged // 레벨업·강화 성공 후 포인트와 스탯 다시 조회
    {
        add
        {
            Controller.StatUpgradeChanged += value;
        }
        remove
        {
            if (Controller != null)
                Controller.StatUpgradeChanged -= value;
        }
    }
    #endregion

    #region 경험치 및 강화 표시
    public long CurrentExp => Controller.CurrentExp; // 현재 레벨에서 누적한 경험치 조회
    public int MainStatValue => Controller.MainStatValue; // 재 직업 주 스탯의 장비 포함 합산 값 조회
    public CharacterStatUpgradeOption GetStatUpgradeOption(CharacterStatUpgradeType type) => Controller.GetStatUpgradeOption(type);

    public event Action<long, long> ExpChanged // 경험치 바 갱신용, 현재 경험치·요구 경험치 순서로 전달
    {
        add
        {
            Controller.ExpChanged += value;
        }
        remove
        {
            if (Controller != null) Controller.ExpChanged -= value;
        }
    }
    public event Action<Sprite> PortraitChanged // 초상화 변경 시 이미지 갱신, null 처리 필요
    {
        add
        {
            Controller.PortraitChanged += value;
        }
        remove
        {
            if (Controller != null) Controller.PortraitChanged -= value;
        }
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

    public event Action InventoryChanged // 인벤토리 내 미착용 장비 목록이 달라지는 것을 알림
    {
        add
        {
            Controller.InventoryChanged += value;
        }
        remove
        {
            if (Controller != null)
                Controller.InventoryChanged -= value;
        }
    }

    public event Action EquipmentChanged // 장착 칸 갱신용
    {
        add
        {
            Controller.EquipmentChanged += value;
        }
        remove
        {
            if (Controller != null)
                Controller.EquipmentChanged -= value;
        }
    }

    public IReadOnlyList<CharacterInventoryEquipment> GetEquipmentInventory() // 착용 장비를 제외한 읽기 전용 복사본
    {
        return Controller.GetEquipmentInventory();
    }

    public bool TryGetItemData(int itemId, out ItemData data)
    {
        return Controller.TryGetItemData(itemId, out data);
    }

    public void EquipItem(Guid instanceId) //  부위 자동 착용/교체
    {
        Controller.EquipItem(instanceId);
    }

    public bool TryEquipItem(Guid instanceId) // 성공/실패 표시가 필요할 때 사용
    {
        return Controller.TryEquipItem(instanceId);
    }

    public bool UnequipItem(CharacterEquipmentSlot slot) // 장착칸 클릭, 성공하면 아이템이 미착용 목록에 다시 표시
    {
        return Controller.UnequipItem(slot);
    }
    #endregion

    #region 스킬 UI 및 스킬 장착용
    public int SkillSlotCount => CharacterControllers.SkillSlotCount; // 6개, 인덱스는 0~5

    // 이벤트를 직접 발행하지 않고 구독 창구만 제공
    public event Action<SkillSlotInfo> SkillSlotChanged
    {
        add { Controller.SkillSlotChanged += value; }
        remove
        {
            if (Controller != null)
                Controller.SkillSlotChanged -= value;
        }
    }

    // 슬롯별 이벤트 6개
    public IReadOnlyList<CharacterControllers.SkillCooldownChannel> SkillCooldownEvents => Controller.SkillCooldownEvents;

    // 프리젠터 초기화 시 0~5를 조회하여 아이콘/이름과 남은 쿨타임을 동기화
    public SkillSlotInfo GetEquippedSkill(int slotIndex) => Controller.GetEquippedSkill(slotIndex);
    public SkillCooldownInfo GetSkillCooldown(int slotIndex) => Controller.GetSkillCooldown(slotIndex);

    // 같은 캐릭터의 스킬 컴포넌트를 장착
    public bool EquipSkill(int slotIndex, CharacterSkillBase skill) => Controller.EquipSkill(slotIndex, skill);
    public bool UnequipSkill(int slotIndex) => Controller.UnequipSkill(slotIndex);

    // UI 버튼은 번호만 전달
    public bool UseSkill(int slotIndex) => Controller.UseSkill(slotIndex);
    #endregion

    #region 피해 계산 조회 및 알림
    public CharacterDamageMainStat DamageMainStat => Controller.DamageMainStat; // STR·DEX·INT·LUK 중 현재 주 스탯 종류 조회
    public CharacterDamageAttackData CaptureDamageAttack() => Controller.CaptureDamageAttack(); // 호출 시점의 공격 스탯을 복사, 이 호출만으로 피해를 주지는 않음
    public event Action<IDamageable, CharacterDamageResult> DamageResolved // 대상과 계산 결과 전달, 이 이벤트에서 피해를 다시 적용하지 않음
    {
        add 
        {
            Controller.DamageResolved += value; 
        }
        remove 
        { 
            if (Controller != null)
                Controller.DamageResolved -= value; 
        }
    }
    #endregion

    #region 몬스터 및 전투용
    public void TakeDamage(long damage) // 캐릭터 HP에서 전달한 피해량 차감
    {
        Controller.Status.TakeDamage(damage);
    }

    public void RecoverHp(long amount) // 캐릭터 현재 HP 회복
    {
        Controller.Status.RecoverHp(amount);
    }

    public bool UseMp(int amount) // MP 사용 요청, 부족하면 false 반환
    {
        return Controller.Status.UseMp(amount);
    }

    public void RecoverMp(int amount) // 캐릭터 현재 MP 회복
    {
        Controller.Status.RecoverMp(amount);
    }
    #endregion

    #region 보상 및 재화 용
    public void GainExp(long amount) // 경험치 지급 및 조건 충족 시 레벨업 처리
    {
        Controller.GainExp(amount);
    }

    public void SetMoney(long amount) // 획득량이 아닌 최종 보유 잔액 전달
    {
        Controller.SetMoney(amount);
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

    #region 아이템 스탯 연동 용
    // 현재 착용 장비 전체의 합계를 교체, 생략한 항목은 0
    public void SetEquipmentStats( // 현재 착용 장비 전체의 스탯 합계를 전달
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

    public bool RefreshEquipmentStats() // 장비 옵션 변경 후 착용 장비의 스탯 재계산 요청
    {
        return Controller.RefreshEquipmentStats(); // 조회·합산·적용 요청을 전달
    }

    public void ClearEquipmentStats() // 장비 보너스만 초기화, 기본 스탯은 유지
    {
        Controller.ClearEquipmentStats();
    }
    #endregion

    #region 자동사냥
    public void SetAutoFarming(bool enabled) // 자동사냥 ON/OFF
    {
        Controller.SetAutoFarming(enabled); 
    }

    public bool IsAutoFarming => Controller.IsAutoFarming; // 현재 ON/OFF 상태 표시
    public string AutoFarmingState => Controller.AutoFarmingState; // 자동사냥 상태 표시·디버깅용 상태 문자열 조회

    public void SetAutoFarmingTargetFilter(Func<Collider2D, bool> filter) // 자동사냥이 선택할 대상의 허용 조건 연결
    {
        Controller.SetAutoFarmingTargetFilter(filter);
    }
    #endregion

    #region 캐릭터 조작 및 자동전투용
    public void SetMoveInput(float input) // 좌우 이동 입력 전달, 정지 시 0 전달
    {
        Controller.SetMoveInput(input);
    }

    public void Jump() // 점프 요청, 실제 가능 여부는 Controller에서 판단
    {
        Controller.Jump();
    }

    public bool Attack() // 현재 기본 공격 사용 요청, 성공 여부 반환
    {
        return Controller.TryAttack();
    }

    public bool SetBasicAttackReplacement(CharacterSkillBase skill) // 기본 공격을 대체할 스킬 지정, 성공 여부 반환
    {
        return Controller.SetBasicAttackReplacement(skill);
    }

    public void RestoreBasicAttack() // 기본 공격 대체를 해제하고 일반 공격으로 복원
    {
        Controller.RestoreBasicAttack();
    }
    #endregion

    #region 직업 및 성장 용
    public bool ChangeJob(int id) // 지정 ID로 직업 변경 요청, 성공 여부 반환
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