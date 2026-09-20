using UnityEngine;
using System.Collections.Generic;


//�׳��� ��Ʈ��Ʈ���۸� ���� pathfinder�� ã�´�.
//�ᱹ FSM���� ȣ���� Trace�Լ� �ϳ� ������ָ� ��.
public class Navi2DAgent : MonoBehaviour
{   
    private const int GroundLayerMask = (1<<30) | (1<<31);

    private float moveSpeed;
    private float maxAirHorizontalSpeed;
    private float jumpMaxHeight = 5f; 

    private float agentHeight;

    private float AirClearanceMargin { get { return col.bounds.extents.y * 0.2f; } }
    private float AirHorizontalClearance { get { return col.bounds.extents.x; } }
    private float AirBodyHeight { get { return col.bounds.size.y; } }

    private Navi2DPathFinder pathFinder;
    private Rigidbody2D rb;

    private Collider2D col;

    private List<Navi2DPathStep> path;
    private int currentPathIndex;

    private Vector2 targetPosition;    
    private Navi2DNode lastTargetNode;
    private Navi2DNode airTargetNode;

    private bool isTracing;    
    private bool repathPending;
    private bool isAirMoving;

    private bool hasLeftGround;
    private bool isApproachingDrop;
    private Vector2 dropDeparture;
    private float dropDirection;
    private float dropApproachTimeLeft;
    private bool isWaitingToSteerDrop;
    private float dropSteeringY;

    private readonly ContactPoint2D[] groundContacts = new ContactPoint2D[8];
    private ContactFilter2D groundContactFilter;

    float gravity;

    public Vector2 FootPosition
    {
        get
        {
            Bounds bounds = col.bounds;
            return new Vector2(bounds.center.x, bounds.min.y);
        }
    }

    private void Awake()//��Ʈ��Ʈ���ۿ��� ������ �ֵ�.
    {
        pathFinder = FindFirstObjectByType<Navi2DPathFinder>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        groundContactFilter = new ContactFilter2D();
        groundContactFilter.SetLayerMask(GroundLayerMask); //���� ������ ����ϰ��� �ϴ� ���̾� ����ũ ����
        groundContactFilter.useTriggers = false; //���� ������ Trigger���� �ݶ��̴� ����
    }

    private void Start()
    {
        gravity = Mathf.Abs(Physics2D.gravity.y * rb.gravityScale);
        agentHeight = AirBodyHeight;
    }

    private void FixedUpdate()
    {
        if (!isTracing) return;

        if (isApproachingDrop)
        {
            UpdateDropApproach();
            return;
        }

        if (isWaitingToSteerDrop)
        {
            UpdateDropSteering();
            return;
        }

        if(isAirMoving)
        {
            UpdateAirMove();
            return;
        }

        FollowPath();
    }



    public void Trace(Vector2 targetPos, float moveSpeed) //FSM���� ������ ȣ���ؾ� �� �Լ�.
                                         //�÷��̾ �̵����� �� repath�ϵ��� ����ȭ
    {       
        this.moveSpeed = moveSpeed;
        maxAirHorizontalSpeed = moveSpeed;
        targetPosition = targetPos;
        isTracing = true;

        Navi2DNode targetNode = pathFinder.FindGroundNodeBelow(targetPos);

        if (targetNode == null)
        {
            return;
        }
        bool isFirstTarget = lastTargetNode == null;
        bool targetChanged = !isFirstTarget && targetNode != lastTargetNode;

        if (isFirstTarget || targetChanged)
        {
            lastTargetNode = targetNode;
        }

        if (isAirMoving || !IsGrounded())
        {
            if (isFirstTarget || targetChanged || path == null)
            {
                repathPending = true;
            }

            return;
        }

        if (path == null || isFirstTarget || targetChanged || repathPending)
        {

            isAirMoving = false;
            hasLeftGround = false;
            airTargetNode = null;

            repathPending = false;
            RequestPath();

            return;
        }


    }
    private void FollowPath() //walk���� MoveToNode �� ��ȣ�� �ϴ� while������ ��ġ��.
    {
        if (path == null || path.Count == 0) return;

        if (currentPathIndex >= path.Count)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

            return;
        }

