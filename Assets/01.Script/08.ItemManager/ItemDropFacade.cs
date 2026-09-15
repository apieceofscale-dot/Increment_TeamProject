using UnityEngine;

public class ItemDropFacade : MonoBehaviour
{
    private ItemDropManager dropManager;

    /// <summary>ItemDropManager가 부트 초기화 끝에 호출하고  외부에서 부르지 말 것.
    public void Bind(ItemDropManager manager)
    {
        dropManager = manager;
    }


    private ItemFacade itemFacade;

    private void Awake()
    {
        if (dropManager == null)
        {
            dropManager = GetComponent<ItemDropManager>();
        }
    }



    /// <summary>
    /// 드랍 요청은 큐에 쌓이고 이번 프레임 LateUpdate에 처리됩니다
    /// 호출은 이후 작업할 스테이지매니저가 할 예정이고 일단은 TempMonsterDropBridge가 대신 호출
    /// </summary>
    public void RequestDrop(int dropTableId, Vector3 position)
    {
        dropManager.Enqueue(dropTableId, position);
    }

    /// <summary>
    /// 아직 스폰되지 않은 드랍 요청을 모두 취소합니다
    /// 스테이지 전환 시 이전 스테이지 드랍이 새 스테이지에 튀어나오는 것을 막기 위해
    /// </summary>
    public void CancelPendingDrops()
    {
        dropManager.ClearPending();
    }
}
