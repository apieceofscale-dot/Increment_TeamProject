using System;
using UnityEngine;

public class MonsterFacade : MonoBehaviour, IBootStrapper
{
    public static event Action<MonsterDiedInfo> MonsterDied;

    [SerializeField] MonsterController prefab;
    [SerializeField] ItemFacade itemFacade;

    public int BootOrder => (int)BootLayer.Monster;

    public void IBootStrapperInject(BootstrapContext context)
    {
        if (itemFacade == null)
        {
            context.TryGet(out itemFacade);
        }
    }

    public void IBootStrapperInitialize()
    {
        MonsterDied -= HandleMonsterDied;
        MonsterDied += HandleMonsterDied;
    }

    void OnEnable()
    {
        MonsterDied -= HandleMonsterDied;
        MonsterDied += HandleMonsterDied;
    }

    void OnDisable()
    {
        MonsterDied -= HandleMonsterDied;
    }

    public static void NotifyDied(in MonsterDiedInfo info)
    {
        MonsterDied?.Invoke(info);
    }

    public MonsterController Spawn(int monsterId, Vector3 position, Quaternion rotation, int stageIndex = 1)
    {
        if (prefab == null)
        {
            Debug.LogWarning("[MonsterFacade] prefab is missing.");
            return null;
        }

        var monster = Instantiate(prefab, position, rotation);
        monster.BindSpawn(monsterId, stageIndex);
        monster.OnSpawn();
        return monster;
    }

    public void Despawn(MonsterController monster)
    {
        if (monster == null)
        {
            return;
        }

        monster.ReturnToPool();
    }

    void HandleMonsterDied(MonsterDiedInfo info)
    {
        if (itemFacade != null)
        {
            itemFacade.DropFromMonster(info);
        }

        if (info.Source != null)
        {
            Despawn(info.Source);
        }
    }
}
