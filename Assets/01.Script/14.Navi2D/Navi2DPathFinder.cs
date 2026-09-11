
using System.Collections.Generic;
using UnityEngine;

//점프에 가로길이 추가해야함 -> 캐릭터 가로길이가 2인데, 점프길이가 1이면 걍 걸어가기.
//Navi2Dgrddata 있는 오브젝트에 같이 붙이세요.
public class Navi2DPathFinder : MonoBehaviour  //closestReachableNode 만들기
{
    Navi2DGridata gD;

    private void Awake()
    {
        gD = GetComponent<Navi2DGridata>();
    }

    public List<Navi2DPathStep> PathFinding(
        Vector2 agentPos,
        Vector2 targetPos,
        float agentHeigth,
        float airMoveSpeed,
        float jumpMaxHeight,
        float gravity,
        float airClearanceMargin,
        float airHorizontalClearance,
        float airBodyHeight)
    {
        //Debug.Log($"PathFinding 호출 / LinkCount = {gD.LinkData.Count}");

        Navi2DNode startNode = FindGroundNodeBelow(agentPos);       
        Navi2DNode targetNode = FindGroundNodeBelow(targetPos);        

        if (startNode == null || targetNode == null)
        {
            Debug.LogWarning($"PathFinding 실패 - Start:{startNode?.gridPos.ToString() ?? "null"} / Target:{targetNode?.gridPos.ToString() ?? "null"}");

            return null;
        }

        List<Navi2DNode> open = new List<Navi2DNode>(); //조사할 노드
        HashSet<Navi2DNode> closed = new HashSet<Navi2DNode>(); //조사가 끝난 노드
        Dictionary<Navi2DNode, float> cost = new Dictionary<Navi2DNode, float>(); //각 노드까지 이동한 최소비용
        

        Dictionary<Navi2DNode, Navi2DPathStep> cameFrom = new Dictionary<Navi2DNode, Navi2DPathStep>();

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
                List<Navi2DPathStep> path = new List<Navi2DPathStep>();//반환용 리스트

                Navi2DNode pathNode = targetNode; //목적지부터 검사해서 시작지점까지.

                while (pathNode != startNode)//시작지점에 도달하면 종료
                {
                    Navi2DPathStep step = cameFrom[pathNode];

                    path.Add(step);

                    pathNode = step.fromNode;                    
                }
               
                path.Reverse(); //목적지 시작지가 반대라 뒤집음

                return path;
            }

            open.Remove(current);// currnet에 들어간 노드의 조사가 끝남
            closed.Add(current); //이제 여기 넣어 중복조사를 방지.

            Vector2Int[] neighborPositions = { current.gridPos + Vector2Int.left, current.gridPos + Vector2Int.right };

            foreach (Vector2Int neighborPos in neighborPositions)
            {
                if (!gD.NodeData.TryGetValue(neighborPos, out Navi2DNode neighbor)) continue; //양옆에 노드가 있는지 확인
                if (closed.Contains(neighbor)) continue; //조사가 끝낸 노드이면 넘어가기
                if (neighbor.height < agentHeigth) continue; //높이때문에 못 지나가면 넘어가기.

                float moveCost = Vector2.SqrMagnitude(current.worldPos - neighbor.worldPos);// 현재->이웃 이동비용 게산.

                float newCost = cost[current] + moveCost;//총 비용을 계산. cost[current]는 시작지점부터 현재 지점까지라는 뜻.           

                if (!cost.ContainsKey(neighbor) || newCost < cost[neighbor])//처음 발견 node or 경로가 저렴해지면 갱신
                {
                    cost[neighbor] = newCost; //현재까지 발견한 neighbor까지의 최소 비용
                    
                    cameFrom[neighbor] = new Navi2DPathStep(current, neighbor, Navi2DMoveType.walk);


                    if (!open.Contains(neighbor))//탐색 후보에 없던 노드라면 open에 추가.
                    {
                        open.Add(neighbor);
                    }


                }
            }




