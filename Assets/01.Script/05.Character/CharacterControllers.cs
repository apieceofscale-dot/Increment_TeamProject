using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterControllers : MonoBehaviour //기존 컴포넌트랑 이름 같아서 s붙임
{
    public CharacterStatus Status { get; private set; }
    private CharacterLevelUpProvider characterLevelUpProvider;
    private CharacterSkillLevelUpProvider skillLevelUpProvider;

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
        rigid = GetComponent<Rigidbody2D>();
        jobAdvancedment = GetComponent<CharacterJobAdvancedment>();
        if (visualRoot != null)
            FacingDirection = visualRoot.localScale.x < 0f ? -1 : 1;
        
        Status = new CharacterStatus();
        characterLevelUpProvider = new CharacterLevelUpProvider();
        skillLevelUpProvider = new CharacterSkillLevelUpProvider();
    }

    private void FixedUpdate()
    {
        Move();
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
        while (true) // 보유 경험치량이 다음 레벨 업 요구 경험치 보다 많으면 반복해서 레벨업함
        {
            long requiredExp = characterLevelUpProvider.GetRequiredExp(Status.Level);

            if (Status.Exp < requiredExp)
                break;

            Status.UseExp(requiredExp);
            Status.IncreaseLevel();

            ApplyLevelUpGrowth();
        }
    }

    public void SetMoveInput(float input)
    {
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
        if (!IsGrounded())
            return;

        rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, jumpForce);
    }

    private void UpdateDirection()
    {
        if (moveInput == 0f)
            return;

        FacingDirection = moveInput > 0f ? 1 : -1;

        if (visualRoot == null)
            return;

        Vector3 scale = visualRoot.localScale;
        scale.x = Mathf.Abs(scale.x) * (moveInput > 0f ? 1f : -1f);

        visualRoot.localScale = scale;
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
        if (replacement != null && (!replacement.CanReplaceBasicAttack || !replacement.BelongsTo(GetComponent<CharacterFacade>())))
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

        CharacterSkill runtime = skillSlash.RuntimeSkill;
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
        long defenseGrowth = characterLevelUpProvider.GetDefenseGrowth(currentLevel);

        Status.IncreaseMaxHp(hpGorwth);
        Status.IncreaseAttack(attackGrowth);
        Status.IncreaseDefense(defenseGrowth);

        Debug.Log($"레벨업! | Lv.{Status.Level} | 최대체력 +{hpGorwth} | 공격력 +{attackGrowth} | 방어력 +{defenseGrowth}");
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
}
