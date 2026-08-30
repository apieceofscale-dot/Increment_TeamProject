using System.Collections.Generic;
using UnityEngine;

public class ItemObjectPoolManager : ObjectPoolManager<ItemController>, IBootStrapper
{
    [SerializeField] List<ItemController> prewarmPrefabs = new List<ItemController>();

    public void IBootStrapperInitialize(BootstrapContext context)
    {
        if (prewarmPrefabs != null && prewarmPrefabs.Count > 0)
        {
            MakeFirstPools(prewarmPrefabs);
        }

        context.OnStepCompleted?.Invoke();
    }
}
