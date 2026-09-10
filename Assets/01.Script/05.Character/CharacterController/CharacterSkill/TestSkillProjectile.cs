using UnityEngine;

public class TestSkillProjectile : MonoBehaviour
{
    [SerializeField, Min(0.01f)] private float hitRadius = 0.15f;
    private int damage;
    private float speed;
    private Vector2 direction;
    private float remainingDistance;
    private LayerMask monsterLayer;
    private LayerMask blockingLayer;
    private Transform owner;
    private bool initialized;
    private bool finished;

    public void Initialize(int damage, float speed, Vector2 direction, float maxDistance, LayerMask monsterLayer, LayerMask blockingLayer, Transform owner)
    {
        this.damage = damage;
        this.speed = Mathf.Max(0.01f, speed);
        this.direction = direction.sqrMagnitude > 0f ? direction.normalized : Vector2.right;
        remainingDistance = Mathf.Max(0.01f, maxDistance);
        this.monsterLayer = monsterLayer;
        this.blockingLayer = blockingLayer;
        this.owner = owner;
        finished = false;
        initialized = true;
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(this.direction.y, this.direction.x) * Mathf.Rad2Deg);
    }

    private void Update()
    {
        if (!initialized || finished)
            return;

        float step = Mathf.Min(speed * Time.deltaTime, remainingDistance);

        if (step <= 0f)
            return;

        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, Mathf.Max(0.01f, hitRadius), direction, step, monsterLayer.value | blockingLayer.value);

        foreach (RaycastHit2D hit in hits)
        {
            Collider2D other = hit.collider;

            if (other == null || (owner != null && other.transform.IsChildOf(owner)))
                continue;

            int layer = 1 << other.gameObject.layer;

            if ((blockingLayer.value & layer) != 0)
            {
                Finish();
                return;
            }
            IDamageable target = other.GetComponentInParent<IDamageable>();

            if ((monsterLayer.value & layer) != 0 && target != null)
            {
                finished = true;
                target.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }
        }

        transform.position += (Vector3)(direction * step);
        remainingDistance -= step;

        if (remainingDistance <= 0f)
            Finish();
    }

    private void Finish()
    {
        finished = true;
        Destroy(gameObject); 
    }
}
