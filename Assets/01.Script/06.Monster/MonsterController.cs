using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
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

    [Header("Hover")]
    [SerializeField] bool enableHover;
    [SerializeField, Min(0f)] float hoverHeight = 0.35f;
    [SerializeField, Min(0f)] float hoverAmplitude = 0.2f;
    [SerializeField, Min(0.1f)] float hoverPeriod = 2.5f;
    [SerializeField, Min(0f)] float hoverMaxSpeed = 1f;
    float _hoverCenterY;
    float _hoverPhase;

    readonly MonsterStatus _status = new MonsterStatus();
    readonly MonsterAI _ai = new MonsterAI();
    readonly MonsterStageStatusProvider _stageProvider = MonsterStageStatusProvider.Default;

    Action _returnToPool;
    SpriteRenderer _spriteRenderer;
    Animator _animator;
    Navi2DAgent _naviAgent;
    Rigidbody2D _body;
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
        _body = GetComponent<Rigidbody2D>();
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
        ResetMovement();
        _hoverCenterY = _body.position.y + hoverHeight;
        _hoverPhase = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
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
        ResetMovement();
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

    void FixedUpdate()
    {
        if (!_spawned || IsDead)
        {
            StopMovement();
            return;
        }

        _ai.Tick(Time.fixedDeltaTime);
        if (_ai.State != MonsterState.Trace)
            StopMovement();

        if (_spawned && !IsDead && enableHover && _naviAgent == null)
            UpdateHover();
    }

    private void UpdateHover()
    {
        float angularSpeed = Mathf.PI * 2f / Mathf.Max(0.1f, hoverPeriod);
        _hoverPhase = Mathf.Repeat(_hoverPhase + angularSpeed * Time.fixedDeltaTime, Mathf.PI * 2f);
        float desiredY = _hoverCenterY + Mathf.Sin(_hoverPhase) * hoverAmplitude;
        float desiredVelocity = Mathf.Cos(_hoverPhase) * hoverAmplitude * angularSpeed;
        desiredVelocity += (desiredY - _body.position.y) * 4f;
        float verticalSpeedLimit = _ai.State == MonsterState.Trace
            ? Mathf.Max(hoverMaxSpeed, _status.MoveSpeed)
            : hoverMaxSpeed;
        desiredVelocity = Mathf.Clamp(desiredVelocity, -verticalSpeedLimit, verticalSpeedLimit);

        // Balance weak gravity while alive. Move the rigidbody, never its transform,
        // so floors and ceilings still block the motion without accumulated force.
        desiredVelocity -= Physics2D.gravity.y * _body.gravityScale * Time.fixedDeltaTime;
        _body.linearVelocity = new Vector2(_body.linearVelocity.x, desiredVelocity);
    }

    private void StopMovement()
    {
        if (_naviAgent != null)
            _naviAgent.StopMovement();
        if (_body != null)
            _body.linearVelocity = new Vector2(0f, _body.linearVelocity.y);
    }

    private void ResetMovement()
    {
        StopMovement();
        if (_body != null)
        {
            _body.linearVelocity = Vector2.zero;
            _body.angularVelocity = 0f;
        }
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
        // StageManager가 캐릭터 참조를 들고 있으면 우선 사용한다.
        StageController stage = FindFirstObjectByType<StageController>();
        if (stage != null && stage.Character != null)
        {
            return stage.Character.transform;
        }

        return FindTargetByTag();
    }

    Transform FindTargetByTag()
    {
        if (string.IsNullOrEmpty(targetTag))
        {
            return null;
        }

        try
        {
            GameObject target = GameObject.FindGameObjectWithTag(targetTag);
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

        // Hovering monsters chase the target's height as well as its X position.
        // UpdateHover applies vertical velocity; physics still blocks solid terrain.
        if (enableHover)
            _hoverCenterY = worldPosition.y;
        float deltaX = worldPosition.x - _body.position.x;
        float horizontalSpeed = Mathf.Clamp(deltaX / Time.fixedDeltaTime, -speed, speed);
        _body.linearVelocity = new Vector2(horizontalSpeed, _body.linearVelocity.y);
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
