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

            List<Navi2DNode> aCandidates = GetLinkCandidates(aNode,link.CandidateRange);
            List<Navi2DNode> bCandidates = GetLinkCandidates(bNode,link.CandidateRange);

            GetLinkObstacleData(
                aNode,
                bNode,
                out float obstacleTopY,
                out float obstacleMinX,
                out float obstacleMaxX,
                out float ceilingBottomY,
                out float ceilingMinX,
                out float ceilingMaxX);



            //Debug.Log($"Link 장애물 : " + $"TopY={obstacleTopY}, " + $"MinX={obstacleMinX}, " + $"MaxX={obstacleMaxX}");
            /*
            foreach (Navi2DNode node in aCandidates)
            {
                Debug.Log($"A Candidate : {node.gridPos}");
            }

            foreach (Navi2DNode node in bCandidates)
            {
                Debug.Log($"B Candidate : {node.gridPos}");
            }
            Debug.Log($"Link : {aNode.gridPos} <-> {bNode.gridPos} / " +  $"ObstacleTopY : {obstacleTopY}");
            */


            linkData.Add(new Navi2DLinkData(aNode, bNode, aCandidates, bCandidates, obstacleTopY, obstacleMinX, obstacleMaxX, ceilingBottomY, ceilingMinX, ceilingMaxX));

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


    private List<Navi2DNode> GetLinkCandidates(Navi2DNode anchorNode,int range)
    {
        List<Navi2DNode> candidates = new List<Navi2DNode>();

        if(anchorNode == null) return candidates;

        candidates.Add(anchorNode);

        for (int i = 1; i <= range; i++) //왼쪽 후보 모으기
        {
            Vector2Int gridPos = anchorNode.gridPos + Vector2Int.left * i;

            if (!nodeData.TryGetValue(gridPos, out Navi2DNode node)) break; //중간에 끊긴 노드 체크
            
            candidates.Add(node);
        }

        for(int i = 1; i<=range; i++) //오른쪽 후보 모으기
        {
            Vector2Int gridPos = anchorNode.gridPos + Vector2Int.right * i;

            if(!nodeData.TryGetValue(gridPos,out Navi2DNode node)) break;
            candidates.Add(node);
        }

        return candidates;
    }



    private void GetLinkObstacleData(
        Navi2DNode aNode,
        Navi2DNode bNode, 
        out float obstacleTopY,
        out float obstacleMinX,
        out float obstacleMaxX,
        out float ceilingBottomY,
        out float ceilingMinX,
        out float ceilingMaxX)
    {


        int minX = Mathf.Min(aNode.gridPos.x, bNode.gridPos.x);
        int maxX = Mathf.Max(aNode.gridPos.x, bNode.gridPos.x);

        float baseY = Mathf.Min(aNode.worldPos.y,bNode.worldPos.y);
        obstacleTopY = baseY;

        obstacleMinX = float.PositiveInfinity;
        obstacleMaxX = float.NegativeInfinity;

        ceilingBottomY = float.PositiveInfinity;
        ceilingMinX = float.PositiveInfinity;
        ceilingMaxX = float.NegativeInfinity;

        float halfCellWidth = boundsTile.layoutGrid.cellSize.x * 0.5f;

        for (int x = minX+1; x< maxX;x++)
        {
            Navi2DNode closestSurface = null;

            foreach (var node in nodeData.Values)
            {
                if (node.gridPos.x != x) continue;
                if (node.worldPos.y < baseY) continue;

                if (!float.IsInfinity(node.height))
                {
                    float nodeCeilingBottomY = node.worldPos.y + node.height;

                    float ceilingCellMinX = node.worldPos.x - halfCellWidth;

                    float ceilingCellMaxX = node.worldPos.x + halfCellWidth;

                    if (nodeCeilingBottomY < ceilingBottomY)
                    {
                        ceilingBottomY = nodeCeilingBottomY;
                        ceilingMinX = ceilingCellMinX;
                        ceilingMaxX = ceilingCellMaxX;
                    }
                    else if (Mathf.Approximately(nodeCeilingBottomY, ceilingBottomY))
                    {
                        if (ceilingCellMinX < ceilingMinX)
                            ceilingMinX = ceilingCellMinX;

                        if (ceilingCellMaxX > ceilingMaxX)
                            ceilingMaxX = ceilingCellMaxX;
                    }
                }

                if (closestSurface == null || node.worldPos.y < closestSurface.worldPos.y)
                {
                    closestSurface = node;
                }
            }


            if (closestSurface == null) continue;
            if (closestSurface.worldPos.y <= baseY) continue;

            if(closestSurface.worldPos.y > obstacleTopY)
            {
                obstacleTopY = closestSurface.worldPos.y;
            }

            float cellMinX = closestSurface.worldPos.x - halfCellWidth;
            float cellMaxX = closestSurface.worldPos.x + halfCellWidth;

            if (cellMinX < obstacleMinX)
            {
                obstacleMinX = cellMinX;
            }

            if(cellMaxX > obstacleMaxX)
            {
                obstacleMaxX = cellMaxX;
            }
        }
        return;
    }



    public bool TryGetDropDeparture(Navi2DNode source, int direction, float halfWidth,
        out Vector2 departure)
    {
        departure = Vector2.zero;
        if (source == null || (direction != -1 && direction != 1)) return false;
        Navi2DNode edge = source;
        Vector2Int offset = direction > 0 ? Vector2Int.right : Vector2Int.left;
        while (nodeData.TryGetValue(edge.gridPos + offset, out Navi2DNode next))
            edge = next;

        Vector3Int cell = new Vector3Int(edge.gridPos.x, edge.gridPos.y, 0);
        if (direction > 0) cell += new Vector3Int(1, 0, 0);
        float edgeX = boundsTile.CellToWorld(cell).x;
        departure = new Vector2(edgeX + direction * (halfWidth + 0.02f), source.worldPos.y);
        return true;
    }

    public bool IsWalkSegmentClear(Vector2 start, Vector2 end, float halfWidth, float bodyHeight)
    {
        return IsAirArcClear(start, end - start, 1f, 0f, halfWidth, bodyHeight);
    }

    // Check the whole body against actual tiles, including the landing platform.
    // The parabola's extrema over each tile's X interval give continuous coverage.
    public bool IsAirArcClear(Vector2 start, Vector2 velocity, float duration,
        float gravity, float halfWidth, float bodyHeight)
    {
        const float contactTolerance = 0.005f;
        float apexTime = gravity > 0f ? Mathf.Clamp(velocity.y / gravity, 0f, duration) : 0f;
        float endX = start.x + velocity.x * duration;
        float endY = start.y + velocity.y * duration - 0.5f * gravity * duration * duration;
        float peakY = Mathf.Max(endY, start.y + velocity.y * apexTime - 0.5f * gravity * apexTime * apexTime);
        Vector3Int first = boundsTile.WorldToCell(new Vector3(
            Mathf.Min(start.x, endX) - halfWidth, Mathf.Min(start.y, endY), 0f));
        Vector3Int last = boundsTile.WorldToCell(new Vector3(
            Mathf.Max(start.x, endX) + halfWidth, peakY + bodyHeight, 0f));

        for (int x = Mathf.Max(bounds.xMin, first.x - 1); x <= Mathf.Min(bounds.xMax - 1, last.x + 1); x++)
        for (int y = Mathf.Max(bounds.yMin, first.y - 1); y <= Mathf.Min(bounds.yMax - 1, last.y + 1); y++)
        {
            Vector3Int cell = new Vector3Int(x, y, 0);
            if (!HasMapTile(cell)) continue;
            Vector3 min = boundsTile.CellToWorld(cell);
            Vector3 max = boundsTile.CellToWorld(cell + new Vector3Int(1, 1, 0));
            float left = min.x - halfWidth + contactTolerance;
            float right = max.x + halfWidth - contactTolerance;
            float enter = 0f;
            float exit = duration;
            if (Mathf.Abs(velocity.x) < 0.0001f)
            {
                if (start.x <= left || start.x >= right) continue;
            }
            else
            {
                float t1 = (left - start.x) / velocity.x;
                float t2 = (right - start.x) / velocity.x;
                enter = Mathf.Max(0f, Mathf.Min(t1, t2));
                exit = Mathf.Min(duration, Mathf.Max(t1, t2));
                if (enter >= exit) continue;
            }
            float y1 = start.y + velocity.y * enter - 0.5f * gravity * enter * enter;
            float y2 = start.y + velocity.y * exit - 0.5f * gravity * exit * exit;
            float topTime = Mathf.Clamp(apexTime, enter, exit);
            float highestFoot = start.y + velocity.y * topTime - 0.5f * gravity * topTime * topTime;
            if (Mathf.Min(y1, y2) < max.y - contactTolerance &&
                highestFoot + bodyHeight > min.y + contactTolerance)
                return false;
        }
        return true;
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

