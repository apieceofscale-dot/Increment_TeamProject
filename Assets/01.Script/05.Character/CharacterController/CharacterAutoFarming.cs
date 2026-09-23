using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterControllers), typeof(Navi2DAgent), typeof(Collider2D))]
public class CharacterAutoFarming : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float searchRadius = 1000f;
    [SerializeField, Min(0.1f)] private float searchInterval = 0.5f;
    [SerializeField] private bool startAutomatically;

    public bool IsRunning { get; private set; }
    public string CurrentState { get; private set; } = "Idle";
    public Collider2D CurrentTarget => target;
    public Func<Collider2D, bool> TargetFilter { get; set; }

    private CharacterControllers controller;
    private Navi2DAgent agent;
    private Collider2D bodyCollider, target;
    private float nextSearch;
    private const float TargetSwitchDistance = 0.25f;

    private bool initialized;
    private bool automaticStartPending;

    internal void Inject(CharacterControllers owner)
    {
        controller = owner;
        agent = GetComponent<Navi2DAgent>();
        bodyCollider = GetComponent<Collider2D>();

        if (owner == null || agent == null || bodyCollider == null)
            throw new InvalidOperationException("필수 컴포넌트 확인");

        agent.enabled = false; // 부트 중 Navi2DAgent의 프레임 실행을 막는다.
    }

    internal void Initialize()
    {
        if (initialized)
            return;

        if (controller == null || agent == null || bodyCollider == null)
            throw new InvalidOperationException("자동사냥 Inject를 먼저 호출");

        IsRunning = false;
        CurrentState = "Idle";
        automaticStartPending = startAutomatically;
        initialized = true;
    }

    private void OnDisable()
    { 
        SetAutoFarming(false);
    }

    private void Update()
    {
        if (automaticStartPending && controller != null && controller.CanRun && controller.isActiveAndEnabled)
        {
            automaticStartPending = false;
            SetAutoFarming(true);
        }

        if (IsRunning && (controller == null || !controller.isActiveAndEnabled))
            SetAutoFarming(false);
    }

    public void SetAutoFarming(bool enabled)
    {
        if (enabled && (!initialized || controller == null || !controller.CanRun))
            return;

        automaticStartPending = false;

        if (!enabled)
        {
            if (IsRunning)
                PauseAgent();

            IsRunning = false;
            CurrentState = "Idle";
            target = null;
            return;
        }

        if (IsRunning || !isActiveAndEnabled || controller == null || !controller.isActiveAndEnabled || controller.Status.CurrentHp <= 0)
            return;

        if (agent == null || !controller.AutoFarmingConfigured || !bodyCollider.enabled || bodyCollider.isTrigger)
        {
            Debug.LogWarning("자동사냥: Agent, 본체 Collider, Ground Check, Attack Point, 레이어 설정을 확인", this);
            return;
        }

        IsRunning = true;
        CurrentState = "Search";
        target = null;
        nextSearch = 0f;
        PauseAgent();
    }

    internal void Tick()
    {
        if (!initialized || controller == null || !controller.CanRun || !IsRunning)
            return;

        if (controller.Status.CurrentHp <= 0 || agent == null)
        {
            SetAutoFarming(false);
            return;
        }

        if (!IsValidTarget(target))
        {
            target = null;
            CurrentState = "Search";
            PauseAgent();

        }

        // Reconsider nearby enemies even while the current target is valid.
        if (Time.time >= nextSearch)
            FindTarget();

        if (target == null)
            return;

        controller.FaceAutoFarmingTarget(target.bounds.center.x);
        
        if (controller.AutoFarmingGrounded && controller.CanAutoFarmingAttack(target))
        {
            CurrentState = "Attack";
            PauseAgent();

            if(controller.TryAttack())
                Debug.Log("공격");

            return;
        }

        CurrentState = "Move";

        if (!agent.enabled)
        {
            agent.enabled = true;
            return;
        }

        agent.Trace(new Vector2(target.bounds.center.x, target.bounds.min.y), controller.Status.MoveSpeed);
    }

    private void PauseAgent()
    {
        if (agent != null)
        {
            agent.StopMovement();
            agent.enabled = false;
        }

        if (controller != null)
            controller.StopAutoFarmingMovement();
    }

    private bool IsValidTarget(Collider2D candidate)
    {
        if (candidate == null || !candidate.enabled || !candidate.gameObject.activeInHierarchy || candidate.transform.IsChildOf(transform) || (controller.AutoFarmingMonsterMask.value & (1 << candidate.gameObject.layer)) == 0 || candidate.GetComponentInParent<IDamageable>() == null)
            return false;

        if (Vector2.Distance(candidate.bounds.center, bodyCollider.bounds.center) > searchRadius)
            return false;

        if (TargetFilter == null)
            return true;

        try 
        { 
            return TargetFilter(candidate); 
        }
        catch (Exception exception) 
        { 
            Debug.LogException(exception, this); 
            return false; 
        }
    }
    private void FindTarget()
    {
        nextSearch = Time.time + Mathf.Max(0.1f, searchInterval);
        float closest = float.PositiveInfinity;
        Collider2D nearest = null;

        foreach (Collider2D candidate in Physics2D.OverlapCircleAll(bodyCollider.bounds.center, searchRadius, controller.AutoFarmingMonsterMask))
        {
            if (!IsValidTarget(candidate))
                continue;

            float distance = (candidate.bounds.center - bodyCollider.bounds.center).sqrMagnitude;
            
            if (distance >= closest)
                continue;

            closest = distance;
            nearest = candidate;
        }

        if (nearest == null || nearest == target)
            return;

        // Keep the current target when distances are nearly equal.
        if (target != null)
        {
            float currentDistance = Vector2.Distance(target.bounds.center, bodyCollider.bounds.center);
            if (Mathf.Sqrt(closest) + TargetSwitchDistance >= currentDistance)
                return;
        }

        PauseAgent();
        target = nearest;
    }

    [ContextMenu("Auto Farming/시작")]
    private void StartFromInspector() 
    { 
        if (Application.isPlaying)
            SetAutoFarming(true);
    }

    [ContextMenu("Auto Farming/중지")]
    private void StopFromInspector() 
    {
        if (Application.isPlaying)
            SetAutoFarming(false); 
    }
}