        Navi2DPathStep step = path[currentPathIndex];

        MoveToStep(step);
    }

    private void MoveToStep(Navi2DPathStep step)
    {        
        switch (step.moveType)
        {
            case Navi2DMoveType.walk:
                Walk(step.toNode); 
                break;

            case Navi2DMoveType.Drop:
                StartDrop(step);
                break;

            case Navi2DMoveType.Traverse:
                StartAirMove(step);
                break;

            case Navi2DMoveType.JumpUp:
                StartJumpUp(step);
                break;
        }



    }

    private void Walk(Navi2DNode targetNode) //���ڷ� ���� �Ѵٸ� �׳� ��带 �����ؼ� �ѹ��� ���� �ɷ� ����ȭ.
    {

        float distanceX = Mathf.Abs(targetNode.worldPos.x - FootPosition.x);

        if (distanceX < 0.01f)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            currentPathIndex++;

            if (currentPathIndex < path.Count)
            {
                MoveToStep(path[currentPathIndex]);
            }

            return;
        }

        float direction = Mathf.Sign(targetNode.worldPos.x - FootPosition.x);
        float speed = Mathf.Min(moveSpeed, distanceX / Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);

    }
 
    private void StartAirMove(Navi2DPathStep step)
    {
        if (isAirMoving)
            return;

        if (!IsGrounded())
            return;

        bool canAirMove = pathFinder.TryCalculatePlatformJumpVelocity(
            FootPosition,           
            step.toNode.worldPos,
            maxAirHorizontalSpeed,
            jumpMaxHeight,
            gravity,
            AirClearanceMargin,
            AirHorizontalClearance,
            AirBodyHeight,
            out Vector2 airVelocity);
               
      
        if (!canAirMove)
        {
            Debug.LogWarning($"AirMove �Ұ��� :{step.fromNode.gridPos} -> {step.toNode.gridPos}");

            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

            return;
        }


        rb.linearVelocity = airVelocity;
        airTargetNode = step.toNode;

        hasLeftGround = false;
        isAirMoving = true;
    }

    private void StartJumpUp(Navi2DPathStep step)
    {
        if (isAirMoving)
            return;

        if (!IsGrounded())
            return;

        bool canJumpUp =
            pathFinder.TryCalculateJumpUpVelocity(
                FootPosition,
                step.toNode.worldPos,
                maxAirHorizontalSpeed,
                jumpMaxHeight,
                gravity,
                AirClearanceMargin,
                AirHorizontalClearance,
                AirBodyHeight,
                out Vector2 jumpVelocity
            );


        if (!canJumpUp)
        {            
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        rb.linearVelocity = jumpVelocity;

        airTargetNode = step.toNode;

        hasLeftGround = false;
        isAirMoving = true;
    }

    private void StartDrop(Navi2DPathStep step)
    {
        if (isAirMoving || !IsGrounded()) return;
        if (moveSpeed <= 0f || !pathFinder.TryPlanDrop(step.fromNode, FootPosition,
            step.toNode.worldPos, maxAirHorizontalSpeed, gravity, AirHorizontalClearance,
            AirBodyHeight, out dropDeparture, out _))
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        dropDirection = Mathf.Sign(dropDeparture.x - FootPosition.x);
        dropApproachTimeLeft = Mathf.Abs(dropDeparture.x - FootPosition.x) / moveSpeed + 2f;
        airTargetNode = step.toNode;
        hasLeftGround = false;
        isAirMoving = true;
        isApproachingDrop = true;
        isWaitingToSteerDrop = false;
        UpdateDropApproach();
    }

    private void UpdateDropApproach()
    {
        bool grounded = IsGrounded();
        if (!grounded) hasLeftGround = true;
        dropApproachTimeLeft -= Time.fixedDeltaTime;

        // Keep walking until the entire collider has cleared the upper platform.
        float remaining = (dropDeparture.x - FootPosition.x) * dropDirection;
        if (dropApproachTimeLeft > 0f && (remaining > 0f || grounded) &&
            !(hasLeftGround && grounded) && FootPosition.y > airTargetNode.worldPos.y + 0.01f)
        {
            rb.linearVelocity = new Vector2(dropDirection * moveSpeed, rb.linearVelocity.y);
            return;
        }

        isApproachingDrop = false;
        if (!grounded && pathFinder.TryCalculateDropMotion(FootPosition,
            airTargetNode.worldPos, maxAirHorizontalSpeed, gravity, Mathf.Min(0f, rb.linearVelocity.y),
            AirHorizontalClearance, AirBodyHeight, out Vector2 velocity, out float delay))
        {
            // Preserve downward momentum: no upward impulse and no reset to vy = 0.
            if (delay > 0f)
            {
                dropSteeringY = FootPosition.y + rb.linearVelocity.y * delay - 0.5f * gravity * delay * delay;
                isWaitingToSteerDrop = true;
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }
            else rb.linearVelocity = velocity;
            return;
        }

        // A changed map or an unexpected contact invalidated the planned drop.
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        if (grounded)
        {
            isAirMoving = false;
            hasLeftGround = false;
            airTargetNode = null;
            RequestPath();
        }
        // Otherwise gravity finishes the fall; UpdateAirMove replans on landing.
    }

    private void UpdateDropSteering()
    {
        if (IsGrounded())
        {
            isWaitingToSteerDrop = false;
            UpdateAirMove();
            return;
        }
        if (FootPosition.y > dropSteeringY) return;

        isWaitingToSteerDrop = false;
        if (pathFinder.TryCalculateDropVelocity(FootPosition, airTargetNode.worldPos,
            maxAirHorizontalSpeed, gravity, Mathf.Min(0f, rb.linearVelocity.y),
            AirHorizontalClearance, AirBodyHeight, out Vector2 velocity))
            rb.linearVelocity = velocity;
        else rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    private void UpdateAirMove()
    {
        if(!hasLeftGround)
        {
            if(!IsGrounded())
            {
                hasLeftGround = true;
            }
            return;            
        }

        if (!IsGrounded()) return;
        
        isAirMoving =false;
        isApproachingDrop = false;
        isWaitingToSteerDrop = false;
        hasLeftGround = false;
        airTargetNode = null;
        repathPending = false;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        RequestPath();

    }


    private bool IsGrounded()
    {
        int contactCount = col.GetContacts(groundContactFilter, groundContacts);
        for(int i =  0; i < contactCount; i++)
        {
            ContactPoint2D contact = groundContacts[i];
            if(contact.normal.y >0.7f)
            {
                return true;
            }

        }
        return false;

    }
    public void RequestPath()
    {       
        path = pathFinder.PathFinding(
            FootPosition,
            targetPosition,
            agentHeight,
            maxAirHorizontalSpeed,
            jumpMaxHeight,
            gravity,
            AirClearanceMargin,
            AirHorizontalClearance,
            AirBodyHeight);

        if (path == null || path.Count == 0)
        {
            currentPathIndex = 0;
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;

        }
        currentPathIndex = 0;       
    }

    /// <summary>FSM�� ����(Trace)�� ��� �� ȣ��. �̵��� ���߰� Ǯ ��ȯ�� ��ü �ʱ�ȭ�� ResetMovement.</summary>
    public void StopMovement()
    {
        isTracing = false;
        repathPending = false;

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
    }

    /// <summary>���� Ǯ�� OnDespawn �� ? ��Ρ����� ���¡��ӵ� ���� �ʱ�ȭ.</summary>
    public void ResetMovement()
    {
        isTracing = false;
        repathPending = false;
        isAirMoving = false;
        isApproachingDrop = false;
        isWaitingToSteerDrop = false;
        hasLeftGround = false;

        path = null;
        currentPathIndex = 0;
        lastTargetNode = null;
        airTargetNode = null;
        targetPosition = default;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

}
