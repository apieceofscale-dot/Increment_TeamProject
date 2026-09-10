using UnityEngine;

public class CharacterSkillProjectile : CharacterSkillBase
{
    [SerializeField] private TestSkillProjectile projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField, Min(0.01f)] private float projectileSpeed = 8f;
    [SerializeField, Min(0.01f)] private float maxDistance = 8f;
    [SerializeField, Min(0f)] private float damageMultiplier = 1.5f;
    [SerializeField] private LayerMask monsterLayer;
    [SerializeField] private LayerMask blockingLayer;
    public override bool CanReplaceBasicAttack => true;

    protected override bool CanUse()
    {
        return firePoint != null && projectilePrefab != null && projectilePrefab.gameObject.activeSelf && projectilePrefab.enabled;
    }

    protected override void Execute()
    {
        int damage = (int)((double)characterFacade.Status.Attack * Mathf.Max(0f, damageMultiplier));
        Vector2 direction = characterFacade.FacingDirection < 0 ? Vector2.left : Vector2.right;
        TestSkillProjectile projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        projectile.Initialize(damage, projectileSpeed, direction, maxDistance, monsterLayer, blockingLayer, characterFacade.transform);
    }
}