using UnityEngine;

public class ItemDropManager : MonoBehaviour, IBootStrapper
{

    public int BootOrder => (int)BootLayer.ItemManager;

    private readonly ItemDropProvider provider = new ItemDropProvider();


    public void IBootStrapperInject(BootstrapContext context)
    {

    }


    public void IBootStrapperInitialize()
    {

    }

    public void RequestDrop(int dropTableId, Vector3 position)
    {

    }

    // 프레임마다 몬스터 사망 처리 후 처리
    private void LateUpdate()
    {
        PickUpAll();
    }


    private void PickUpAll()
    {

    }


}
