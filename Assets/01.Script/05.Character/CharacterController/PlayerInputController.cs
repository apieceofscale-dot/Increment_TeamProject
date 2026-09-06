using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    [SerializeField] private CharacterFacade characterFacade;

    private void Awake()
    {
        if(characterFacade == null)
            characterFacade = GetComponent<CharacterFacade>();
    }

    private void Update()
    {
        TestGainExp();
        TestTakeDamage();
        TestRecoverHp();
        TestUseMp();
        TestRecoverMp();
        UseTestSkill();
        TestSkillLevelUp();
    }

    // 테스트용 임시 메서드들
    private void TestGainExp()
    {
        if (!Keyboard.current.digit1Key.wasPressedThisFrame)
            return;

        characterFacade.GainExp(100);

        Debug.Log($"Lv.{characterFacade.Status.Level} / Exp : {characterFacade.Status.Exp} / Attack : {characterFacade.Status.Attack}");
    }

    private void TestTakeDamage()
    {
        if (!Keyboard.current.digit2Key.wasPressedThisFrame)
            return;

        characterFacade.TakeDamage(10);

        Debug.Log($"피해 받음 | HP : {characterFacade.Status.CurrentHp} / {characterFacade.Status.MaxHp}");
    }

    private void TestRecoverHp()
    {
        if (!Keyboard.current.digit3Key.wasPressedThisFrame)
            return;

        characterFacade.RecoverHp(10);

        Debug.Log($"HP 회복 | HP : {characterFacade.Status.CurrentHp} / {characterFacade.Status.MaxHp}");
    }

    private void TestUseMp()
    {
        if (!Keyboard.current.digit4Key.wasPressedThisFrame)
            return;

        bool isUsed = characterFacade.UseMp(10);

        if (isUsed)
        {
            Debug.Log($"MP 소모 | MP : {characterFacade.Status.CurrentMp} / {characterFacade.Status.MaxMp}");
        }
        else
        {
            Debug.Log($"MP 부족 | MP : {characterFacade.Status.CurrentMp} / {characterFacade.Status.MaxMp}");
        }
    }

    private void TestRecoverMp()
    {
        if (!Keyboard.current.digit5Key.wasPressedThisFrame)
            return;

        characterFacade.RecoverMp(20);

        Debug.Log($"MP 회복 | MP : {characterFacade.Status.CurrentMp} / {characterFacade.Status.MaxMp}");
    }

    private void UseTestSkill()
    {
        if (!Keyboard.current.digit6Key.wasPressedThisFrame)
            return;

        bool isUsed = characterFacade.UseTestSkill();

        if (isUsed)
            Debug.Log($"스킬 사용 | MP : {characterFacade.Status.CurrentMp} / {characterFacade.Status.MaxMp}");
        else
            Debug.Log("스킬 사용 실패");
    }

    private void TestSkillLevelUp()
    {
        if(!Keyboard.current.digit7Key.wasPressedThisFrame)
            return;

        characterFacade.TestSkillLevelUp();
    }
}
