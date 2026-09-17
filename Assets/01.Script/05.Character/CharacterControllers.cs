using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CharacterJobAdvancedment), typeof(CharacterInventory))]
[RequireComponent(typeof(CharacterEquipment))]
public class CharacterControllers : MonoBehaviour //기존 컴포넌트랑 이름 같아서 s붙임
{
    #region
    public const int SkillSlotCount = 6;

    [SerializeField] private CharacterSkillBase[] skillSlots = new CharacterSkillBase[SkillSlotCount];

    public sealed class SkillCooldownChannel
    {
        public event Action<SkillCooldownInfo> Changed;
        internal void Raise(SkillCooldownInfo data) { Changed?.Invoke(data); }
    }

    // 장착 교체 해제 때 전달
    public event Action<SkillSlotInfo> SkillSlotChanged;
    private SkillCooldownChannel[] skillCooldownChannels;
    private IReadOnlyList<SkillCooldownChannel> skillCooldownEvents;
    private readonly bool[] wasSkillCooling = new bool[SkillSlotCount];
    private readonly CharacterSkillBase[] observedSkills = new CharacterSkillBase[SkillSlotCount];

    // 스킬 컬렉션 길이는 6으로 고정함
    public IReadOnlyList<SkillCooldownChannel> SkillCooldownEvents
    {
        get { EnsureSkillSlots(); return skillCooldownEvents; }
    }

    private void EnsureSkillSlots()
    {
        if (skillSlots == null)
            skillSlots = new CharacterSkillBase[SkillSlotCount];
        else if (skillSlots.Length != SkillSlotCount)
            Array.Resize(ref skillSlots, SkillSlotCount);

        if (skillCooldownChannels != null)
            return;
        
        skillCooldownChannels = new SkillCooldownChannel[SkillSlotCount];

        for (int i = 0; i < SkillSlotCount; i++)
            skillCooldownChannels[i] = new SkillCooldownChannel();

        skillCooldownEvents = Array.AsReadOnly(skillCooldownChannels);
    }

    // 잘못 연결한 다른 캐릭터의 스킬 및 같은 컴포넌트 중복 제거
    private void ValidateInitialSkillSlots()
    {
        EnsureSkillSlots();
        var seen = new HashSet<CharacterSkillBase>();

        for (int i = 0; i < SkillSlotCount; i++)
        {
            CharacterSkillBase entry = skillSlots[i];
            if (entry != null && (!entry.BelongsTo(this) || !seen.Add(entry)))
            {
                Debug.LogWarning($"스킬 슬롯 {i}: 소유 캐릭터 또는 중복 연결을 확인하세요.", this);
                skillSlots[i] = null;
            }
        }
    }

    private static bool IsValidSkillSlot(int slotIndex)
    {
        return slotIndex >= 0 && slotIndex < SkillSlotCount;
    }

    // UI 최초 표시나 재생성 시 호출. 현재 상태 동기화 필요
    public SkillSlotInfo GetEquippedSkill(int slotIndex) //
    {
        EnsureSkillSlots();

        if (!IsValidSkillSlot(slotIndex))
            throw new ArgumentOutOfRangeException(nameof(slotIndex));

        CharacterSkillBase entry = skillSlots[slotIndex];

        return new SkillSlotInfo(slotIndex, entry != null, entry != null ? entry.SkillIcon : null, entry != null ? entry.RuntimeSkill.SkillName : string.Empty);
    }

    public SkillCooldownInfo GetSkillCooldown(int slotIndex)
    {
        EnsureSkillSlots();

        if (!IsValidSkillSlot(slotIndex))
            throw new ArgumentOutOfRangeException(nameof(slotIndex));

        CharacterSkillBase entry = skillSlots[slotIndex];

        if (entry == null)
            return new SkillCooldownInfo(slotIndex, 0f, 0f);

        CharacterSkill runtime = entry.RuntimeSkill;

        return new SkillCooldownInfo(slotIndex, runtime.RemainingCooldown, runtime.LastUsedCooldown);
    }

    public bool EquipSkill(int slotIndex, CharacterSkillBase entry)
    {
        EnsureSkillSlots();

        if (!IsValidSkillSlot(slotIndex) || entry == null || !entry.BelongsTo(this))
            return false;
        
        for (int i = 0; i < SkillSlotCount; i++)
            if (i != slotIndex && skillSlots[i] == entry)
                return false;
        
        if (skillSlots[slotIndex] == entry)
            return true;

        skillSlots[slotIndex] = entry;
        PublishSkillSlot(slotIndex);

        return true;
    }

