using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;


//네놈은 부트스트래퍼를 통해 pathfinder를 찾는다.
//결국 FSM에서 호출할 Trace함수 하나 만들어주면 끝.
public class Navi2DAgent : MonoBehaviour
{       
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckHeight = 0.1f;
    [SerializeField] private float groundCheckWidthRatio = 0.8f;

    [Header("Agent 설정")]    
    [SerializeField] private float MoveSpeed = 3f; //캐릭터 스테이터스
<<<<<<< HEAD
    [SerializeField] private float jumpMaxHeight = 2f; // 실제 값 아님. 
=======
    [SerializeField] private float jumpMaxHeight = 0.1f; // 실제 값 아님. 
>>>>>>> teamjang
    [SerializeField] private float agentHeight = 1f; //스프라이트 값 가져오기   

    private Navi2DPathFinder pathFinder;    
    private Rigidbody2D rb;
<<<<<<< HEAD
=======

>>>>>>> teamjang
    private Collider2D col;

    private List<Navi2DNode> path;
    private int currentPathIndex;

    private Vector2 targetPosition;
    private Navi2DNode jumpTargetNode;
    private Navi2DNode lastTargetNode;

    private bool isTracing;
    private bool isJumping;
    private bool repathPending;
    private bool isDropping;
    private bool hasleftGround;

<<<<<<< HEAD
=======
    float gravity;

>>>>>>> teamjang
    public Vector2 FootPosition
    { get
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
    }

    private void Start()
    {
<<<<<<< HEAD
        
=======
        gravity = Mathf.Abs(Physics2D.gravity.y * rb.gravityScale);
>>>>>>> teamjang
    }

    private void FixedUpdate()
    {
        if (!isTracing) return;

        if (isJumping)
        {
            UpdateJump();
            return;
        }
        if (isDropping)
        {
            UpdateDrop();
            return;
        }

        FollowPath();
    }
    private void FollowPath() //walk에서 MoveToNode 를 재호출 하니 while문으로 고치기.
    {
        if(path == null || path.Count == 0) return;

        if(currentPathIndex >= path.Count)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            
            return;
        }

        Navi2DNode targetNode = path[currentPathIndex];

        MoveToNode(targetNode);
    }

    private void MoveToNode(Navi2DNode targetNode)
    {
        if(currentPathIndex <= 0) return;

        Navi2DNode currentNode = path[currentPathIndex - 1];
        if(currentNode == null) return;

        Vector2Int delta = targetNode.gridPos - currentNode.gridPos;

        bool isWalk = delta.y == 0 && Mathf.Abs(delta.x) <= 1; //근데 이러면 노드단위로 이동하긴 함.

        if(isWalk)
        {
            Walk(targetNode);
        }
        else
        {
            MoveThroughLink(currentNode, targetNode);
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
                MoveToNode(path[currentPathIndex]);
            }

            return;
        }

        float direction = Mathf.Sign(targetNode.worldPos.x - FootPosition.x);
        rb.linearVelocity = new Vector2(direction *MoveSpeed, rb.linearVelocity.y);

    }
    private void MoveThroughLink(Navi2DNode currentNode, Navi2DNode targetNode)
    {
        float heightDelta = targetNode.worldPos.y - currentNode.worldPos.y;
        if(heightDelta >= -0.01f)
        {
            StartJump(currentNode,targetNode);
            return;
        }        

        StartDrop(currentNode, targetNode);

    }

    private void StartJump(Navi2DNode currentNode, Navi2DNode targetNode)
    {
        //Debug.Log("StartJump 호출");
        if (isJumping) return;

<<<<<<< HEAD
        float gravity = Mathf.Abs(Physics2D.gravity.y * rb.gravityScale);
=======
        
>>>>>>> teamjang

        bool canJump = Navi2DJumpCalculator.TryCalculateJumpVelocity(
            currentNode.worldPos,
            targetNode.worldPos,
            MoveSpeed,
            jumpMaxHeight,
            gravity,
            out Vector2 jumpVelocity);

        if (!canJump)
        {
            Debug.LogWarning($"점프 불가능 : {currentNode.gridPos} -> {targetNode.gridPos}");
            rb.linearVelocity = new Vector2(0f, rb.linearVelocityY);

            return;
        }

        rb.linearVelocity = jumpVelocity;

        jumpTargetNode = targetNode;
        isJumping = true;   
    }

    private void StartDrop(Navi2DNode currentNode, Navi2DNode targetNode)
    {
        float deltaX = targetNode.worldPos.x - currentNode.worldPos.x;

        if(Mathf.Abs(deltaX) < 0.01f)
        {
            Debug.LogWarning($"수직 Drop은 현재 지원하지 않음 : " + $"{currentNode.gridPos} -> {targetNode.gridPos}");
            return;
        }

        float direction = Mathf.Sign(deltaX);
        rb.linearVelocity = new Vector2(direction * MoveSpeed, 0f);

        hasleftGround = false;
        isDropping = true;


    }

    private void UpdateDrop()
    {
        if(!hasleftGround)
        {
            if(!IsGrounded())
            {
                hasleftGround=true;
            }
            return;
        }

        if (!IsGrounded()) return;


        isDropping =false;
        hasleftGround = false;

        if(repathPending)
        {
            repathPending = false;
            RequestPath();
            return;
        }

        currentPathIndex++;

        

    }

    private void UpdateJump()
    {
        if(rb.linearVelocity.y > 0f)
            return;

        if (!IsGrounded()) return;

        isJumping = false;      
        jumpTargetNode = null;

        if(repathPending)
        {
            repathPending = false;
            RequestPath();
            return;
        }

        currentPathIndex++;
    }

    private bool IsGrounded()
    {
        Bounds bounds = col.bounds;

        Vector2 checkPosition = new Vector2(bounds.center.x, bounds.min.y - groundCheckHeight * 0.5f);
        Vector2 checkSize = new Vector2(bounds.size.x * groundCheckWidthRatio, groundCheckHeight);

        return Physics2D.OverlapBox(checkPosition, checkSize, 0f, groundLayer) != null;
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
            repathPending = false;

            RequestPath();

            return;
        }


    }

    public void RequestPath()
    {
<<<<<<< HEAD
        path = pathFinder.PathFinding(FootPosition, targetPosition, agentHeight);
=======
        path = pathFinder.PathFinding(FootPosition, targetPosition, agentHeight, MoveSpeed,jumpMaxHeight, gravity);
>>>>>>> teamjang

        if (path == null || path.Count == 0)
        {
            currentPathIndex = 0;
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
<<<<<<< HEAD
        }

=======

        }
>>>>>>> teamjang
        currentPathIndex = path.Count > 1 ? 1 : 0;
        Debug.Log($"Repath : Target Node {lastTargetNode?.gridPos}");
    }

}
