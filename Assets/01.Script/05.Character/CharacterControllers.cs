using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterControllers : MonoBehaviour //기존 컴포넌트랑 이름 같아서 s붙임
{
    public CharacterStatus Status { get; private set; }
    private CharacterLevelUpProvider characterLevelUpProvider;
    private CharacterSkill testSkill;
    private CharacterSkillLevelUpProvider skillLevelUpProvider;

    // 이동 및 점프 관련
    private Rigidbody2D rigid;
    private float moveInput;

    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [SerializeField] Transform visualRoot;

    // 공격 관련
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private LayerMask monsterLayer;

    // 스킬 테스트용
    [SerializeField] private CharacterSkillSlash skillSlash;
    [SerializeField] private CharacterSkillProjectile skillProjectile;
    [SerializeField] private CharacterSkillAttackBuff skillAttackBuff;

    private float lastAttackTime;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();

        Status = new CharacterStatus();
        characterLevelUpProvider = new CharacterLevelUpProvider();
        skillLevelUpProvider = new CharacterSkillLevelUpProvider();

        testSkill = new CharacterSkill("테스트", 1, 10, 3f, true);
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
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer) != null;
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

        Vector3 scale = visualRoot.localScale;
        scale.x = Mathf.Abs(scale.x) * (moveInput > 0f ? 1f : -1f);

        visualRoot.localScale = scale;
    }

    public bool TryAttack()
    {
        float attackInterval = GetAttackInterval();

        if (Time.time < lastAttackTime + attackInterval)
            return false;

        lastAttackTime = Time.time;

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, monsterLayer);

        foreach (Collider2D hit in hits)
        {
            IDamageable damageable = hit.GetComponent<IDamageable>();

            if (damageable == null)
                continue;

            damageable.TakeDamage(Status.Attack);
        }

        return true;
    }

    private float GetAttackInterval()
    {
        const float baseAttackInterval = 1f;
        float attackSpeedMultiplier = 1f + Status.AttackSpeedRate;
        return baseAttackInterval / attackSpeedMultiplier;
    }

    public bool UseTestSkill() // 테스트용 임시 메서드
    {
        if (!testSkill.CanUse())
            return false;

        if (!Status.UseMp(testSkill.MpCost))
            return false;

        testSkill.Use();

        return true;
    }

    public void TestSkillLevelUp()
    {
        testSkill.IncreaseLevel();

        int currentLevel = testSkill.Level;
        int mpCost = skillLevelUpProvider.GetMpCost(currentLevel);
        float cooldown = skillLevelUpProvider.GetCooldown(currentLevel);

        testSkill.SetMpCost(mpCost);
        testSkill.SetCooldown(cooldown);

        Debug.Log($"{testSkill.SkillName} 강화 | Lv.{testSkill.Level} / Mp : {testSkill.MpCost} / Coodown : {testSkill.Cooldown}");
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
        return skillSlash.TryUse();
    }

    public bool UseSkillProjectile()
    {
        return skillProjectile.TryUse();
    }

    public bool UseSkillAttackBuff()
    {
        return skillAttackBuff.TryUse();
    }
}
