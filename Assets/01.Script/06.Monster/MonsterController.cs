using System;
using UnityEngine;

public class MonsterController : MonoBehaviour, IPoolable, IDamageable
{
    [SerializeField] int monsterId = (int)MonsterId.Slime;
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
    Animator _animator;
    Navi2DAgent _naviAgent;
    Color _baseSpriteColor = Color.white;
    bool _spawned;
    bool _deathNotified;
    MonsterData _runtimeData;

    public MonsterStatus Status => _status;
    public MonsterAI AI => _ai;
    public bool IsDead => _status.IsDead;
    public int DropTableId => dropItemId;

    void Awake()
    {
        _ai.Bind(this);
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _animator = GetComponentInChildren<Animator>();
        _naviAgent = GetComponent<Navi2DAgent>();
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

    public void Initialize(MonsterData data, int stage = 1)
    {
        if (data == null)
        {
            return;
        }

        _runtimeData = data;
        monsterId = data.id;
        stageIndex = Mathf.Max(1, stage);
        maxHp = data.maxHp;
        attackDamage = data.attackDamage;
        moveSpeed = data.moveSpeed;
        traceRange = data.traceRange;
        attackRange = data.attackRange;
        attackCooldown = data.attackCooldown;
        dropItemId = data.dropTableId > 0 ? data.dropTableId : data.id;
    }

    public void InitializePoolObj(Action returnAction)
    {
        _returnToPool = returnAction;
    }

    public void OnSpawn()
    {
        ApplyVisuals(_runtimeData);

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
        _runtimeData = null;
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
            MonsterId = dropItemId,
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

    public void TraceTowards(Vector3 worldPosition)
    {
        float speed = _status.MoveSpeed;

        if (_naviAgent != null)
        {
            _naviAgent.Trace(worldPosition, speed);
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            worldPosition,
            speed * Time.deltaTime);
    }

    public void PerformAttack(Transform target)
    {
        if (target == null)
        {
            return;
        }

        int damage = Mathf.Max(1, _status.AttackDamage);

        if (target.TryGetComponent<IDamageable>(out IDamageable damageable))
        {
            damageable.TakeDamage(damage);
            return;
        }

        // Unity에는 TryGetComponentInParent가 없다. GetComponentInParent를 사용한다.
        // 플레이어 콜라이더가 자식 오브젝트에 붙어 있을 수 있어 부모까지 탐색한다.
        CharacterFacade character = target.GetComponent<CharacterFacade>();
        if (character == null)
        {
            character = target.GetComponentInParent<CharacterFacade>();
        }

        if (character != null)
        {
            character.TakeDamage(damage);
        }
    }

    public void ReturnToPool()
    {
        OnDespawn();
        if (MonsterFactory.Instance != null)
        {
            MonsterFactory.Instance.Release(this);
            return;
        }

        if (_returnToPool != null)
        {
            _returnToPool.Invoke();
            return;
        }

        Destroy(gameObject);
    }

    void ApplyVisuals(MonsterData data)
    {
        if (data == null || _animator == null)
        {
            return;
        }

        if (data.animatorController != null)
        {
            _animator.runtimeAnimatorController = data.animatorController;
        }
    }
}
