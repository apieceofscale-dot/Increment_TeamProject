using UnityEngine;

public class ItemDropFacade : MonoBehaviour
{
    private ItemDropManager dropManager;

    public int BootOrder => (int)BootLayer.ItemManager;

    private ItemFacade itemFacade;

    private void Awake()
    {
        if (dropManager == null)
        {
            dropManager = GetComponent<ItemDropManager>();
        }
    }

    // 아이템파사드주입
    public void IBootStrapperInject(BootstrapContext context)
    {
        itemFacade = context.Get<ItemFacade>();
    }

    public void IBootStrapperInitialize()
    {
        if (dropManager == null)
        {
            throw new System.InvalidOperationException(
                "[ItemDropFacade] ItemDropManager가 같은 오브젝트에 없습니다.");
        }

        if (itemFacade == null)
        {
            throw new System.InvalidOperationException(
                "[ItemDropFacade] ItemFacade를 주입받지 못했습니다. 씬에 ItemFacade가 있고 IBootStrapper를 구현했는지 확인하세요.");
        }

        dropManager.Initialize(new TempDropTableSource(), itemFacade);
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
