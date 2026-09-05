using UnityEngine;
using System.Collections.Generic;


//네놈은 부트스트래퍼를 통해 pathfinder를 찾는다.
//결국 FSM에서 호출할 Trace함수 하나 만들어주면 끝.
public class Navi2DAgent : MonoBehaviour
{
    [SerializeField] private Transform testDes;
    [SerializeField] private float runSpeed = 3f; //캐릭터 스테이터스
    [SerializeField] private float jumpMaxHeight = 3f; // 캐릭터 스테이터스
    [SerializeField] private float agentHeight = 1f; //스프라이트 값 가져오기

    public float walkCostModifier = 1f; //지금은 그냥 쓰지만, 나중에 NavigationStatus 를 만들어서 넣을 개념들.
    public float jumpCostModifier = 1f;
    public float dropCostModifier = 1f;

    private float jumpTranslate;
    public Vector2 FootPosition => (Vector2)transform.position + Vector2.down * (agentHeight * 0.5f);

    private Navi2DPathFinder finder;
    private List<Navi2DNode> path;
    private Rigidbody2D rb;

    private int currentPathIndex;


    private void Awake()
    {
        finder = FindFirstObjectByType<Navi2DPathFinder>();
        rb = GetComponent<Rigidbody2D>();


    }

    private void Start()
    {
        currentPathIndex = 0;

        Vector2 targetFootPosition = (Vector2)testDes.position + Vector2.down * (agentHeight * 0.5f);

        path = finder.PathFinding(FootPosition, targetFootPosition, agentHeight);
        jumpTranslate = jumpMaxHeight;

    }
    private void FixedUpdate()
    {
        movetest();
    }
    private void movetest()
    {
        if (path == null || path.Count == 0) return;

        if (currentPathIndex >= path.Count) return;

        Navi2DNode currentNode = path[currentPathIndex];

        Vector2 targetPos = currentNode.worldPos + Vector2.up * (agentHeight * 0.5f);

        Vector2 newPos = Vector2.MoveTowards(rb.position, targetPos, runSpeed * Time.fixedDeltaTime
        );

        rb.MovePosition(newPos);

        if ((rb.position - targetPos).sqrMagnitude < 0.01f)
        {
            currentPathIndex++;
        }
    }


    public Vector2 GetFootPos()
    {
        return Vector2.zero;
    }
}