    public bool UnequipSkill(int slotIndex)
    {
        EnsureSkillSlots();
        if (!IsValidSkillSlot(slotIndex) || skillSlots[slotIndex] == null) return false;
        skillSlots[slotIndex] = null;
        PublishSkillSlot(slotIndex);
        return true;
    }

    public bool UseSkill(int slotIndex)
    {
        EnsureSkillSlots();

        if (!isActiveAndEnabled || !IsValidSkillSlot(slotIndex))
            return false;

        CharacterSkillBase entry = skillSlots[slotIndex];

        if (entry == null || !entry.BelongsTo(this) || !entry.TryUse())
            return false;

        PublishSkillCooldown(slotIndex); // 성공 직후 UI에 즉시 알림

        return true;
    }

    private void PublishSkillSlot(int slotIndex)
    {
        observedSkills[slotIndex] = skillSlots[slotIndex];
        SkillSlotChanged?.Invoke(GetEquippedSkill(slotIndex)); // 아이콘/이름부터 설정
        PublishSkillCooldown(slotIndex); // 교체한 스킬의 진행 상태도 함께 동기화
    }

    private void PublishSkillCooldown(int slotIndex)
    {
        SkillCooldownInfo data = GetSkillCooldown(slotIndex);
        wasSkillCooling[slotIndex] = data.TimeLeft > 0f;
        skillCooldownChannels[slotIndex].Raise(data);
    }
    #endregion

    private CharacterStatus status;
    public CharacterStatus Status
    {
        get
        {
            if (status == null)
                status = new CharacterStatus();
            return status;
        }
    }

    [SerializeField] private CharacterInventory characterInventory;
    public CharacterInventory Inventory
    {
        get
        {
            if (characterInventory == null)
                characterInventory = GetComponent<CharacterInventory>();

            return characterInventory;
        }
    }

    private CharacterEquipment characterEquipment;
    public CharacterEquipment Equipment
    {
        get
        {
            if (characterEquipment == null)
                characterEquipment = GetComponent<CharacterEquipment>();
            return characterEquipment;
        }
    }

    [SerializeField] private Animator animator;
    public Animator Animator
    {
        get
        {
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
            return animator;
        }
    }

    private CharacterLevelUpProvider characterLevelUpProvider;
    private CharacterSkillLevelUpProvider skillLevelUpProvider;
    public PlayerData Data { get; private set; }

    // 이동 및 점프 관련
    private Rigidbody2D rigid;
    private float moveInput;

    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private Transform visualRoot;

    // 공격 관련
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private LayerMask monsterLayer;

    // 스킬 테스트용
    [SerializeField] private CharacterSkillSlash skillSlash;
    [SerializeField] private CharacterSkillProjectile skillProjectile;
    [SerializeField] private CharacterSkillAttackBuff skillAttackBuff;

    private float attackReadyTime = float.NegativeInfinity;
    private CharacterSkillBase basicAttackReplacement;
    private CharacterJobAdvancedment jobAdvancedment;
    public int FacingDirection { get; private set; } = 1;
    public PlayerData CurrentJob => GetJobController().CurrentJob;

    public void Initialize(PlayerData playerData)
    {
        if (playerData == null)
        {
            Debug.LogError("플레이어 데이터 없음", this);
            return;
        }

        Data = playerData;
        Status.Initialize(playerData);

        ChangeJob(playerData.id);
        RefreshEquipmentStats();
    }

    private CharacterJobAdvancedment GetJobController()
    {
        if (jobAdvancedment == null)
            jobAdvancedment = GetComponent<CharacterJobAdvancedment>();

        return jobAdvancedment;
    }

    public bool ChangeJob(int id)
    {
        return GetJobController().TryChangeJob(id);
    }

    public bool ChangeNextJob()
    {
        return GetJobController().TryChangeNextJob();
    }

    private void Awake()
    {
        ValidateInitialSkillSlots();

        rigid = GetComponent<Rigidbody2D>();
        jobAdvancedment = GetComponent<CharacterJobAdvancedment>();

        if (visualRoot != null)
            FacingDirection = visualRoot.localScale.x < 0f ? -1 : 1;

        characterLevelUpProvider = new CharacterLevelUpProvider();
        skillLevelUpProvider = new CharacterSkillLevelUpProvider();
    }

