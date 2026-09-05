using UnityEngine;


//빈 오브젝트 만들기->자식으로 2개 빈 오브젝트. 그 2개를 위치로.
//링크는 어떤 방식이든 '걸어서'갈 수 없는 두 지점을 연결한다.
public class Navi2DLink : MonoBehaviour
{
    [Header("Link의 양 끝점")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;

    public Vector2 PointA => pointA.position;
    public Vector2 PointB => pointB.position;

    private void OnDrawGizmos()
    {
        if (pointA == null || pointB == null)
            return;

        Gizmos.DrawLine(pointA.position, pointB.position);
    }
}

public class Navi2DLinkData
{
    public Navi2DNode ANode;
    public Navi2DNode BNode; 

    public Navi2DLinkData(Navi2DNode ANode, Navi2DNode BNode)
    {
        this.ANode = ANode;
        this.BNode = BNode;             
    }
}
