using UnityEngine;
using System.Collections.Generic;


//네놈은 부트스트래퍼를 통해 pathfinder를 찾는다.
//결국 FSM에서 호출할 Trace함수 하나 만들어주면 끝.
public class Navi2DAgent : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;
    

    [Header("Agent 설정")]
    [SerializeField] private float MoveSpeed = 3f; //캐릭터 스테이터스
    [SerializeField] private float jumpMaxHeight = 2f; // 실제 값 아님. 
    [SerializeField] private float agentHeight = 1f; //스프라이트 값 가져오기

    private float AirClearanceMargin
    {
        get
        {
            return col.bounds.extents.y * 0.2f;
        }
    }

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

    private void Awake()//부트스트래퍼에서 주입할 애들.
    {
        pathFinder = FindFirstObjectByType<Navi2DPathFinder>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        groundContactFilter = new ContactFilter2D();
        groundContactFilter.SetLayerMask(groundLayer); //필터 정보에 사용하고자 하는 레이어 마스크 정보
        groundContactFilter.useTriggers = false; //필터 정보에 Trigger쓰는 콜라이더 제외
    }

    private void Start()
    {
        gravity = Mathf.Abs(Physics2D.gravity.y * rb.gravityScale);
    }

    private void FixedUpdate()
    {
        if (!isTracing) return;

        if(isAirMoving)
        {
            UpdateAirMove();
            return;
        }

        FollowPath();
    }
    private void FollowPath() //walk에서 MoveToNode 를 재호출 하니 while문으로 고치기.
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

            case Navi2DMoveType.AirMove:
                StartAirMove(step);
                break;
        }

    }

    private void Walk(Navi2DNode targetNode) //일자로 가야 한다면 그냥 노드를 압축해서 한번에 가는 걸로 최적화.
    {

        float distanceX = Mathf.Abs(targetNode.worldPos.x - FootPosition.x);

        if (distanceX < 0.05f)
        {
            currentPathIndex++;

            if (currentPathIndex < path.Count)
            {
                MoveToStep(path[currentPathIndex]);
            }

            return;
        }

        float direction = Mathf.Sign(targetNode.worldPos.x - FootPosition.x);
        rb.linearVelocity = new Vector2(direction * MoveSpeed, rb.linearVelocity.y);

    }
 
    private void StartAirMove(Navi2DPathStep step)
    {
        if (isAirMoving)
            return;

        if (!IsGrounded())
            return;

        bool canAirMove = Navi2DAirMoveCalculator.TryCalculateAirVelocity(
            step.fromNode.worldPos,
            step.toNode.worldPos,
            MoveSpeed,
            jumpMaxHeight,
            gravity,
            AirClearanceMargin,
            step.linkData.obstacleTopY,
            out Vector2 airVelocity);

        if (step.linkData == null)
        {
            Debug.LogError($"AirMove Step에 LinkData가 없습니다. " + $"{step.fromNode.gridPos} -> {step.toNode.gridPos}");
            return;
        }

        if (!canAirMove)
        {
            Debug.LogWarning($"AirMove 불가능 :{step.fromNode.gridPos} -> {step.toNode.gridPos}");

            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

            return;
        }


        rb.linearVelocity = airVelocity;
        airTargetNode = step.toNode;

        hasLeftGround = false;
        isAirMoving = true;
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
        hasLeftGround = false;
        airTargetNode = null;

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


    public void Trace(Vector2 targetPos) //FSM에서 실제로 호출해야 할 함수.
                                         //플레이어가 이동했을 때 repath하도록 최적화
    {
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

        if (!IsGrounded())
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

    public void RequestPath()
    {
        path = pathFinder.PathFinding(FootPosition, targetPosition, agentHeight, MoveSpeed, jumpMaxHeight, gravity, AirClearanceMargin);

        if (path == null || path.Count == 0)
        {
            currentPathIndex = 0;
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;

        }
        currentPathIndex = 0;       
    }

}
