using System.Collections;
using UnityEngine;

public class CharacterSkillAttackBuff : CharacterSkillBase
{
    [SerializeField, Min(0f)] private float attackIncreaseRate = 0.5f;
    [SerializeField, Min(0.01f)] private float duration = 5f;
    private bool isBuffActive;
    private int appliedBonus;
    private CharacterStatus buffTarget;
    private Coroutine routine;

    protected override bool CanUse() 
    {
        return !isBuffActive;
    }
    protected override void Execute() 
    { 
        routine = StartCoroutine(AttackBuff()); 
    }

    private IEnumerator AttackBuff()
    {
        isBuffActive = true;
        buffTarget = characterControllers.Status;
        appliedBonus = (int)(buffTarget.Attack * Mathf.Max(0f, attackIncreaseRate));
        buffTarget.IncreaseAttack(appliedBonus);
        yield return new WaitForSeconds(Mathf.Max(0.01f, duration));
        RemoveBonus();
        routine = null;
    }

    private void OnDisable()
    {
        if (routine != null)
            StopCoroutine(routine);

        routine = null;
        RemoveBonus();
    }

    private void RemoveBonus()
    {
        if (isBuffActive && buffTarget != null)
            buffTarget.DecreaseAttack(appliedBonus);

        appliedBonus = 0;
        buffTarget = null;
        isBuffActive = false;
    }
}