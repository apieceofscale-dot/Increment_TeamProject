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

    //private List<Navi2DNode> nodeData;
    private Dictionary<Vector2Int,Navi2DNode> nodeData;
    //public IReadOnlyList<Navi2DNode> NodeData => nodeData;
    public IReadOnlyDictionary<Vector2Int, Navi2DNode> NodeData => nodeData;


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
        nodeSize = boundsTile.cellSize.x / resolution;
        bounds = boundsTile.cellBounds;

        BakeNodeData();
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


    private bool HasMapTile(Vector3Int pos)
    {
        foreach (Tilemap map in mapTiles)
        {
            if (map.HasTile(pos))
                return true;
        }
        return false;
    }

    public Vector2Int WorldToGridPos(Vector2 pos)
    {
        Vector3Int cell = boundsTile.WorldToCell(pos + Vector2.down*0.01f);

        return new Vector2Int(cell.x,cell.y);     

    }





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

