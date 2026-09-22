using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterSkillAreaVariation : CharacterSkillBase // 전방 범위 스킬 베이스
{
    [SerializeField] private LayerMask monsterLayer;
    [SerializeField] private LayerMask blockingLayer;
    [SerializeField] private float heightOffset = 0.6f;

    protected abstract Vector2 AreaSize { get; }
    protected abstract float ForwardDistance { get; }
    protected abstract float DamageMultiplier { get; }
    protected virtual int HitCount => 1;
    public override bool CanReplaceBasicAttack => false;

    protected override bool CanUse()
    {
        return characterControllers != null
            && characterControllers.CanRun
            && characterControllers.isActiveAndEnabled
            && isActiveAndEnabled
            && characterControllers.Status.CurrentHp > 0
            && monsterLayer.value != 0
            && AreaSize.x > 0
            && AreaSize.y > 0
            && DamageMultiplier > 0
            && !float.IsInfinity(DamageMultiplier)
            && !float.IsNaN(DamageMultiplier);
    }

    protected override void Execute()
    {
        var attack = characterControllers.CaptureDamageAttack();
        int facing = characterControllers.FacingDirection < 0 ? -1 : 1;
        Vector2 origin = (Vector2)characterControllers.transform.position + Vector2.up * heightOffset;
        Vector2 center = origin + Vector2.right * (facing * ForwardDistance);
        int strikes = Mathf.Clamp(HitCount, 1, 4);

        for (int strike = 0; strike < strikes; strike++)
        {
            if (!CanUse())
                return;
            
            Collider2D[] hits = Physics2D.OverlapBoxAll(center, AreaSize, 0f, monsterLayer);
            var damaged = new HashSet<IDamageable>();
            foreach (Collider2D hit in hits)
            {
                if (!CanUse())
                    return;

                if (hit == null || !hit.enabled || !hit.gameObject.activeInHierarchy || hit.transform.IsChildOf(characterControllers.transform))
                    continue;

                IDamageable target = hit.GetComponentInParent<IDamageable>();

                if (target == null || (target is Object obj && obj == null))
                    continue;

                if (target is Component component && !component.gameObject.activeInHierarchy)
                    continue;

                Vector2 point = hit.ClosestPoint(origin);

                if (blockingLayer.value != 0 && Physics2D.Linecast(origin, point, blockingLayer).collider != null)
                    continue;

                if (!damaged.Add(target))
                    continue;

                CharacterDamageResult result = CharacterDamageUtility.Apply(attack, DamageMultiplier, target);

                if (characterControllers != null)
                    characterControllers.ReportDamageResult(target, result);
            }
        }
    }
}