    private void Update()
    {
        EnsureSkillSlots();
        for (int i = 0; i < SkillSlotCount; i++)
        {
            if (!ReferenceEquals(observedSkills[i], skillSlots[i]) || (!ReferenceEquals(skillSlots[i], null) && skillSlots[i] == null))
            {
                if (skillSlots[i] == null)
                    skillSlots[i] = null;

                PublishSkillSlot(i);

                continue;
            }
            SkillCooldownInfo data = GetSkillCooldown(i);

            if (data.TimeLeft > 0f || wasSkillCooling[i])
                PublishSkillCooldown(i);
        }
    }

    private void FixedUpdate()
    {
        if (IsAutoFarming)
            AutoFarming.Tick();

        if (!IsAutoFarming)
        {
            if (Status.CurrentHp > 0)
                Move();
            else
                rigid.linearVelocity = new Vector2(0f, rigid.linearVelocity.y);
        }
        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        Animator currentAnimator = Animator;

        if (currentAnimator == null)
            return;

        bool isJump = !IsGrounded() || rigid.linearVelocity.y > 0.1f;
        bool isRun = !isJump && Mathf.Abs(rigid.linearVelocity.x) > 0.01f;

        currentAnimator.SetBool("isRun", isRun);
        currentAnimator.SetBool("isJump", isJump);
    }

    public void GainExp(long amount)
    {
        if (amount <= 0)
            return;

        Status.AddExp(amount);
        CheckLevelUp();
    }

    private void CheckLevelUp() // 레벨 수치 상승
    {
        if (characterLevelUpProvider == null)
            characterLevelUpProvider = new CharacterLevelUpProvider();

        while (true) // 보유 경험치량이 다음 레벨 업 요구 경험치 보다 많으면 반복해서 레벨업함
        {
            long requiredExp = characterLevelUpProvider.GetRequiredExp(Status.Level);

            if (requiredExp <= 0)
            {
                Debug.LogWarning("요구 경험치는 0보다 커야됨", this);
                break;
            }

            if (Status.Exp < requiredExp)
                break;

            status.UseExp(requiredExp);
            status.IncreaseLevel();

            ApplyLevelUpGrowth();
        }
    }

    public void SetMoveInput(float input)
    {
        if (IsAutoFarming)
            return;

        moveInput = Mathf.Clamp(input, -1f, 1f);
        UpdateDirection();
    }

    private void Move()
    {
        rigid.linearVelocity = new Vector2(moveInput * Status.MoveSpeed, rigid.linearVelocity.y);
    }

