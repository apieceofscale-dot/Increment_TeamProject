using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D), typeof(CharacterJobAdvancedment), typeof(CharacterInventory))]
[RequireComponent(typeof(CharacterEquipment))]
public class CharacterControllers : MonoBehaviour, IBootStrapper
{
    [SerializeField] private int bootOrder = 1000;
    [SerializeField] private int scenePlayerId = 1000;
    public int BootOrder => bootOrder;
    public bool IsInitialized {  get; private set; }
    public bool CanRun => !duplicateCharacter && IsInitialized && bootStrapper != null && bootStrapper.IsBootCompleted;
    private BootStrapper bootStrapper;
    private bool referencesInjected;
    private CharacterSkillBase[] ownedSkills;

    public void IBootStrapperInject(BootstrapContext context)
    {
        if (!ClaimPlayer())
            return;

        BindSceneBootstrap(FindSceneBootstrap(SceneManager.GetActiveScene()));

        if (referencesInjected)
            return;
        
        rigid = GetComponent<Rigidbody2D>();
        jobAdvancedment = GetComponent<CharacterJobAdvancedment>();

        if (characterInventory == null)
            characterInventory = GetComponent<CharacterInventory>();

        characterEquipment = GetComponent<CharacterEquipment>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>(true);

        autoFarming = GetComponent<CharacterAutoFarming>();

        if (rigid == null || jobAdvancedment == null || characterInventory == null || characterEquipment == null)
            throw new InvalidOperationException("캐릭터 필수 컴포넌트 확인");

        if (characterInventory.gameObject != gameObject)
            throw new InvalidOperationException("Inventory는 같은 캐릭터 오브젝트의 컴포넌트여야힘");

        characterInventory.Inject(characterEquipment);
        characterEquipment.Inject(this, characterInventory);
        EnsureSkillSlots(); // UI 구독 전에 채널 6개를 한 번만 생성한다.
        ownedSkills = GetComponentsInChildren<CharacterSkillBase>(true);

        foreach (var facade in GetComponentsInChildren<CharacterFacade>(true))
            if (facade.GetComponentInParent<CharacterControllers>() == this)
                facade.Inject(this);

        foreach (var input in GetComponentsInChildren<PlayerInputController>(true))
            if (input.GetComponentInParent<CharacterControllers>() == this)
                input.Inject(this);

        if (autoFarming != null)
            autoFarming.Inject(this);

        referencesInjected = true;
    }

    public void IBootStrapperInitialize()
    {
        if (duplicateCharacter)
            return;

        if (IsInitialized)
            return;

        if (DataManager.instance == null || !DataManager.instance.TryGetPlayerData(scenePlayerId, out PlayerData data) || data == null)
            throw new InvalidOperationException($"캐릭터 초기 데이터 없음: {scenePlayerId}. DataManager 초기화 순서를 확인하세요.");

        Initialize(data);
    }

    #region DDOL 및 씬 진입
    public static CharacterControllers Current { get; private set; }
    private bool duplicateCharacter;
    private bool scenePhysicsPaused;
    private bool previousSimulated;
    private int bootstrapSceneHandle = -1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetCurrent() 
    {
        Current = null;
    }

    private bool ClaimPlayer()
    {
        if (duplicateCharacter)
            return false;

        if (Current != null && Current != this)
        {
            duplicateCharacter = true;
            gameObject.SetActive(false);
            Destroy(gameObject);
            return false;
        }

        Current = this;
        return true;
    }

    private static BootStrapper FindSceneBootstrap(Scene scene)
    {
        BootStrapper selected = null;

        foreach (BootStrapper candidate in FindObjectsByType<BootStrapper>(FindObjectsSortMode.None))
        {
            if (candidate.gameObject.scene != scene)
                continue;

            if (selected != null)
                throw new InvalidOperationException("하나 이상의 부트스트래퍼 존재");

            selected = candidate;
        }

        if (selected != null)
            return selected;

        BootStrapper[] all = FindObjectsByType<BootStrapper>(FindObjectsSortMode.None);

        if (all.Length == 1)
            return all[0];

        throw new InvalidOperationException("지금 씬의 부트스트래퍼 지정 필요");
    }

