using UnityEngine;
using System.Collections.Generic;

// 드랍 요청 모아서 프레임 단위로 처리
public sealed class ItemDropManager : MonoBehaviour
{
    // 전부 임시 상수
    private const int MaxSpawnPerFrame = 16; //프레임당 생성할 아이템 오브젝트 수 상한
    private const float ScatterRadius = 0.6f; //드랍이 여러 개일 때 겹쳐 보이지 않도록 흩뿌리기
    private const int InitialQueueCapacity = 32; // 리스트 재할당 방지

    // 아직 처리 안 된 드랍요청
    private readonly Queue<DropRequest> pendingRequests = new Queue<DropRequest>(InitialQueueCapacity);
    // 판정 결과 재사용용
    private readonly List<DropResult> resolveBuffer = new List<DropResult>(8);
    //확률계산기
    private readonly ItemDropProvider dropProvider = new ItemDropProvider();
    // 드랍 전용 난수
    private readonly System.Random random = new System.Random();


    private IDropTableSource dropTableSource;
    private ItemFacade itemFacade;

    /// <summary>
    /// ItemDropFacade가 호출해 참조 세팅
    /// </summary>
    public void Initialize(IDropTableSource source, ItemFacade facade)
    {
        dropTableSource = source ?? throw new System.ArgumentNullException(nameof(source));
        itemFacade = facade ?? throw new System.ArgumentNullException(nameof(facade));
    }

    /// <summary>
    /// 외부에서 필요시 ItemDropFacade.RequestDrop을 사용할것, 진행은 LateUpdate에서
    /// </summary>
    public void Enqueue(int dropTableId, Vector3 position)
    {
        pendingRequests.Enqueue(new DropRequest(dropTableId, position));
    }

    //  드랍 취소 요청시
    public void ClearPending()
    {
        pendingRequests.Clear();
    }

    private void LateUpdate()
    {
        if (pendingRequests.Count == 0)
        {
            return;
        }

        ProcessPendingRequests();
    }

    private void ProcessPendingRequests()
    {
        int spawnedThisFrame = 0;

        // 몬스터 한 마리의 드랍은 도중에 끊지 않고 통째로 처리
        while (pendingRequests.Count > 0 && spawnedThisFrame < MaxSpawnPerFrame)
        {
            DropRequest request = pendingRequests.Dequeue();

            if (!dropTableSource.TryGetEntries(request.DropTableId, out IReadOnlyList<DropTableEntry> entries))
            {
                Debug.LogWarning($"[ItemDropManager] 드랍 테이블을 찾지 못했습니다. dropTableId={request.DropTableId}");
                continue;
            }

            dropProvider.Resolve(entries, random, resolveBuffer);

            for (int i = 0; i < resolveBuffer.Count; i++)
            {
                DropResult result = resolveBuffer[i];
                Vector3 spawnPosition = GetScatteredPosition(request.Position, i, resolveBuffer.Count);

                // 이부분 추후 코드 관계 정리 필요
                //itemFacade.Spawn(result.ItemId, result.Amount, spawnPosition);

                spawnedThisFrame++;
            }
        }
    }

    /// <summary>
    /// 드랍 여러 개일 때 원형으로 흩뿌리기
    /// </summary>
    private Vector3 GetScatteredPosition(Vector3 origin, int index, int total)
    {
        if (total <= 1)
        {
            return origin;
        }

        float angle = Mathf.PI * 2f * index / total;
        return origin + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * ScatterRadius;
    }

    /// <summary>
    /// 큐에 담기는 요청 1건, 테이블아이디랑 위치지정, 이 구조체는 여기서만 씀
    /// </summary>
    private readonly struct DropRequest
    {
        public readonly int DropTableId;
        public readonly Vector3 Position;

        public DropRequest(int dropTableId, Vector3 position)
        {
            DropTableId = dropTableId;
            Position = position;
        }
    }
}