    private bool IsGrounded()
    {
        return groundCheck != null && Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer) != null;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        if (attackPoint != null)
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    public void Jump()
    {
        if (IsAutoFarming || Status.CurrentHp <= 0)
            return;

        if (!IsGrounded())
            return;

        rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, jumpForce);
    }

    private void UpdateDirection()
    {
        if (moveInput == 0f)
        {
            return;
        }

        FacingDirection = moveInput > 0f ? 1 : -1;

        if (visualRoot == null)
            return;

        Vector3 scale = visualRoot.localScale;
        scale.x = Mathf.Abs(scale.x) * (moveInput > 0f ? 1f : -1f);
        visualRoot.localScale = scale;
    }

    private CharacterAutoFarming autoFarming;
    private CharacterAutoFarming AutoFarming
    {
        get
        {
            if (autoFarming == null)
                autoFarming = GetComponent<CharacterAutoFarming>();

            return autoFarming;
        }
    }
    public bool IsAutoFarming => AutoFarming != null && AutoFarming.IsRunning;
    public string AutoFarmingState => AutoFarming != null ? AutoFarming.CurrentState : "Idle";
    public bool AutoFarmingConfigured => groundCheck != null && attackPoint != null && groundLayer.value != 0 && monsterLayer.value != 0;
    public bool AutoFarmingGrounded => IsGrounded() && rigid != null && Mathf.Abs(rigid.linearVelocity.y) < 0.1f;
    public LayerMask AutoFarmingGroundMask => groundLayer;
    public LayerMask AutoFarmingMonsterMask => monsterLayer;

    public void SetAutoFarming(bool enabled)
    {
        if (AutoFarming != null)
            AutoFarming.SetAutoFarming(enabled);
        else if (enabled)
            Debug.LogWarning("자동사냥 컴포넌트 추가", this);
    }

    public void SetAutoFarmingTargetFilter(Func<Collider2D, bool> filter)
    {
        if (AutoFarming != null)
            AutoFarming.TargetFilter = filter;
    }

    internal void StopAutoFarmingMovement()
    {
        moveInput = 0f;
        if (rigid != null)
            rigid.linearVelocity = new Vector2(0f, rigid.linearVelocity.y);
    }

    public void FaceAutoFarmingTarget(float worldx)
    {
        float delta = worldx - transform.position.x;

        if (Mathf.Abs(delta) < 0.01f)
            return;

        FacingDirection = delta > 0f ? 1 : -1;

        if (visualRoot == null)
            return;

        Vector3 scale = visualRoot.localScale;
        scale.x = Mathf.Abs(scale.x) * FacingDirection;
        visualRoot.localScale = scale;
    }

    public bool CanAutoFarmingAttack(Collider2D target)
    {
        if (target == null || !target.enabled || !target.gameObject.activeInHierarchy || attackPoint == null)
            return false;

        Vector2 origin = attackPoint.position;
        Vector2 closest = target.ClosestPoint(origin);

        if ((closest - origin).sqrMagnitude > attackRange * attackRange)
            return false;

        return Physics2D.Linecast(origin, target.bounds.center, groundLayer).collider == null;
    }

    public bool TryAttack()
    {
        if (Status == null || Status.CurrentHp <= 0 || Time.time < attackReadyTime)
            return false;

        if (basicAttackReplacement != null)
        {
            if (!basicAttackReplacement.TryUse())
                return false;

            attackReadyTime = Time.time + basicAttackReplacement.GetUseInterval();
            return true;
        }

        if (attackPoint == null)
            return false;

        attackReadyTime = Time.time + GetAttackInterval();
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, monsterLayer);
        HashSet<IDamageable> damaged = new HashSet<IDamageable>();

        foreach (Collider2D hit in hits)
        {
            if (hit == null || !hit.gameObject.activeInHierarchy || hit.transform.IsChildOf(transform))
                continue;
            IDamageable target = hit.GetComponentInParent<IDamageable>();
            if (target != null && damaged.Add(target))
                target.TakeDamage(Status.Attack);
        }
        return true;
    }

    private float GetAttackInterval()
    {
        return 1f / (1f + Mathf.Clamp(Status.AttackSpeedRate, 0f, 1.5f));
    }

    public bool SetBasicAttackReplacement(CharacterSkillBase replacement)
    {
        if (replacement != null && (!replacement.CanReplaceBasicAttack))
            return false;
        basicAttackReplacement = replacement;

        return true;
    }

    public void RestoreBasicAttack()
    {
        SetBasicAttackReplacement(null);
    }
    public bool SetSlashBasicAttack()
    {
        return skillSlash != null && SetBasicAttackReplacement(skillSlash);
    }
    public bool SetProjectileBasicAttack()
    {
        return skillProjectile != null && SetBasicAttackReplacement(skillProjectile);
    }

    public bool UseTestSkill()
    {
        return UseSkillSlash();
    }

    public void TestSkillLevelUp()
    {
        if (skillSlash == null)
            return;

        if (skillLevelUpProvider == null)
            skillLevelUpProvider = new CharacterSkillLevelUpProvider();

        CharacterSkill runtime = skillSlash.RuntimeSkill;

        if (runtime == null)
            return;

        runtime.IncreaseLevel();
        runtime.SetMpCost(skillLevelUpProvider.GetMpCost(runtime.Level));
        runtime.SetCooldown(skillLevelUpProvider.GetCooldown(runtime.Level));

        Debug.Log($"{runtime.SkillName} 강화 | Lv.{runtime.Level} / MP {runtime.MpCost} / CD {runtime.Cooldown}");
    }

    private void ApplyLevelUpGrowth() // 실질적인 레벨 업 시 스탯 상승 적용
    {
        int currentLevel = Status.Level;
        long hpGorwth = characterLevelUpProvider.GetMaxHpGrowth(currentLevel);
        int attackGrowth = characterLevelUpProvider.GetAttackGrowth(currentLevel);
        long defenceGrowth = characterLevelUpProvider.GetDefenceGrowth(currentLevel);

        Status.IncreaseMaxHp(hpGorwth);
        Status.IncreaseAttack(attackGrowth);
        Status.IncreaseDefence(defenceGrowth);

        Debug.Log($"레벨업! | Lv.{Status.Level} | 최대체력 +{hpGorwth} | 공격력 +{attackGrowth} | 방어력 +{defenceGrowth}");
    }

    public bool UseSkillSlash()
    {
        return skillSlash != null && skillSlash.TryUse();
    }

    public bool UseSkillProjectile()
    {
        return skillProjectile != null && skillProjectile.TryUse();
    }

    public bool UseSkillAttackBuff()
    {
        return skillAttackBuff != null && skillAttackBuff.TryUse();
    }

    #region 장비 팝업 연결
    public event Action InventoryChanged
    {
        add
        {
            Inventory.ListChanged += value;
            Equipment.ListChanged += value;
        }

        remove
        {
            if (Inventory != null)
                Inventory.ListChanged -= value;

            if (Equipment != null)
                Equipment.ListChanged -= value;
        }
    }

    public event Action EquipmentChanged
    {
        add
        {
            Equipment.ListChanged += value;
        }

        remove
        {
            if (Equipment != null)
                Equipment.ListChanged -= value;
        }
    }

    public IReadOnlyList<CharacterInventoryEquipment> GetEquipmentInventory() // 팝업용 미착용 목록 복사본
    {
        var result = new List<CharacterInventoryEquipment>();

        if (Inventory == null)
            return result.AsReadOnly();

        foreach (CharacterInventoryEquipment item in Inventory.GetEquipmentSnapshot())
            if (!IsEquipmentEquipped(item.InstanceId))
                result.Add(item);

        return result.AsReadOnly();
    }

    public bool TryGetItemData(int itemId, out ItemData data) // 공용 원본 데이터 조회
    {
        data = default;

        return DataManager.instance != null && DataManager.instance.TryGetItemData(itemId, out data);
    }

    public void EquipItem(Guid instanceId) 
    { 
        TryEquipItem(instanceId); 
    }

    public bool TryEquipItem(Guid instanceId)
    {
        return Equipment != null && Equipment.GetComponent<CharacterInventory>() == Inventory && Equipment.TryEquipItem(instanceId);
    }

    public bool UnequipItem(CharacterEquipmentSlot slot)// 장착칸 클릭 시 호출
    {
        return UnequipEquipment(slot);
    }
    #endregion

    // 장비착용 관련 호출
    public bool EquipEquipment(Guid instanceId, CharacterEquipmentSlot slot)
    {
        return Equipment != null && Equipment.GetComponent<CharacterInventory>() == Inventory && Equipment.TryEquip(instanceId, slot);
    }

    public bool UnequipEquipment(CharacterEquipmentSlot slot)
    {
        return Equipment != null && Equipment.TryUnequip(slot);
    }

    public bool IsEquipmentEquipped(Guid instanceId)
    {
        return Equipment != null && Equipment.IsEquipped(instanceId);
    }

    public bool TryGetEquippedItem(CharacterEquipmentSlot slot, out CharacterInventoryEquipment item)
    {
        item = null;
        return Equipment != null && Equipment.TryGetEquippedItem(slot, out item);
    }

    public IReadOnlyDictionary<CharacterEquipmentSlot, Guid> GetEquipmentSlots()
    {
        return Equipment.GetSlotsSnapshot();
    }

    // 인벤토리 관련 호출용
    public bool AddEquipment(ItemStatus status, out Guid instanceId)
    {
        return Inventory.TryAddEquipment(status, out instanceId);
    }

    public bool RemoveEquipment(Guid InstanceId)
    {
        return Inventory.TryRemoveEquipment(InstanceId);
    }

    public bool TryGetEquipment(Guid instanceId, out CharacterInventoryEquipment equipment)
    {
        return Inventory.TryGetEquipment(instanceId, out equipment);
    }

    public int GetEquipmentCount(int itemId)
    {
        return Inventory.GetEquipmentCount(itemId);
    }

    public IReadOnlyList<CharacterInventoryEquipment> GetInventoryEquipment()
    {
        return Inventory.GetEquipmentSnapshot();
    }

    // 현재 착용 장비 전체의 합계 교체
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
        Status.SetEquipmentStats(
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

    // 강화/옵션 변경 후 착용 장비를 다시 조회 및 합산, 성공 여부 반환
    public bool RefreshEquipmentStats()
    {
        return Equipment != null && Equipment.RefreshEquipmentStats();
    }

    public void ClearEquipmentStats()
    {
        Status.ClearEquipmentStats();
    }

    public long Money { get; private set; }

    public void SetMoney(long amount) // 최신 보유 재화만 전달하는 용
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount));

        Money = amount;
    }

    // 전달용 이벤트 (현재값, 최대값)
    public event Action<long, long> HpChanged;
    public event Action<int, int> MpChanged;
    public event Action<int> LevelChanged;
    public event Action<long> MoneyChanged;
    public event Action<int> CombatPowerChanged;
    public event Action<string> JobNameChanged;

    // UI에서 현재값 조회용 프로퍼티
    public long CurrentHp => Status.CurrentHp;
    public long MaxHp => Status.MaxHp;
    public int CurrentMp => Status.CurrentMp;
    public int MaxMp => Status.MaxMp;
    public int Level => Status.Level;
    public int CombatPower => Status.Attack;

    public string JobName
    {
        get
        {
            {
                PlayerData job = CurrentJob;
                return job == null ? string.Empty : (job.displayName ?? string.Empty);
            }
        }
    }

    // 변경됐는지 확인용 이전 값 저장 변수
    private bool hasUiSnapshot; // 이전에 값을 한번이라도 기록했는지
    private bool publishingUiChanges; // 지금 이벤트 발행하는 중인지
    private long previousCurrentHp;
    private long previousMaxHp;
    private int previousCurrentMp;
    private int previousMaxMp;
    private int previousLevel;
    private long previousMoney;
    private int previousCombatPower;
    private string previousJobName;

    private void OnEnable()
    {
        hasUiSnapshot = false; // 재활성화 후 첫 확인에서는 모든 값을 알림

        if (Equipment != null)
            Equipment.RequestEquipmentStatsRefresh();
    }

    private void LateUpdate()
    {
        if (Equipment != null)
            Equipment.RefreshEquipmentStatsIfNeeded();

        PublishUiChanges(false); // 매 프레임 마지막에 변경된 값만 알림
    }

    public void RefreshUiEvents()
    {
        PublishUiChanges(true); // 요청시 변경 여부와 관계없이 전체 알림
    }

    private void PublishUiChanges(bool force)
    {
        if (publishingUiChanges)
            return;

        publishingUiChanges = true; // 발행 시작 표시

        try
        {
            PublishUiChangesCore(force); // 실제 비교와 이벤트 호출
        }
        finally
        {
            publishingUiChanges = false; // 예외가 발생해도 발행 중 표시 해제
        }
    }

    private void PublishUiChangesCore(bool force)
    {
        long hp = CurrentHp;
        long maxHp = MaxHp;
        int mp = CurrentMp;
        int maxMp = MaxMp;
        int level = Level;
        long money = Money;
        int combatPower = CombatPower;
        string jobName = JobName;

        bool all = force || !hasUiSnapshot; // 강제 갱신 또는 최초 확인이면 전체 알림

        // 현재값과 이전값을 비교. HP/MP는 최대치 변경도 감지
        bool changedHp = all || hp != previousCurrentHp || maxHp != previousMaxHp;
        bool changedMp = all || mp != previousCurrentMp || maxMp != previousMaxMp;
        bool changedLevel = all || level != previousLevel;
        bool changedMoney = all || money != previousMoney;
        bool changedCombatPower = all || combatPower != previousCombatPower;
        bool changedJobName = all || jobName != previousJobName;

        // 다음 프레임에서 비교할 수 있도록 이번 값을 저장
        hasUiSnapshot = true;
        previousCurrentHp = hp;
        previousMaxHp = maxHp;
        previousCurrentMp = mp;
        previousMaxMp = maxMp;
        previousLevel = level;
        previousMoney = money;
        previousCombatPower = combatPower;
        previousJobName = jobName;

        // 알림이 필요한 항목만 발행. ?.Invoke는 구독자가 있을 때만 호출
        if (changedHp)
            HpChanged?.Invoke(hp, maxHp);

        if (changedMp)
            MpChanged?.Invoke(mp, maxMp);

        if (changedLevel)
            LevelChanged?.Invoke(level);

        if (changedMoney)
            MoneyChanged?.Invoke(money);

        if (changedCombatPower)
            CombatPowerChanged?.Invoke(combatPower);

        if (changedJobName)
            JobNameChanged?.Invoke(jobName);
    }

    [SerializeField] private Sprite characterPortrait; // 초상화용 이미지

    public Sprite GetCharacterPortrait()
    {
        return characterPortrait; // 미지정 상태에서는 null 반환
    }
}
