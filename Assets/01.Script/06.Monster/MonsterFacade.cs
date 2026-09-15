using System;
using UnityEngine;

public class MonsterFacade : MonoBehaviour
{
    public static event Action<MonsterDiedInfo> MonsterDied;

    [SerializeField] ItemDropFacade itemDropFacade;

    void Awake()
    {
        if (itemDropFacade == null)
        {
            itemDropFacade = FindFirstObjectByType<ItemDropFacade>();
        }
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
        if (MonsterFactory.Instance == null)
        {
            Debug.LogWarning("[MonsterFacade] MonsterFactory is missing.");
            return null;
        }

        return MonsterFactory.Instance.Create(monsterId, position, rotation, stageIndex);
    }

    public MonsterController Spawn(MonsterData data, Vector3 position, Quaternion rotation, int stageIndex = 1)
    {
        if (MonsterFactory.Instance == null)
        {
            Debug.LogWarning("[MonsterFacade] MonsterFactory is missing.");
            return null;
        }

        if (data == null)
        {
            Debug.LogWarning("[MonsterFacade] monster data is missing.");
            return null;
        }

        return MonsterFactory.Instance.Create(data.id, position, rotation, stageIndex);
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
        if (itemDropFacade != null)
        {
            itemDropFacade.RequestDrop(info.MonsterId, info.Position);
        }

        if (info.Source != null)
        {
            Despawn(info.Source);
        }
    }
}
