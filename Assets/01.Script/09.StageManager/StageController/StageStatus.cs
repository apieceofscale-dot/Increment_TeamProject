using UnityEngine;

public class StageStatus : MonoBehaviour
{
    public int RemainingMonsters { get; private set; }

    public void RegisterSpawned(int count)
    {
        RemainingMonsters += Mathf.Max(0, count);
    }

    public void NotifyMonsterDefeated()
    {
        RemainingMonsters = Mathf.Max(0, RemainingMonsters - 1);
    }

    public void ResetCount()
    {
        RemainingMonsters = 0;
    }
}
