using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterControllers : MonoBehaviour //기존 컴포넌트랑 이름 같아서 s붙임
{
    public CharacterStatus Status { get; private set; }
    private CharacterLevelUpProvider characterLevelUpProvider;
    private CharacterSkill testSkill;
    private CharacterSkillLevelUpProvider skillLevelUpProvider;

    private Rigidbody2D rigid;
    private float moveInput;

    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

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
        if (groundCheck == null)
            return;

        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundCheckRadius
        );
    }

    public void Jump()
    {
        if (!IsGrounded())
            return;

        rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, jumpForce);
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
        long attackGrowth = characterLevelUpProvider.GetAttackGrowth(currentLevel);
        long defenseGrowth = characterLevelUpProvider.GetDefenseGrowth(currentLevel);

        Status.IncreaseMaxHp(hpGorwth);
        Status.IncreaseAttack(attackGrowth);
        Status.IncreaseDefense(defenseGrowth);

        Debug.Log($"레벨업! | Lv.{Status.Level} | 최대체력 +{hpGorwth} | 공격력 +{attackGrowth} | 방어력 +{defenseGrowth}");
    }
}
