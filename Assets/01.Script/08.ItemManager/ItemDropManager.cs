using UnityEngine;

public class ItemDropManager : MonoBehaviour, IBootStrapper
{
    public int LastPickedItemId { get; private set; }
    public int LastPickedValue { get; private set; }

    public void IBootStrapperInitialize(BootstrapContext context)
    {
        ItemFacade.ItemPickedUp -= HandlePickedUp;
        ItemFacade.ItemPickedUp += HandlePickedUp;
        context.OnStepCompleted?.Invoke();
    }

    void OnDisable()
    {
        ItemFacade.ItemPickedUp -= HandlePickedUp;
    }

    void HandlePickedUp(ItemPickedUpInfo info)
    {
        LastPickedItemId = info.ItemId;
        LastPickedValue = info.Value;
    }
}
