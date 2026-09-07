using UnityEngine;
using System.Collections;

public class CharacterSkillAttackBuff : CharacterSkillBase
{
    [SerializeField] private float attackIncreaseRate = 0.5f;
    [SerializeField] private float duration = 5f;

    private bool isBuffActive;

    protected override void Execute()
    {
        if (isBuffActive)
            return;

        StartCoroutine(AttackBuff());
    }

    private IEnumerator AttackBuff()
    {
        isBuffActive = true;

        long bonusAttack = (long)(characterFacade.Status.Attack * attackIncreaseRate);
        characterFacade.Status.IncreaseAttack(bonusAttack);

        Debug.Log($"공격력 {bonusAttack} 증가");

        yield return new WaitForSeconds(duration);

        characterFacade.Status.DecreaseAttack(bonusAttack);

        isBuffActive = false;

        Debug.Log("공격력 버프 시간 종료");
    }
}
