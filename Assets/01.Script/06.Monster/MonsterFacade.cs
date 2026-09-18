using System;
using UnityEngine;

/// <summary>
/// 몬스터 시스템 외부 API.
/// Factory로 생성하고, 사망 시 ItemDropFacade에 드랍을 요청한다.
/// </summary>
/// <remarks>
/// [팀 전달 — 2025-09-16, 신현수 부재 시 참고]
/// ■ Facade/API: Spawn·Despawn·MonsterDied 이벤트 껍데기 구현됨.
/// ■ 동작함: MonsterFactory+풀링, AI FSM, 사망→RequestDrop→Despawn (ItemDropFacade 연결 시).
/// ■ 타팀·씬 연결:
///   - MonsterSpawner → MonsterFactory.Create 연결됨 (09). 맵 스폰 포인트 없으면 경고만.
///   - StageController MonsterDied 구독 → 킬/경험치 (드랍은 이 Facade만 RequestDrop).
///   - 씬 Inspector: MonsterFactory defaultPrefab, MonsterFacade→itemDropFacade, ItemDropFacade on ItemDropManager GO.
///   - Monster prefab에 Navi2DAgent 없으면 Trace 폴백(MoveTowards)만 동작.
///   - CSV→SO 임포트(Tools/ExcelTest) 안 하면 DataManager 데이터 비어 있음.
/// </remarks>
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

    // TODO(UI): 정예 소환 버튼 → Spawn(...)
    // TODO(UI): 몬스터 처치 카운트 → MonsterDied 이벤트 구독

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
            int dropTableId = info.DropTableId > 0 ? info.DropTableId : (int)info.MonsterId;
            itemDropFacade.RequestDrop(dropTableId, info.Position);
        }

        if (info.Source != null)
        {
            Despawn(info.Source);
        }
    }
}
