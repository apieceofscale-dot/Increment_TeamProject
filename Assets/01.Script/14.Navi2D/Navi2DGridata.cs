using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

//런타임시 부분 재계산 기능 필요
//맵이 쉽게 바뀌는 게 아니므로 시작하기 전에 이걸 seriailzable로 해서 값 자체를 미리 저장한 후 게임으로 보내기 -> 부분재계산이 가장 좋음.
public class Navi2DGridata : MonoBehaviour
{
    private Tilemap boundsTile;
    private Tilemap[] mapTiles;
    private TilemapRenderer tilemapRenderer; //타일맵 경계를 그려주는 용도. 시작하면 경계는 안 보임.  
    private BoundsInt bounds; 

    [Header("2의 지수로만. 근데 아직 코드에 적용 안됐음.")]
    [SerializeField] private int resolution = 1;
    private float nodeSize;
        
    private Dictionary<Vector2Int,Navi2DNode> nodeData;   
    public IReadOnlyDictionary<Vector2Int, Navi2DNode> NodeData => nodeData;

    private List<Navi2DLinkData> linkData;
    public IReadOnlyList<Navi2DLinkData> LinkData => linkData;


    private void Awake()
    {
        boundsTile = GetComponent<Tilemap>();
        Tilemap[] allTiles = GetComponentsInChildren<Tilemap>();

        List<Tilemap> temp = new List<Tilemap>();
        foreach (Tilemap t in allTiles)
        {
            if(t == boundsTile) continue;
            temp.Add(t);            
        }
        mapTiles = temp.ToArray();


        tilemapRenderer = GetComponent<TilemapRenderer>();
        tilemapRenderer.enabled = false;

        nodeData = new Dictionary<Vector2Int, Navi2DNode>();
        linkData = new List<Navi2DLinkData>();

        nodeSize = boundsTile.cellSize.x / resolution;
        bounds = boundsTile.cellBounds;

        BakeNodeData();
        BakeLinkData();
    }

    private void BakeNodeData() //아직은 땅 밖에 못찾음.
    {
        nodeData.Clear();

        for (int i = bounds.xMin; i < bounds.xMax; i++)
        {
            for (int j = bounds.yMin; j < bounds.yMax; j++)
            {
                Vector3Int pos = new Vector3Int(i, j,0);

                if (!HasMapTile(pos)) continue;

                if(HasMapTile(pos + Vector3Int.up)) continue;

                Vector3 center = boundsTile.GetCellCenterWorld(pos); //실제 좌표계로 바꿔주기 위한 절차
                Vector2 nodePos = new Vector2(center.x, center.y + boundsTile.cellSize.y * 0.5f); //바닥이 노드 위치임.                              

                float height = float.PositiveInfinity;

                for (int y = j + 1; y < bounds.yMax; y++) //천장 까지 가는 코드.
                {
                    Vector3Int upperPos = new Vector3Int(i, y,0);
                    if(!HasMapTile(upperPos)) continue;

                    Vector3 upperCenter = boundsTile.GetCellCenterWorld(upperPos); //천장이 있으면 실제 좌표계로 전환.

                    float ceilingBottom = upperCenter.y - boundsTile.cellSize.y * 0.5f; //실제 천장 위치.

                    height = ceilingBottom -nodePos.y;

                    break;
                }

                nodeData.Add(new Vector2Int(i,j), new Navi2DNode(new Vector2Int(i, j), nodePos,height));
                
            }
        }
    }

    public void BakeLinkData()
    {
        linkData.Clear();

        Navi2DLink[] links = GetComponentsInChildren<Navi2DLink>();

        foreach(Navi2DLink link in links)
        {
            if(!link.IsValid) continue;

            if(!TryGetGroundNodeBelow(link.PointA, out Navi2DNode aNode))
            {
                Debug.LogError($"[Navi2D] {link.name}의 PointA에 노드 없음");
                continue;
            }

            if (!TryGetGroundNodeBelow(link.PointB, out Navi2DNode bNode))
            {
                Debug.LogError($"[Navi2D] {link.name}의 PointB에 노드 없음");
                continue;
            }

            linkData.Add(new Navi2DLinkData(aNode, bNode));
            //Debug.Log($"[Navi2D Link] " + $"{aNode.gridPos} <-> {bNode.gridPos}");
        }

       
    }




    private bool HasMapTile(Vector3Int pos)
    {
        foreach (Tilemap map in mapTiles)
        {
            if (map.HasTile(pos))
                return true;
        }
        return false;
    }

    public Vector2Int WorldToGroundGridPos(Vector2 pos)
    {
        Vector3Int cell = boundsTile.WorldToCell(pos + Vector2.down*0.01f);

       
        return new Vector2Int(cell.x,cell.y);            
    }
    
    public bool TryGetGroundNodeBelow(Vector2 worldPos, out Navi2DNode node) 
    {
        Vector2Int cell = WorldToGroundGridPos(worldPos);
       
        for(int i = cell.y; i>=bounds.yMin; i--)//플레이어가 공중에 있을 경우 아래 노드를 찾음)
        {
            Vector2Int gridPos = new Vector2Int(cell.x, i);

            if(nodeData.TryGetValue(gridPos, out node)) return true;
        }
       /* if (nodeData.TryGetValue(cell, out node)) return true;

        Vector2Int below = cell + Vector2Int.down;

        if(nodeData.TryGetValue(below, out node)) return true;
       */
        node = null;
        return false;
    }
    //int.Tryparse 메서드를 선언하는 방식과 똑같음.
     //이 방식을 쓰는 이유는 노드가 필요한데 null을 반환하면 null체크를 해야함.
     //try~를 쓰면 bool을 함께 반환하기 때문에 의도가 명확해져서 읽기 좋은 코드가됨.



    private void OnDrawGizmos()
    {
        if(nodeData ==null) return;

        foreach (var node in nodeData)
        {
            Gizmos.DrawWireCube(node.Value.worldPos, new Vector3(0.1f, 0.1f, 0f));
            if (!float.IsPositiveInfinity(node.Value.height))
            {
                Gizmos.DrawLine(node.Value.worldPos, node.Value.worldPos + Vector2.up * node.Value.height );
            }
        }
    }
    



}



public class Navi2DNode
{
    public Vector2Int gridPos;
    public Vector2 worldPos;
    public float height;

    public Navi2DNode(Vector2Int gridpos, Vector2 worldPos, float height)
    {
        this.gridPos = gridpos;
        this.worldPos = worldPos;
        this.height = height;
    }
}

