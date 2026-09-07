using UnityEngine;

public class CharacterSkillProjectile : CharacterSkillBase
{
    [SerializeField] private TestSkillProjectile projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private float damageMultiplier = 1.5f;

    protected override void Execute()
    {
        if (projectilePrefab == null || firePoint == null)
            return;

        long damage = (long)(characterFacade.Status.Attack * damageMultiplier);
        Vector2 direction = transform.localScale.x >= 0f ? Vector2.right : Vector2.left;

        // 테스트 용 추후 ObjectPoolManager 사용
        TestSkillProjectile projectile = Instantiate(projectilePrefab,firePoint.position,Quaternion.identity);
        projectile.Initialize(damage, projectileSpeed, direction);

        Debug.Log($"{skillName} 사용 | {damage} 피해");
    }
}
