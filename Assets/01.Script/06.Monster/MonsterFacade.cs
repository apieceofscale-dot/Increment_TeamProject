using System;
using UnityEngine;

/// <summary>
/// 몬스터 시스템 외부 API.
/// Factory로 생성하고, 사망 시 ItemDropFacade에 드랍을 요청한다.
/// </summary>
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

    // -------------------------------------------------------------------------
    // UI / StageManager 연동 API
    // 팀에서 요청하는 메서드는 이 region 아래에 추가한다.
    // -------------------------------------------------------------------------
    #region UI · Stage 연동

    /// <summary>몬스터 id로 스폰. StageManager·테스트 UI에서 호출.</summary>
    public MonsterController Spawn(int monsterId, Vector3 position, Quaternion rotation, int stageIndex = 1)
    {
        if (MonsterFactory.Instance == null)
        {
            Debug.LogWarning("[MonsterFacade] MonsterFactory is missing.");
            return null;
        }

        return MonsterFactory.Instance.Create(monsterId, position, rotation, stageIndex);
    }

    /// <summary>MonsterData(SO)로 스폰.</summary>
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

    /// <summary>몬스터를 풀에 반환.</summary>
    public void Despawn(MonsterController monster)
    {
        if (monster == null)
        {
            return;
        }

        monster.ReturnToPool();
    }

    // TODO(UI): StageManager 정예 소환 버튼 → Spawn(...) 연결
    // TODO(UI): 몬스터 처치 카운트 표시 → MonsterDied 이벤트 구독

    #endregion

    // -------------------------------------------------------------------------
    // 내부 이벤트
    // -------------------------------------------------------------------------

    public static void NotifyDied(in MonsterDiedInfo info)
    {
        MonsterDied?.Invoke(info);
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
