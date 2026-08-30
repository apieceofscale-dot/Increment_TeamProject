using UnityEngine;

public class ItemDropFacade : MonoBehaviour, IBootStrapper
{
    [SerializeField] ItemDropManager itemDropManager;

    public void IBootStrapperInitialize(BootstrapContext context)
    {
        if (itemDropManager == null)
        {
            itemDropManager = FindFirstObjectByType<ItemDropManager>();
        }

        context.OnStepCompleted?.Invoke();
    }
}
