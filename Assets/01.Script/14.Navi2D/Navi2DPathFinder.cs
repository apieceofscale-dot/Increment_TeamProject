
using System.Collections.Generic;
using UnityEngine;

//점프에 가로길이 추가해야함 -> 캐릭터 가로길이가 2인데, 점프길이가 1이면 걍 걸어가기.
//Navi2Dgrddata 있는 오브젝트에 같이 붙이세요.
public class Navi2DPathFinder : MonoBehaviour
{   
    Navi2DGridata gD;

    private void Awake()
    {
        gD = GetComponent<Navi2DGridata>();
    }

    public List<Navi2DNode> PathFinding(Vector2 agentPos, Vector2 targetPos, float agentHeigth)
    {
        Navi2DNode startNode = FindClosestNode(agentPos);
        Navi2DNode targetNode = FindClosestNode(targetPos);

        if (startNode == null || targetNode == null) return null;

        List<Navi2DNode> open = new List<Navi2DNode>(); //조사할 노드
        HashSet<Navi2DNode> closed = new HashSet<Navi2DNode>(); //조사가 끝난 노드
        Dictionary<Navi2DNode, float> cost = new Dictionary<Navi2DNode, float>(); //각 노드까지 이동한 최소비용
        Dictionary<Navi2DNode, Navi2DNode> parent = new Dictionary<Navi2DNode, Navi2DNode>(); // 이 노드까지 어디에서 왔는가?

        open.Add(startNode);
        cost[startNode] = 0f;

        while (open.Count > 0)
        {
            // 이동이 비용이 가장 낮은 노드 찾기
            Navi2DNode current = open[0];
            for (int i = 1; i < open.Count; i++)
            {
                if (cost[open[i]] < cost[current])
                {
                    current = open[i];
                }
            }

            //목적지 도착후 그동안 있던 경로를 반환하는 코드.
            if (current == targetNode)//조사할 경로가 목적지
            {
                List<Navi2DNode> path = new List<Navi2DNode>();//반환용 리스트

                Navi2DNode pathNode = targetNode; //목적지부터 검사해서 시작지점까지.

                while (pathNode != startNode)//시작지점에 도달하면 종료
                {
                    path.Add(pathNode);//반환용 리스트에 목적지부터~시작노드까지 추가.
                    pathNode = parent[pathNode];//추적 담당용 딕셔너리에 키값이 현재노드, 값이 이전 노드로 저장 되어있음.
                    //그래서 노드를 pathNOde를 먼저 대임-> 이걸 키로 조회 하면 이전 노드가 나옴.
                }

                path.Add(startNode);//같은순간 끝나기 대문에 여기서 한 번 추가.

                path.Reverse(); //목적지 시작지가 반대라 뒤집음

                return path;
            }

            open.Remove(current);// currnet에 들어간 노드의 조사가 끝남
            closed.Add(current); //이제 여기 넣어 중복조사를 방지.

            //좌우만 이동가능. 일단은
            Vector2Int[] neighborPositions = { current.gridPos + Vector2Int.left, current.gridPos + Vector2Int.right }; //왜.x,y가 필요없지?

            foreach(Vector2Int neighborPos in neighborPositions)
            {
                if(!gD.NodeData.TryGetValue(neighborPos,out Navi2DNode neighbor)) continue; //양옆에 노드가 있는지 확인
                if (closed.Contains(neighbor)) continue; //조사가 끝낸 노드이면 넘어가기
                if(neighbor.height < agentHeigth) continue; //높이때문에 못 지나가면 넘어가기.

                float moveCost =Vector2.Distance(current.worldPos, neighbor.worldPos);// 현재->이웃 이동비용 게산.

                float newCost = cost[current] + moveCost;//총 비용을 계산. cost[current]는 시작지점부터 현재 지점까지라는 뜻.           

                if (!cost.ContainsKey(neighbor) || newCost < cost[neighbor])//처음 발견 node or 경로가 저렴해지면 갱신
                {
                    cost[neighbor] = newCost; //현재까지 발견한 neighbor까지의 최소 비용
                    parent[neighbor] = current; //그 비용으로 neghbor까지 왔을 때, 바로 전 노드는 current다.

                    if (!open.Contains(neighbor))//탐색 후보에 없던 노드라면 open에 추가.
                    {
                        open.Add(neighbor);
                    }
                }
            }  
        }
        return null;
    }



    public Navi2DNode FindClosestNode(Vector2 currentPos) //아직은 좌/우만 찾음. -> 좌우에 없을 수도 있으므로 개선 필요할 수도있음
                                                          //다만, 애초에 몹은 노드 위에 배치되어야 하므로, 런타임에서 넉백등으로
                                                          //날라가는 게 문제인데 그건 나중에 생각
    {
        Vector2Int currentGrid = gD.WorldToGridPos(currentPos);

        foreach (var pair in gD.NodeData)
        {
           Debug.Log($"Node Key : {pair.Key}, WorldPos : {pair.Value.worldPos}");            
        }
        //현재 위치에 노드가 있으면 즉시 반환.
        if (gD.NodeData.TryGetValue(currentGrid, out Navi2DNode currentNode))
        {
            Debug.Log($"FindClosestNode 반환 성공 : {currentNode.gridPos}");
            return currentNode;
        } 

        Vector2Int leftKey = currentGrid + Vector2Int.left;
        Vector2Int rightKey = currentGrid + Vector2Int.right;

        bool hasLeft = gD.NodeData.TryGetValue(leftKey, out Navi2DNode leftNode);
        bool hasRight = gD.NodeData.TryGetValue(rightKey, out Navi2DNode rightNode);

        // 둘 다 없음
        if (!hasLeft && !hasRight) return null;
        // 왼쪽만 있음
        if (hasLeft && !hasRight) return leftNode;
        // 오른쪽만 있음
        if (!hasLeft && hasRight) return rightNode;
        // 둘 다 있으면 실제 World 거리 비교
        float leftDistance = (leftNode.worldPos - currentPos).sqrMagnitude;
        float rightDistance = (rightNode.worldPos - currentPos).sqrMagnitude;

        return leftDistance <= rightDistance ? leftNode : rightNode;

    }

    //Dictionary<Vector2Int,Navi2DNode> nodeData
    /*
    public class Navi2DNode
    public Vector2Int gridPos;
    public Vector2 worldPos;
    public float height;
    */




    /*
    
    1. Navi2DNode에 이웃 개념 추가
2. Bake 후 Node끼리 좌우 연결
3. FindClosestNode()
4. A* 구현
5. Agent Height 필터 적용
6. 경로 반환
    */

}
