using UnityEngine;

public class CharacterSkillSlash : CharacterSkillBase
{
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRadius = 2f;
    [SerializeField] private float damageMultiplier = 2f;
    [SerializeField] private LayerMask monsterLayer;

    protected override void Execute()
    {
        if (attackPoint == null)
            return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, monsterLayer);

        int damage = (int)(characterFacade.Status.Attack * damageMultiplier);

        foreach (Collider2D hit in hits)
        {
            IDamageable damageable = hit.GetComponent<IDamageable>();

            if (damageable == null)
                continue;

            damageable.TakeDamage(damage);
        }

        Debug.Log($"{skillName} 사용 | {damage} 피해");
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.DrawWireSphere(attackPoint.position,attackRadius);
    }
}
