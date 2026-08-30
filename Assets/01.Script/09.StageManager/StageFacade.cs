using UnityEngine;

public class StageFacade : MonoBehaviour, IBootStrapper
{
    [SerializeField] StageStatus stageStatus;

    public void IBootStrapperInitialize(BootstrapContext context)
    {
        MonsterFacade.MonsterDied -= HandleMonsterDied;
        MonsterFacade.MonsterDied += HandleMonsterDied;
        context.OnStepCompleted?.Invoke();
    }

    void OnDisable()
    {
        MonsterFacade.MonsterDied -= HandleMonsterDied;
    }

    void HandleMonsterDied(MonsterDiedInfo info)
    {
        if (stageStatus != null)
        {
            stageStatus.NotifyMonsterDefeated();
        }
    }
}
