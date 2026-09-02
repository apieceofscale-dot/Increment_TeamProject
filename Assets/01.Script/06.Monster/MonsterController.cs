using System;
using UnityEngine;

public class MonsterController : MonoBehaviour, IPoolable, IDamageable
{
    [SerializeField] int monsterId = 1;
    [SerializeField] int stageIndex = 1;
    [SerializeField] int maxHp = 10;
    [SerializeField] int attackDamage = 1;
    [SerializeField] float moveSpeed = 1.5f;
    [SerializeField] float traceRange = 6f;
    [SerializeField] float attackRange = 1.4f;
    [SerializeField] float attackCooldown = 1f;
    [SerializeField] int dropItemId = 1;
    [SerializeField] float dropChance = 1f;
    [SerializeField] string targetTag = "Player";

    readonly MonsterStatus _status = new MonsterStatus();
    readonly MonsterAI _ai = new MonsterAI();
    readonly MonsterStageStatusProvider _stageProvider = MonsterStageStatusProvider.Default;

    Action _returnToPool;
    SpriteRenderer _spriteRenderer;
    Color _baseSpriteColor = Color.white;
    bool _spawned;
    bool _deathNotified;

    public MonsterStatus Status => _status;
    public MonsterAI AI => _ai;
    public bool IsDead => _status.IsDead;

    void Awake()
    {
        _ai.Bind(this);
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (_spriteRenderer != null)
        {
            _baseSpriteColor = _spriteRenderer.color;
        }
    }

    public void BindSpawn(int id, int stage)
    {
        monsterId = id;
        stageIndex = Mathf.Max(1, stage);
    }

    public void InitializePoolObj(Action returnAction)
    {
        _returnToPool = returnAction;
    }

    public void OnSpawn()
    {
        var palette = _stageProvider.GetPalette(stageIndex);
        _deathNotified = false;
        _spawned = true;
        _status.Reset(
            monsterId,
            maxHp,
            attackDamage,
            moveSpeed,
            traceRange,
            attackRange,
            attackCooldown,
            dropItemId,
            dropChance,
            palette);
        _stageProvider.ApplyStage(_status, stageIndex);
        _ai.Reset();
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = palette;
        }
    }

    public void OnDespawn()
    {
        _spawned = false;
        _deathNotified = false;
        _ai.Reset();
        _status.Clear();
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = _baseSpriteColor;
        }
    }

    void Update()
    {
        if (!_spawned || IsDead)
        {
            return;
        }

        _ai.Tick(Time.deltaTime);
    }

    public void TakeDamage(int amount)
    {
        if (!_spawned || IsDead)
        {
            return;
        }

        if (_status.ApplyDamage(amount))
        {
            Die();
        }
    }

    public void Die()
    {
        if (!_spawned || _deathNotified)
        {
            return;
        }

        _deathNotified = true;
        if (!_status.IsDead)
        {
            _status.ApplyDamage(_status.CurrentHp);
        }

        _ai.ForceDead();
        MonsterFacade.NotifyDied(new MonsterDiedInfo
        {
            MonsterId = monsterId,
            Position = transform.position,
            Source = this
        });
    }

    public Transform FindTarget()
    {
        if (string.IsNullOrEmpty(targetTag))
        {
            return null;
        }

        try
        {
            var target = GameObject.FindGameObjectWithTag(targetTag);
            return target != null ? target.transform : null;
        }
        catch (UnityException)
        {
            return null;
        }
    }

    public void MoveTowards(Vector3 worldPosition, float deltaTime)
    {
        transform.position = Vector3.MoveTowards(transform.position, worldPosition, _status.MoveSpeed * deltaTime);
    }

    public void PerformAttack(Transform target)
    {
        if (target == null)
        {
            return;
        }

        var damage = Mathf.Max(1, _status.AttackDamage);
        if (target.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(damage);
            return;
        }

        if (target.TryGetComponent<CharacterFacade>(out var characterFacade))
        {
            characterFacade.TakeDamage(damage);
            return;
        }

        if (target.TryGetComponent<CharacterControllers>(out var character))
        {
            character.Status.TakeDamage(damage);
        }
    }

    public void ReturnToPool()
    {
        OnDespawn();
        if (_returnToPool != null)
        {
            _returnToPool.Invoke();
            return;
        }

        Destroy(gameObject);
    }
}
