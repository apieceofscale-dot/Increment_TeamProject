using System.Collections.Generic;
using UnityEngine;

public class CharacterSkillSlash : CharacterSkillBase
{
    [SerializeField] private Transform attackPoint;
    [SerializeField, Min(0.01f)] private float attackRadius = 2f;
    [SerializeField, Min(0f)] private float damageMultiplier = 2f;
    [SerializeField] private LayerMask monsterLayer;
    public override bool CanReplaceBasicAttack => true;

    protected override bool CanUse()
    {
        return attackPoint != null;
    }

    protected override void Execute()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, Mathf.Max(0.01f, attackRadius), monsterLayer);
        HashSet<IDamageable> damaged = new HashSet<IDamageable>();

        foreach (Collider2D hit in hits)
        {
            if (hit == null || !hit.gameObject.activeInHierarchy || hit.transform.IsChildOf(characterControllers.transform))
                continue;

            IDamageable target = hit.GetComponentInParent<IDamageable>();

            if (target != null && damaged.Add(target))
                characterControllers.DealAttackDamage(target, damageMultiplier);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
            Gizmos.DrawWireSphere(attackPoint.position, Mathf.Max(0.01f, attackRadius));
    }
}
