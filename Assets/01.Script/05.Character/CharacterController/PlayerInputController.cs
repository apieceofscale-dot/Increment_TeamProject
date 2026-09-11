using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    [SerializeField] private CharacterControllers characterControllers;

    private void Awake()
    {
        if (characterControllers == null)
            characterControllers = GetComponent<CharacterControllers>();
    }

    private void Update()
    {
        if (Keyboard.current == null || characterControllers == null)
            return;

        HandleMovement();
        HandleJump();
        UseTestSkill();
        HandleAttack();

        TestGainExp();
        TestTakeDamage();
        TestRecoverHp();
        TestUseMp();
        TestRecoverMp();
    }

    private void HandleMovement()
    {
        float moveInput = 0f;

        if (Keyboard.current.aKey.isPressed)
            moveInput -= 1f;
        if (Keyboard.current.dKey.isPressed)
            moveInput += 1f;

        characterControllers.SetMoveInput(moveInput);
    }

    private void HandleJump()
    {
        if (!Keyboard.current.spaceKey.wasPressedThisFrame)
            return;

        characterControllers.Jump();
    }

    private void HandleAttack()
    {
        if (!Keyboard.current.jKey.wasPressedThisFrame)
            return;

        bool isAttacked = characterControllers.TryAttack();

        if (isAttacked)
            Debug.Log("공격");
    }

    // 테스트용 임시 메서드들
    private void TestGainExp()
    {
        if (!Keyboard.current.digit1Key.wasPressedThisFrame)
            return;

        characterControllers.GainExp(100);

        Debug.Log($"Lv.{characterControllers.Status.Level} / Exp : {characterControllers.Status.Exp} / Attack : {characterControllers.Status.Attack}");
    }

    private void TestTakeDamage()
    {
        if (!Keyboard.current.digit2Key.wasPressedThisFrame)
            return;

        characterControllers.Status.TakeDamage(10);

        Debug.Log($"피해 받음 | HP : {characterControllers.Status.CurrentHp} / {characterControllers.Status.MaxHp}");
    }

    private void TestRecoverHp()
    {
        if (!Keyboard.current.digit3Key.wasPressedThisFrame)
            return;

        characterControllers.Status.RecoverHp(10);

        Debug.Log($"HP 회복 | HP : {characterControllers.Status.CurrentHp} / {characterControllers.Status.MaxHp}");
    }

    private void TestUseMp()
    {
        if (!Keyboard.current.digit4Key.wasPressedThisFrame)
            return;

        bool isUsed = characterControllers.Status.UseMp(10);

        if (isUsed)
        {
            Debug.Log($"MP 소모 | MP : {characterControllers.Status.CurrentMp} / {characterControllers.Status.MaxMp}");
        }
        else
        {
            Debug.Log($"MP 부족 | MP : {characterControllers.Status.CurrentMp} / {characterControllers.Status.MaxMp}");
        }
    }

    private void TestRecoverMp()
    {
        if (!Keyboard.current.digit5Key.wasPressedThisFrame)
            return;

        characterControllers.Status.RecoverMp(20);

        Debug.Log($"MP 회복 | MP : {characterControllers.Status.CurrentMp} / {characterControllers.Status.MaxMp}");
    }

    private void UseTestSkill()
    {
        if (Keyboard.current.digit6Key.wasPressedThisFrame)
            characterControllers.UseSkillSlash();

        if (Keyboard.current.digit7Key.wasPressedThisFrame)
            characterControllers.UseSkillProjectile();

        if (Keyboard.current.digit8Key.wasPressedThisFrame)
            characterControllers.UseSkillAttackBuff();

        if (Keyboard.current.digit9Key.wasPressedThisFrame)
            characterControllers.ChangeNextJob();

        if (Keyboard.current.f1Key.wasPressedThisFrame)
        {
            characterControllers.RestoreBasicAttack();
            Debug.Log("기존 기본 공격 선택");
        }

        if (Keyboard.current.f2Key.wasPressedThisFrame)
            Debug.Log(characterControllers.SetSlashBasicAttack() ? "베기 대체 공격 선택" : "베기 연결 확인 필요");

        if (Keyboard.current.f3Key.wasPressedThisFrame)
            Debug.Log(characterControllers.SetProjectileBasicAttack() ? "투사체 대체 공격 선택" : "투사체 연결 확인 필요");

        if (Keyboard.current.f4Key.wasPressedThisFrame)
            characterControllers.TestSkillLevelUp();
    }

    private void OnDisable()
    {
        if (characterControllers != null)
            characterControllers.SetMoveInput(0f);
    }
}
