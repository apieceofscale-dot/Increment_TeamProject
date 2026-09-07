using UnityEngine;

public class TestSkillProjectile : MonoBehaviour
{
    private long damage;
    private float speed;
    private Vector2 direction;

    public void Initialize(long damage, float speed, Vector2 direction)
    {
        this.damage = damage;
        this.speed = speed;
        this.direction = direction;
    }

    private void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        IDamageable damageable = other.GetComponent<IDamageable>();

        if (damageable == null)
            return;

        damageable.TakeDamage(damage);
        Destroy(gameObject);
    }
}
