using System.Collections.Generic;
using UnityEngine;



//빈 오브젝트 만들기->자식으로 2개 빈 오브젝트. 그 2개를 위치로.
//링크는 어떤 방식이든 '걸어서'갈 수 없는 두 지점을 연결한다.
public class Navi2DLink : MonoBehaviour // 나중에 이거 상속시켜서 사다리, 덫 만들기.
{
    [Header("Link의 양 끝점")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;

    [SerializeField, Min(0)] int candidateRange = 2;

    public Vector2 PointA => pointA.position;
    public Vector2 PointB => pointB.position;

    public int CandidateRange => candidateRange;

    public bool IsValid => pointA != null && pointB != null; 

    

    private void OnDrawGizmos()
    {
        if (!IsValid)
            return;

        Gizmos.DrawLine(pointA.position, pointB.position);
        Gizmos.DrawWireSphere(pointA.position, 0.1f);
        Gizmos.DrawWireSphere(pointB.position, 0.1f);
    }
}

public class Navi2DLinkData
{
    public Navi2DNode aNode;
    public Navi2DNode bNode;

    public List<Navi2DNode> aCandidates;
    public List<Navi2DNode> bCandidates;

    public float obstacleTopY;
    public float obstacleMinX;
    public float obstacleMaxX;

    public float ceilingBottomY;
    public float ceilingMinX;
    public float ceilingMaxX;


    public Navi2DLinkData(
    Navi2DNode aNode,
    Navi2DNode bNode,
    List<Navi2DNode> aCandidates,
    List<Navi2DNode> bCandidates,
    float obstacleTopY,
    float obstacleMinX,
    float obstacleMaxX,
    float ceilingBottomY,
    float ceilingMinX,
    float ceilingMaxX)
    {
        this.aNode = aNode;
        this.bNode = bNode;

        this.aCandidates = aCandidates;
        this.bCandidates = bCandidates;

        this.obstacleTopY = obstacleTopY;
        this.obstacleMinX = obstacleMinX;
        this.obstacleMaxX = obstacleMaxX;

        this.ceilingBottomY = ceilingBottomY;
        this.ceilingMinX = ceilingMinX;
        this.ceilingMaxX = ceilingMaxX;
    }
    /*
    public Navi2DLinkData(
        Navi2DNode aNode,
        Navi2DNode bNode,
        List<Navi2DNode> aCandidates,
        List<Navi2DNode> bCandidates,
        float obstacleTopY)
    {
        this.aNode = aNode;
        this.bNode = bNode;       
        
        this.aCandidates = aCandidates;
        this.bCandidates = bCandidates;

        this.obstacleTopY = obstacleTopY;
        ceilingMinX = float.PositiveInfinity;
        ceilingMaxX = float.NegativeInfinity;
    }*/
}
