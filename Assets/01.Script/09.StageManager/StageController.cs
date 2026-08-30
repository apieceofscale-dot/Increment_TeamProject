using UnityEngine;

public class StageController : MonoBehaviour, IBootStrapper
{
    [SerializeField] MonsterSpawner monsterSpawner;
    [SerializeField] StageStatus stageStatus;
    [SerializeField] int stageIndex = 1;
    [SerializeField] bool spawnOnStart = true;

    public void IBootStrapperInitialize(BootstrapContext context)
    {
        context.OnStepCompleted?.Invoke();
        if (spawnOnStart)
        {
            StartCoroutine(RunFirstWave());
        }
    }

    System.Collections.IEnumerator RunFirstWave()
    {
        if (monsterSpawner == null)
        {
            yield break;
        }

        var boot = FindFirstObjectByType<BootStrapper>();
        while (boot != null && !boot.IsBootCompleted)
        {
            yield return null;
        }

        var spawned = monsterSpawner.SpawnWave(stageIndex);
        if (stageStatus != null)
        {
            stageStatus.RegisterSpawned(spawned);
        }
    }
}