            foreach (Navi2DLinkData link in gD.LinkData)
            {
                List<Navi2DNode> targetCandidates = null;

                if (link.aCandidates.Contains(current))  //a->b냐 b->a냐 결정하는 과정
                {
                    
                    targetCandidates = link.bCandidates;
                }
                else if (link.bCandidates.Contains(current))
                {
                    
                    targetCandidates = link.aCandidates;
                }
                else
                {
                    continue;
                }
               
                foreach (Navi2DNode linkNeighbor in targetCandidates)
                {
                    if(linkNeighbor == null) continue;
                    if(closed.Contains(linkNeighbor)) continue;
                    if(linkNeighbor.height < agentHeigth) continue;

                    /*Debug.Log(    $"PathFinder Link 데이터 / " +
                     * $"CeilingY={link.ceilingBottomY}, " +
                     * $"CeilingMinX={link.ceilingMinX}, " +
                     * $"CeilingMaxX={link.ceilingMaxX}");*/
                    float deltaY = linkNeighbor.worldPos.y - current.worldPos.y;

                    bool canMove;

                    Navi2DMoveType moveType;
                    float moveCost = Vector2.SqrMagnitude(current.worldPos - linkNeighbor.worldPos);

                    if (deltaY > 0.1f)
                    {
                        // 위쪽 발판으로 이동
                        moveType = Navi2DMoveType.JumpUp;

                        canMove = TryCalculateJumpUpVelocity(
                            current.worldPos,
                            linkNeighbor.worldPos,
                            airMoveSpeed,
                            jumpMaxHeight,
                            gravity,
                            airClearanceMargin,
                            airHorizontalClearance,
                            airBodyHeight,
                            out _);
                    }
                    else if (deltaY < -0.1f)
                    {
                        moveType = Navi2DMoveType.Drop;

                        canMove = TryPlanDrop(current, current.worldPos, linkNeighbor.worldPos,
                            airMoveSpeed, gravity, airHorizontalClearance, airBodyHeight,
                            out Vector2 departure, out _);
                        if (canMove)
                            moveCost = (departure - current.worldPos).sqrMagnitude +
                                (linkNeighbor.worldPos - departure).sqrMagnitude;
                        else
                        {
                            // A lower platform may be too far away for a plain drop.
                            moveType = Navi2DMoveType.Traverse;
                            canMove = TryCalculatePlatformJumpVelocity(current.worldPos,
                                linkNeighbor.worldPos, airMoveSpeed, jumpMaxHeight, gravity,
                                airClearanceMargin, airHorizontalClearance, airBodyHeight, out _);
                        }
                    }

                    else
                    {
                        // Jump across a gap, with or without an obstacle between platforms.
                        moveType = Navi2DMoveType.Traverse;

                        canMove = TryCalculatePlatformJumpVelocity(
                            current.worldPos,
                            linkNeighbor.worldPos,
                            airMoveSpeed,
                            jumpMaxHeight,
                            gravity,
                            airClearanceMargin,
                            airHorizontalClearance,
                            airBodyHeight,
                            out _);
                    }

                    if (!canMove)
                        continue;


                    float newCost = cost[current] + moveCost;

                    if (!cost.ContainsKey(linkNeighbor) || newCost < cost[linkNeighbor])
                    {
                        cost[linkNeighbor] = newCost;


                       
                        cameFrom[linkNeighbor] = new Navi2DPathStep(current, linkNeighbor, moveType, link);
                        if (!open.Contains(linkNeighbor))
                        {
                            open.Add(linkNeighbor);
                        }

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
        Vector2Int currentGrid = gD.WorldToGroundGridPos(currentPos);

        foreach (var pair in gD.NodeData)
        {
            //Debug.Log($"Node Key : {pair.Key}, WorldPos : {pair.Value.worldPos}");            
        }
        //현재 위치에 노드가 있으면 즉시 반환.
        if (gD.NodeData.TryGetValue(currentGrid, out Navi2DNode currentNode))
        {
            //Debug.Log($"FindClosestNode 반환 성공 : {currentNode.gridPos}");
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

    public bool TryPlanDrop(Navi2DNode source, Vector2 start, Vector2 target,
        float airSpeed, float gravity, float halfWidth, float bodyHeight,
        out Vector2 departure, out Vector2 velocity)
    {
        departure = Vector2.zero;
        velocity = Vector2.zero;
        if (source == null || target.y >= source.worldPos.y - 0.1f) return false;
        float bestCost = float.PositiveInfinity;
        for (int direction = -1; direction <= 1; direction += 2)
        {
            if (!gD.TryGetDropDeparture(source, direction, halfWidth, out Vector2 candidate)) continue;
            if (!gD.IsWalkSegmentClear(start, candidate, halfWidth, bodyHeight)) continue;
            if (!TryCalculateDropMotion(candidate, target, airSpeed, gravity, 0f,
                halfWidth, bodyHeight, out Vector2 candidateVelocity, out _)) continue;
            float cost = (candidate - start).sqrMagnitude + (target - candidate).sqrMagnitude;
            if (cost >= bestCost) continue;
            bestCost = cost;
            departure = candidate;
            velocity = candidateVelocity;
        }
        return !float.IsPositiveInfinity(bestCost);
    }

    // A lower target can sit under the source platform. Fall beside its wall first,
    // then steer inward only after the whole body can pass beneath the platform.
    public bool TryCalculateDropMotion(Vector2 start, Vector2 target,
        float airSpeed, float gravity, float initialVelocityY,
        float halfWidth, float bodyHeight, out Vector2 steeringVelocity, out float steeringDelay)
    {
        steeringVelocity = Vector2.zero;
        steeringDelay = 0f;
        if (!Navi2DDropCalculator.TryCalculateDropVelocity(start,
            new Vector2(start.x, target.y), airSpeed, gravity, initialVelocityY,
            out _, out float totalTime)) return false;

        const int samples = 32;
        for (int i = 0; i <= samples; i++)
        {
            float delay = totalTime * i / (samples + 1f);
            Vector2 fallVelocity = new Vector2(0f, initialVelocityY);
            if (delay > 0f && !gD.IsAirArcClear(start, fallVelocity, delay, gravity,
                halfWidth, bodyHeight)) continue;
            Vector2 turnPosition = new Vector2(start.x,
                start.y + initialVelocityY * delay - 0.5f * gravity * delay * delay);
            if (!TryCalculateDropVelocity(turnPosition, target, airSpeed, gravity,
                initialVelocityY - gravity * delay, halfWidth, bodyHeight, out Vector2 candidate)) continue;
            steeringVelocity = candidate;
            steeringDelay = delay;
            return true;
        }
        return false;
    }

    public bool TryCalculateDropVelocity(Vector2 start, Vector2 target,
        float airSpeed, float gravity, float initialVelocityY,
        float halfWidth, float bodyHeight, out Vector2 velocity)
    {
        if (!Navi2DDropCalculator.TryCalculateDropVelocity(start, target, airSpeed,
            gravity, initialVelocityY, out velocity, out float duration)) return false;
        if (gD.IsAirArcClear(start, velocity, duration, gravity, halfWidth, bodyHeight)) return true;
        velocity = Vector2.zero;
        return false;
    }

    public bool TryCalculatePlatformJumpVelocity(Vector2 start, Vector2 target,
        float airSpeed, float maxRise, float gravity, float clearance,
        float halfWidth, float bodyHeight, out Vector2 velocity)
    {
        return Navi2DAirMoveCalculator.TryCalculatePlatformJumpVelocity(
            start, target, airSpeed, maxRise, gravity, clearance,
            (candidate, duration) => gD.IsAirArcClear(start, candidate, duration,
                gravity, halfWidth, bodyHeight), out velocity);
    }

    public bool TryCalculateJumpUpVelocity(Vector2 start, Vector2 target,
        float airSpeed, float maxRise, float gravity, float clearance,
        float halfWidth, float bodyHeight, out Vector2 velocity)
    {
        return Navi2DAirMoveCalculator.TryCalculateJumpUpVelocity(
            start, target, airSpeed, maxRise, gravity, clearance, halfWidth,
            (candidate, duration) => gD.IsAirArcClear(start, candidate, duration,
                gravity, halfWidth, bodyHeight), out velocity);
    }

    public Navi2DNode FindGroundNodeBelow(Vector2 worldPos)
    {
        if (gD.TryGetGroundNodeBelow(worldPos, out Navi2DNode node))
        {
            return node;
        }
        return null;
    }

}