    public void BindSceneBootstrap(BootStrapper source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        if (duplicateCharacter)
            return;

        bootStrapper = source;
        bootstrapSceneHandle = source.gameObject.scene.handle;
        hasUiSnapshot = false;
    }

    public void PrepareForSceneChange()
    {
        bootStrapper = null;
        bootstrapSceneHandle = -1;
        SuspendSceneMovement();

        if (autoFarming != null)
            autoFarming.TargetFilter = null;
    }

    private void SuspendSceneMovement()
    {
        moveInput = 0f;

        if (autoFarming != null)
            autoFarming.SetAutoFarming(false);

        if (rigid == null)
            return;

        if (!scenePhysicsPaused)
        {
            previousSimulated = rigid.simulated;
            scenePhysicsPaused = true;
        }

        rigid.linearVelocity = Vector2.zero;
        rigid.angularVelocity = 0f;
        rigid.simulated = false;
    }

    private void RestoreScenePhysics()
    {
        if (!scenePhysicsPaused || rigid == null)
            return;

        rigid.simulated = previousSimulated;
        scenePhysicsPaused = false;
    }

    public void MoveToScenePosition(Vector3 position)
    {
        if (!IsInitialized || duplicateCharacter)
            return;

        SuspendSceneMovement();
        transform.position = position;

        if (rigid != null)
            rigid.position = new Vector2(position.x, position.y);

        hasUiSnapshot = false;
    }

    private void HandleSceneUnloaded(Scene scene)
    {
        if (scene.handle == bootstrapSceneHandle)
            PrepareForSceneChange();
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode != LoadSceneMode.Single)
            return;

        PrepareForSceneChange();

        try 
        { 
            BindSceneBootstrap(FindSceneBootstrap(scene));
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, this);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneUnloaded -= HandleSceneUnloaded;

