using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterControllers : MonoBehaviour //기존 컴포넌트랑 이름 같아서 s붙임
{
    public CharacterStatus Status { get; private set; }
    private CharacterLevelUpProvider characterLevelUpProvider;

    private void Awake()
    {
        Status = new CharacterStatus();
        characterLevelUpProvider = new CharacterLevelUpProvider();
    }

    private void Update()
    {
        if (Keyboard.current.spaceKey.isPressed)
        {
            GainExp(100);

            Debug.Log(
                $"Lv.{Status.Level} / " +
                $"Exp : {Status.Exp} / " +
                $"Attack : {Status.Attack}"
            );
        }
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

    private void ApplyLevelUpGrowth() // 실질적인 레벨 업 시 스탯 상승 적용
    {
        int currentLevel = Status.Level;
        long hpGorwth = characterLevelUpProvider.GetMaxHpGrowth(currentLevel);
        long attackGrowth = characterLevelUpProvider.GetAttackGrowth(currentLevel);
        long defenseGrowth = characterLevelUpProvider.GetDefenseGrowth(currentLevel);

        Status.IncreaseMaxHp(hpGorwth);
        Status.IncreaseAttack(attackGrowth);
        Status.IncreaseDefense(defenseGrowth);

        Debug.Log($"레벨업! Lv.{Status.Level} | 최대체력 +{hpGorwth} | 공격력 +{attackGrowth} | 방어력 +{defenseGrowth}");
    }
}
