using System;
using UnityEngine;

public class MonsterFacade : MonoBehaviour
{
    public static event Action<MonsterDiedInfo> MonsterDied;

    [SerializeField] MonsterFactory monsterFactory;
    [SerializeField] ItemFacade itemFacade;

    void OnEnable()
    {
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

    void HandleMonsterDied(MonsterDiedInfo info)
    {
        if (itemFacade != null)
        {
            itemFacade.DropFromMonster(info);
        }

        if (info.Source == null)
        {
            return;
        }

        if (monsterFactory != null)
        {
            monsterFactory.Despawn(info.Source);
        }
        else
        {
            info.Source.ReturnToPool();
        }
    }
}