        if (Current == this)
            Current = null;
    }
    #endregion

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
        get 
        { 
            return skillCooldownEvents ?? throw new InvalidOperationException("스킬 채널 주입 전입니다."); 
        }
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
        if (!IsInitialized)
            throw new InvalidOperationException("캐릭터 초기화 전");

        if (!IsValidSkillSlot(slotIndex))
            throw new ArgumentOutOfRangeException(nameof(slotIndex));

        CharacterSkillBase entry = skillSlots[slotIndex];

        return new SkillSlotInfo(slotIndex, entry != null, entry != null ? entry.SkillIcon : null, entry != null ? entry.RuntimeSkill.SkillName : string.Empty);
    }

    public SkillCooldownInfo GetSkillCooldown(int slotIndex)
    {
        if (!IsInitialized)
            throw new InvalidOperationException("캐릭터 초기화 전");

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
        if (!CanRun)
            return false;

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
        if (!CanRun)
            return false;

        if (!IsValidSkillSlot(slotIndex) || skillSlots[slotIndex] == null)
            return false;

        skillSlots[slotIndex] = null;
        PublishSkillSlot(slotIndex);

        return true;
    }

    public bool UseSkill(int slotIndex)
    {
        if (!CanRun)
            return false;

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
    public CharacterStatus Status => status;

    [SerializeField] private CharacterInventory characterInventory;
    public CharacterInventory Inventory => characterInventory;

    private CharacterEquipment characterEquipment;
    public CharacterEquipment Equipment => characterEquipment;

    [SerializeField] private Animator animator;
    public Animator Animator => animator;

    private CharacterLevelUpProvider characterLevelUpProvider;
    public CharacterLevelUpProvider LevelupProvider => characterLevelUpProvider;
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
        if (duplicateCharacter)
            return;

        if (!referencesInjected)
            throw new InvalidOperationException("IBootStrapperInject를 먼저 호출");

        if (playerData == null)
            throw new ArgumentNullException(nameof(playerData));

        if (IsInitialized)
        {
            if (Data != playerData)
                throw new InvalidOperationException("이미 초기화한 캐릭터의 데이터를 교체할 수 없음");
            return;
        }

        status = new CharacterStatus();
        characterLevelUpProvider = new CharacterLevelUpProvider();
        skillLevelUpProvider = new CharacterSkillLevelUpProvider();
        Data = playerData;
        status.Initialize(playerData);
        statUpgrade = new CharacterStatUpgrade(status);

        if (!jobAdvancedment.TryChangeJob(playerData.id))
            throw new InvalidOperationException("CharacterJobAdvancedment의 플레이어 리스트에 초기 직업이 필요");

        foreach (CharacterSkillBase entry in ownedSkills)
            if (entry != null && entry.GetComponentInParent<CharacterControllers>() == this)
                entry.Initialize(this);

        ValidateInitialSkillSlots(); // 스킬의 소유자·RuntimeSkill 준비 후 검사

        if (visualRoot != null)
            FacingDirection = visualRoot.localScale.x < 0f ? -1 : 1;

        if (!RefreshEquipmentStats())
            throw new InvalidOperationException("초기 장비 스탯 적용에 실패");

        if (autoFarming != null)
            autoFarming.Initialize();

        hasUiSnapshot = false;
        IsInitialized = true; // 이 줄까지 모두 성공해야 입력과 전투 실행을 허용

        transform.SetParent(null, true);
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += HandleSceneLoaded;
        SceneManager.sceneUnloaded += HandleSceneUnloaded;
    }

    private CharacterJobAdvancedment GetJobController() => jobAdvancedment;

    public bool ChangeJob(int id)
    {
        if (!CanRun)
            return false;

        return GetJobController().TryChangeJob(id);
    }

    public bool ChangeNextJob()
    {
        if (!CanRun)
            return false;

        return GetJobController().TryChangeNextJob();
    }

    private void Update()
    {
        if (!CanRun)
            SuspendSceneMovement();
        else
            RestoreScenePhysics();

        if (!CanRun)
            return;

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
        if (!CanRun)
            return;

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
        if (!CanRun)
            return;

        if (amount <= 0)
            return;

        Status.AddExp(amount);
        CheckLevelUp();
    }

    private void CheckLevelUp() // 레벨 수치 상승
    {
        if (!IsInitialized || growthChanging)
            return;

        bool changed = false;
        growthChanging = true;

        try
        {
            CharacterDamageMainStat mainStat = DamageMainStat;

            while (statUpgrade.CanRewardLevelUp && characterLevelUpProvider.TryLevelUp(Status, mainStat))
            {
                statUpgrade.RewardLevelUp();
                changed = true;
            }
            
            if (changed)
                NotifyStatUpgradeChanged();
        }
        finally 
        { 
            growthChanging = false; 
        }
    }

    public void SetMoveInput(float input)
    {
        if (!CanRun)
            return;

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
        if (!CanRun)
            return;

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
    private CharacterAutoFarming AutoFarming => autoFarming;
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

    #region 공통 피해 계산
    [System.Serializable]
    public sealed class DamageMainStatRule
    {
        public int jobId;
        public CharacterDamageMainStat mainStat;
    }
    [SerializeField] private CharacterDamageMainStat defaultDamageMainStat = CharacterDamageMainStat.Strength;
    [SerializeField] private List<DamageMainStatRule> damageMainStatRules = new List<DamageMainStatRule>();

    public CharacterDamageMainStat DamageMainStat
    {
        get
        {
            CharacterDamageMainStat selected = defaultDamageMainStat;
            PlayerData job = jobAdvancedment != null ? jobAdvancedment.CurrentJob : null;
            bool found = false;

            if (job != null && damageMainStatRules != null)
            {
                foreach (DamageMainStatRule rule in damageMainStatRules)
                {
                    if (rule == null || rule.jobId != job.id)
                        continue;

                    if (found)
                        throw new InvalidOperationException("피해 주 스탯 설정에 중복 직업 ID");

                    selected = rule.mainStat;
                    found = true;
                }
            }

            return selected;
        }
    }

    public CharacterDamageAttackData CaptureDamageAttack()
    {
        if (!IsInitialized)
            throw new InvalidOperationException("캐릭터 초기화 후 피해 계산");

        return CharacterDamageUtility.Capture(Status, DamageMainStat);
    }

    public event Action<IDamageable, CharacterDamageResult> DamageResolved;

    internal CharacterDamageResult DealAttackDamage(IDamageable target, float skillMultiplier = 1f)
    {
        if (!CanRun)
            return default;

        CharacterDamageResult result = CharacterDamageUtility.Apply(CaptureDamageAttack(), skillMultiplier, target);
        ReportDamageResult(target, result);
        return result;
    }

    internal void ReportDamageResult(IDamageable target, CharacterDamageResult result)
    {
        var handlers = DamageResolved;

        if (handlers == null)
            return;

        foreach (Delegate handler in handlers.GetInvocationList())
        {
            try 
            {
                ((Action<IDamageable, CharacterDamageResult>)handler)(target, result);
            }
            catch (Exception exception) 
            {
                Debug.LogException(exception, this); 
            }
        }
    }
    #endregion

    public bool TryAttack()
    {
        if (!CanRun)
            return false;

        if (Status == null || Status.CurrentHp <= 0 || Time.time < attackReadyTime)
            return false;

        if (basicAttackReplacement != null)
        {
            if (!basicAttackReplacement.TryUse())
                return false;

            attackReadyTime = Time.time + basicAttackReplacement.GetUseInterval();

            if (Animator != null)
                Animator.SetTrigger("attack");

            return true;
        }

        if (attackPoint == null)
            return false;

        attackReadyTime = Time.time + GetAttackInterval();

        if (Animator != null)
            Animator.SetTrigger("attack");

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, monsterLayer);
        HashSet<IDamageable> damaged = new HashSet<IDamageable>();

        foreach (Collider2D hit in hits)
        {
            if (hit == null || !hit.gameObject.activeInHierarchy || hit.transform.IsChildOf(transform))
                continue;

            IDamageable target = hit.GetComponentInParent<IDamageable>();

            if (target != null && damaged.Add(target))
                DealAttackDamage(target);
        }
        return true;
    }

    private float GetAttackInterval()
    {
        return 1f / (1f + Mathf.Clamp(Status.AttackSpeedRate, 0f, 1.5f));
    }

    public bool SetBasicAttackReplacement(CharacterSkillBase replacement)
    {
        if (!CanRun)
            return false;

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
        if (!CanRun)
            return;

        if (skillSlash == null)
            return;

        CharacterSkill runtime = skillSlash.RuntimeSkill;

        if (runtime == null)
            return;

        runtime.IncreaseLevel();
        runtime.SetMpCost(skillLevelUpProvider.GetMpCost(runtime.Level));
        runtime.SetCooldown(skillLevelUpProvider.GetCooldown(runtime.Level));

        Debug.Log($"{runtime.SkillName} 강화 | Lv.{runtime.Level} / MP {runtime.MpCost} / CD {runtime.Cooldown}");
    }

    #region 레벨 성장 및 능력치 포인트
    private CharacterStatUpgrade statUpgrade;
    public CharacterStatUpgrade StatUpgrade => statUpgrade;
    private bool growthChanging;
    public event Action StatUpgradeChanged;

    public CharacterStatUpgradeInfo GetStatUpgradeInfo()
    {
        if (!IsInitialized)
            throw new InvalidOperationException("캐릭터 초기화 후 조회");

        return statUpgrade.GetInfo();
    }

    public long CurrentExp => IsInitialized ? Status.Exp : 0;
    public int MainStatValue
    {
        get
        {
            if (!IsInitialized)
                return 0;

            switch (DamageMainStat)
            {
                case CharacterDamageMainStat.Strength:
                    return Status.Strength;
                case CharacterDamageMainStat.Dexterity:
                    return Status.Dexterity;
                case CharacterDamageMainStat.Intelligence:
                    return Status.Intelligence;
                case CharacterDamageMainStat.Luck:
                    return Status.Luck;
                default:
                    return 0;
            }
        }
    }

    public CharacterStatUpgradeOption GetStatUpgradeOption(CharacterStatUpgradeType type)
    {
        if (!IsInitialized)
            return new CharacterStatUpgradeOption(type, 0, 0, 0, 0, false);

        return statUpgrade.GetOption(type, DamageMainStat, CanRun);
    }

    public long RequiredExp => IsInitialized ? characterLevelUpProvider.GetRequiredExp(Status.Level) : 0;

    public bool TryUpgradeStat(CharacterStatUpgradeType type)
    {
        if (!CanRun || growthChanging)
            return false;

        growthChanging = true;

        try
        {
            if (!statUpgrade.TryUpgrade(type, DamageMainStat))
                return false;

            NotifyStatUpgradeChanged();
            return true;
        }
        finally 
        { 
            growthChanging = false;
        }
    }

    private void NotifyStatUpgradeChanged()
    {
        Action handlers = StatUpgradeChanged;

        if (handlers == null)
            return;

        foreach (Delegate handler in handlers.GetInvocationList())
        {
            try 
            { 
                ((Action)handler)();
            }
            catch (Exception exception) 
            { 
                Debug.LogException(exception, this); 
            }
        }
    }
    #endregion

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
    private long previousExp;
    private long previousRequiredExp;
    private Sprite previousPortrait;
    public event Action<long, long> ExpChanged;
    public event Action<Sprite> PortraitChanged;
    private long previousMoney;
    private int previousCombatPower;
    private string previousJobName;

    private void OnEnable()
    {
        hasUiSnapshot = false; // 재활성화 후 첫 확인에서는 모든 값을 알림

        if (IsInitialized && Equipment != null)
            Equipment.RequestEquipmentStatsRefresh();
    }

    private void LateUpdate()
    {
        if (!CanRun)
            return;

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
        if (!CanRun || publishingUiChanges)
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
        long exp = CurrentExp;
        long requiredExp = RequiredExp;
        Sprite portrait = GetCharacterPortrait();
        long money = Money;
        int combatPower = CombatPower;
        string jobName = JobName;

        bool all = force || !hasUiSnapshot; // 강제 갱신 또는 최초 확인이면 전체 알림

        // 현재값과 이전값을 비교. HP/MP는 최대치 변경도 감지
        bool changedHp = all || hp != previousCurrentHp || maxHp != previousMaxHp;
        bool changedMp = all || mp != previousCurrentMp || maxMp != previousMaxMp;
        bool changedLevel = all || level != previousLevel;
        bool changedExp = changedLevel || exp != previousExp || requiredExp != previousRequiredExp;
        bool changedPortrait = all || portrait != previousPortrait;
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
        previousExp = exp;
        previousRequiredExp = requiredExp;
        previousPortrait = portrait;
        previousMoney = money;
        previousCombatPower = combatPower;
        previousJobName = jobName;

        // 알림이 필요한 항목만 발행. ?.Invoke는 구독자가 있을 때만 호출
        if (changedExp)
            NotifyExpChanged(exp, requiredExp);

        if (changedPortrait)
            NotifyPortraitChanged(portrait);

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
        PlayerData job = jobAdvancedment != null ? jobAdvancedment.CurrentJob : null;

        if (job != null && job.portrait != null)
            return job.portrait;

        if (Data != null && Data.portrait != null)
            return Data.portrait;

        return characterPortrait;
    }

    private void NotifyExpChanged(long current, long required)
    {
        var handlers = ExpChanged;

        if (handlers == null)
            return;

        foreach (Delegate handler in handlers.GetInvocationList())
        {
            try 
            {
                ((Action<long, long>)handler)(current, required); 
            }
            catch (Exception exception) 
            {
                Debug.LogException(exception, this); 
            }
        }
    }

    private void NotifyPortraitChanged(Sprite portrait)
    {
        var handlers = PortraitChanged;

        if (handlers == null)
            return;

        foreach (Delegate handler in handlers.GetInvocationList())
        {
            try 
            { 
                ((Action<Sprite>)handler)(portrait);
            }
            catch (Exception exception) 
            {
                Debug.LogException(exception, this); 
            }
        }
    }
}
