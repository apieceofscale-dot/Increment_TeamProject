using System.Collections.Generic;
using UnityEngine;

public class MonsterObjectPoolManager : ObjectPoolManager<MonsterController>, IBootStrapper
{
    [SerializeField] List<MonsterController> prewarmPrefabs = new List<MonsterController>();

    public void IBootStrapperInitialize(BootstrapContext context)
    {
        if (prewarmPrefabs != null && prewarmPrefabs.Count > 0)
        {
            MakeFirstPools(prewarmPrefabs);
        }

        context.OnStepCompleted?.Invoke();
    }
}
