using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Tilemaps;

/// <summary>
/// [역할] 스테이지 진입 이후의 흐름을 맡는 스테이지 내부 허브(Controller)
///  1. 세션이 넘겨준 캐릭터를 보관한다 (생성은 GameSessionManager 책임)
///  2. 스테이지 씬을 Additive로 로드/언로드하고 맵 요소(StageMapProvider)를 확보한다
///  3. 몬스터 스폰 지시(MonsterSpawner) / 사망 수신 → 반납 → 킬 집계 → 클리어 판정
///  4. [도전] 버튼 → NextStageId로 이동 (001 → 002 → 003 → 보스맵)
/// 
/// [진행 규칙]
/// - 파밍: ClearKillCount(10) 달성 → 도전 가능(버튼도 활성화). 달성 후에도 파밍(스폰)은 계속된다.
///  - 도전: 다음 스테이지로 이동. 003의 다음이 보스맵이라 "보스맵 이동"도 같은 경로.
///  - 보스: 1마리 처치 → Cleared 고정, 스폰 중단. 다음 스테이지 없음.
/// 
/// 
/// [구조] 외부(UI·세션)는 StageFacade만 부른다. 이 클래스는 세션을 참조하지 않고 이벤트로 보고한다.
/// - OnStageReady (C# event, 받는 쪽 = 세션 하나)  : 진입 완료 -> HUD 생성 (1회)
///  - StageChangedEventChannelSO (SO, 받는 쪽 = UI) : 스테이지 정보·킬 수·도전 가능 여부 갱신
/// </summary>

public class StageController : MonoBehaviour, IBootStrapper
{
    public int BootOrder => (int)BootLayer.StageManager;


    // 부트 주입
    //private CharacterFactory characterFactory;
    private MonsterFactory monsterFactory;
    private ItemDropManager itemDropManager;


    // 초기화 확보
    private ItemDropFacade dropFacade;
    private MonsterSpawner spawner;
    private StageFacade facade;

    // 세션 통해서 받을것
    private CharacterFacade character;


    private readonly StageStatus status = new StageStatus();
    private readonly TempStageTable stageTable = new TempStageTable(); // 스테이지csv생기면 데이터매니저 조회로 교체해야 하는데...


    private MonsterController activeElite;
    private string loadedSceneName;
    private bool isTransitioning;


    [SerializeField] private StageChangedEventChannelSO stageChangedChannel;

    //스테이지 진입이 실제로 끝났을 때(씬로드 맵확보 상태초기화 완료후 ) 발행
    // 세션은 이 이벤트 받고 ui붙
    public event Action<StageFacade> OnStageReady;


    public StageFacade Facade => facade;
    public StageProgressInfo Progress => status.ToInfo();
    //public EliteProgressInfo EliteProgress => status.ToEliteInfo();
    public StageChangedInfo StageInfo => BuildStageChangedInfo();
    public CharacterFacade Character => character;
    public bool IsTransitioning => isTransitioning;


    /// <summary>
    /// [도전] 버튼 활성 조건. UI 표시용이며 실제 이동 시 TryGoNextStage가 한 번 더 검사한다.
    /// 전환 중 아님 + 파밍 중 + 10킬 달성 + 다음 스테이지가 존재.
    /// 도전 조건
    /// </summary>
    public bool CanGoNextStage
    {
        get
        {
            if (isTransitioning) return false;
            if (status.State != StageState.Battle) return false;
            if (!status.IsClearConditionMet) return false;

            int nextStageId = status.Definition.NextStageId;
            if (nextStageId == status.Definition.StageId) return false;   // 마지막(보스) 스테이지

            return stageTable.TryGet(nextStageId, out _);
        }
    }

    public void IBootStrapperInject(BootstrapContext context)
    {

        monsterFactory = context.Get<MonsterFactory>();
        itemDropManager = context.Get<ItemDropManager>();
    }

    public void IBootStrapperInitialize()
    {
        dropFacade = itemDropManager.Facade;

        spawner = GetComponent<MonsterSpawner>();
        if (spawner == null)
        {
            throw new InvalidOperationException(
                "[StageController] 오브젝트에 MonsterSpawner가 없습니다");
        }

        spawner.Initialize(monsterFactory);

        facade = GetComponent<StageFacade>();
        if (facade == null)
        {
            throw new InvalidOperationException(
                "[StageController] 오브젝트에 StageFacade가 없습니다.");
        }
        facade.Bind(this);

        // 채널 누락은 게임이 못 도는 문제는 아니므로 예외 대신 경고 1회 (부트 전체를 멈추지 않게)
        if (stageChangedChannel == null)
            Debug.LogWarning("[StageController] StageChangedEventChannel이 인스펙터에 물려있지 않습니다. UI가 갱신되지 않습니다.", this);

        MonsterFacade.MonsterDied += HandleMonsterDied;
    }

    private void OnDestroy()
    {
        MonsterFacade.MonsterDied -= HandleMonsterDied;
        OnStageReady = null;
    }


    // ── 외부 API (StageFacade / 세션이 호출) ─────────────

    // 세션이 캐릭터를 만든 뒤 1회 호출, 전환마다 다시 부를 필요 x. 캐릭터는 ddol
    public void SetCharacter(CharacterFacade characterFacade)
    {
        if (characterFacade == null)
        {
            Debug.LogError("[StageController] SetCharacter에 null이 들어왔습니다.");
            return;
        }

        character = characterFacade;
    }

    //[임시]! 캐릭터 팩토리가 캐릭터파사드를 반환하도록 바뀌면 이 오버로드 삭제
    public void SetCharacter(CharacterControllers controllers)
    {
        if (controllers == null)
        {
            Debug.LogError("[StageController] SetCharacter에 null이 들어왔습니다.");
            return;
        }

        CharacterFacade characterFacade = controllers.GetComponent<CharacterFacade>();
        if (characterFacade == null)
        {
            Debug.LogError("[StageController] 캐릭터 프리팹에 CharacterFacade가 없습니다.");
            return;
        }

        SetCharacter(characterFacade);
    }

    /// 세션, 세이브, 스테이지 선택 ui가 쓰는 진입점
    public void EnterStage(int stageId)
    {
        if (character == null)
        {
            Debug.LogError("[StageController] SetCharacter가 먼저 호출되어야 합니다. stageId=" + stageId);
            return;
        }

        if (isTransitioning)
        {
            Debug.LogWarning("[StageController] 전환 중에는 스테이지를 바꿀 수 없습니다.");
            return;
        }

        if (!stageTable.TryGet(stageId, out StageDefinition definition))
        {
            Debug.LogError($"[StageController] 스테이지 정의를 찾지 못했습니다. stageId={stageId}");
            return;
        }

        StartCoroutine(StartStageRoutine(definition));
    }



    /// <summary>[도전] 버튼. 조건 미달이면 false, 이동을 시작했으면 true.
    /// 도전 조건 체크</summary>
    public bool TryGoNextStage()
    {
        if (isTransitioning)
        {
            Debug.LogWarning("[StageController] 전환 중에는 다음 스테이지로 이동할 수 없습니다.");
            return false;
        }

        if (status.Definition.StageId == 0)
        {
            Debug.LogWarning("[StageController] 아직 스테이지에 진입하지 않았습니다.");
            return false;
        }

        if (!status.IsClearConditionMet)
        {
            Debug.LogWarning(
                $"[StageController] 클리어 조건 미달 (킬 {status.KillCount}/{status.Definition.ClearKillCount}).");
            return false;
        }

        if (status.Definition.NextStageId == status.Definition.StageId)
        {
            Debug.LogWarning("[StageController] 다음 스테이지가 없습니다.");
            return false;
        }

        EnterStage(status.Definition.NextStageId);
        return true;
    }


    // // 일단 없는셈치고 별도 조건 없이 선형 진행 하겠습니다... 삭제
    //     public bool TryMoveToChallengeStage()
    //     {
    //         if (isTransitioning) return false;
    //         if (status.Definition.StageId == 0) return false;   // 아직 스테이지에 들어간 적 없음

    //         int chapter = status.Definition.Chapter;

    //         if (!stageTable.TryGetChallengeStageId(chapter, out int challengeStageId))
    //         {

    //             //스테이지쪽에 도전맵있어어ㅑ함
    //             Debug.LogWarning($"[StageController] 챕터 {chapter}에 도전맵(Boss 스테이지)이 없습니다.");
    //             return false;
    //         }

    //         // 이미 도전맵이면 재진입시키지 않는다(진행도 초기화 방지)
    //         if (challengeStageId == status.Definition.StageId) return false;

    //         // 클리어 조건을 걸고 싶으면 아래 줄을 살린다 (기획 확정 전까지는 항상 도전 허용)
    //         // if (!status.IsClearConditionMet) return false;

    //         EnterStage(challengeStageId);
    //         return true;
    //     }



    // 내부용 로직----------------------------


    private IEnumerator StartStageRoutine(StageDefinition definition)
    {
        isTransitioning = true;
        RaiseStageChanged();   // 전환 시작 → UI가 도전 버튼을 바로 잠그도록 (연타 방지)

        // 이전 씬의 PathFinder는 곧 언로드되어 파괴된다. 언로드 전에 끊어서
        // 전환 중에 캐릭터가 파괴된 PathFinder로 길찾기를 시도하지 않게 한다.
        // TODO(자동사냥 머지 후 주석 해제)
        // if (character != null) character.SetPathFinder(null);

        // 1.이전 스테이지 정리
        spawner.DespawnAll();
        dropFacade.CancelPendingDrops();
        status.SetState(StageState.None);

        // 2.씬 전환(같은 씬이면 로딩 없이 건너뛰기)
        if (definition.SceneName != loadedSceneName)
        {
            if (!string.IsNullOrEmpty(loadedSceneName))
            {
                yield return SceneManager.UnloadSceneAsync(loadedSceneName);
            }

            yield return SceneManager.LoadSceneAsync(definition.SceneName, LoadSceneMode.Additive);

            Scene loaded = SceneManager.GetSceneByName(definition.SceneName);
            if (!loaded.IsValid() || !loaded.isLoaded)
            {
                Debug.LogError(
                    $"[StageController] 씬 로드 실패. sceneName={definition.SceneName} " +
                    $"(Build Profiles 등록 여부와 스펠링 확인) stageId={definition.StageId}");
                status.SetState(StageState.Failed);
                isTransitioning = false;
                yield break;
            }

            loadedSceneName = definition.SceneName;
        }

        //  3.맵 요소 확보 (StageMapProvider 우선, 없으면 Stage01 레거시 Navi2D 씬 폴백)
        if (!TryResolveMapParts(loadedSceneName, out StageMapParts map))
        {
            Debug.LogWarning(
                $"[StageController] StageMapProvider/Navi2D를 찾지 못했습니다. sceneName={loadedSceneName}. " +
                "타일맵·Navi2DGridData·PathFinder·Link 구성을 확인하세요.");
            status.SetState(StageState.Failed);
            isTransitioning = false;
            yield break;
        }
        spawner.SetMap(map);
        spawner.ResetTimer();

        // 4. z캐릭터 배치
        if (character != null && map.PlayerStart != null)
        {
            character.transform.position = map.PlayerStartPosition;
        }

        // 씬마다 PathFinder가 다르므로 씬을 로드한 쪽(스테이지)이 새 맵의 PathFinder를 넣어준다. (규칙 6)
        // 캐릭터는 스테이지 로드 전에 메인 씬에서 생성되므로 Navi2DAgent.Awake에서는 찾지 못한다.
        // TODO(자동사냥 머지 후 주석 해제)
        // if (character != null) character.SetPathFinder(map.PathFinder);

        // 5. 상태 초기화(State = Battle → Update에서 스폰 시작)
        status.Reset(definition);
        isTransitioning = false;


        // 6. 진입 완료 알림 (UI 갱신 → 세션 HUD 연결 순)
        RaiseStageChanged();
        OnStageReady?.Invoke(facade);
    }

    private bool TryResolveMapParts(string sceneName, out StageMapParts map)
    {
        map = default;
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.IsValid() || !scene.isLoaded)
            return false;

        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            StageMapProvider provider = roots[i].GetComponentInChildren<StageMapProvider>(true);
            if (provider == null)
                continue;

            map = provider.ToParts();
            return map.PathFinder != null;
        }

        Navi2DPathFinder pathFinder = null;
        Tilemap ground = null;
        Transform playerStart = null;
        Transform mapRoot = null;

        for (int i = 0; i < roots.Length; i++)
        {
            if (pathFinder == null)
                pathFinder = roots[i].GetComponentInChildren<Navi2DPathFinder>(true);

            if (ground == null)
                ground = roots[i].GetComponentInChildren<Tilemap>(true);

            if (playerStart == null)
                playerStart = FindTransformByName(roots[i].transform, "PlayerStart");
        }

        if (pathFinder == null)
            return false;

        mapRoot = pathFinder.transform;
        if (playerStart == null)
            playerStart = mapRoot;

        Transform[] spawnPoints = CollectSpawnPointsFromScene(roots);

        map = new StageMapParts(
            mapRoot,
            ground,
            pathFinder,
            playerStart,
            spawnPoints,
            null);

        Debug.LogWarning(
            $"[StageController] StageMapProvider 없음 → Navi2DPathFinder 폴백 사용. scene={sceneName}",
            pathFinder);
        return true;
    }

    private static Transform FindTransformByName(Transform root, string objectName)
    {
        if (root.name == objectName)
            return root;

        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindTransformByName(root.GetChild(i), objectName);
            if (found != null)
                return found;
        }

        return null;
    }

    private static Transform[] CollectSpawnPointsFromScene(GameObject[] roots)
    {
        var points = new System.Collections.Generic.List<Transform>();
        for (int i = 0; i < roots.Length; i++)
            CollectSpawnPointsRecursive(roots[i].transform, points);

        return points.Count > 0 ? points.ToArray() : System.Array.Empty<Transform>();
    }

    private static void CollectSpawnPointsRecursive(Transform node, System.Collections.Generic.List<Transform> points)
    {
        if (node.name.StartsWith("SpawnPoint", System.StringComparison.OrdinalIgnoreCase))
            points.Add(node);

        for (int i = 0; i < node.childCount; i++)
            CollectSpawnPointsRecursive(node.GetChild(i), points);
    }


    private void Update()
    {
        // 스폰은 파밍/보스 전투 중에만. Cleared(보스 처치 후)·None(전환 중)·Failed에서는 멈춘다.
        if (isTransitioning) return;
        if (status.State != StageState.Battle) return;


        spawner.TickSpawn(status.Definition, Time.deltaTime);

    }

    // 몬스터 사망 수신. 스테이지는 드랍 테이블 id만 알고, 테이블 내용과 확률 판정은 아이템 쪽이 한다.
    private void HandleMonsterDied(MonsterDiedInfo info)
    {
        // ★ 죽은 몬스터 정리는 스테이지가 한다. 씬에 MonsterFacade가 없으므로 여기서 안 하면
        //   시체가 남고 CountAlive가 줄지 않아 스폰이 멈춘다.
        if (info.Source != null) spawner.Despawn(info.Source);

        if (status.State != StageState.Battle) return;

        int dropTableId = info.DropTableId != 0 ? info.DropTableId : (int)info.MonsterId;
        dropFacade.RequestDrop(dropTableId, info.Position);

        // 경험치 (MonsterData에 exp 필드가 생기기 전까지는 0이라 실제로는 안 오름)
        if (character != null && info.ExpReward > 0)
        {
            character.GainExp(info.ExpReward);
        }

        status.AddKill();

        // 도전 조건
        if (!status.IsClearConditionMet && status.KillCount >= status.Definition.ClearKillCount)
        {
            status.CheckClearConditionMet();   // → CanGoNextStage = true → 도전 버튼 활성

            // 보스는 Cleared로 고정해 추가 스폰을 막는다.
            // 파밍 스테이지는 Battle을 유지해 도전 버튼을 누르기 전까지 계속 파밍할 수 있게 둔다.
            if (status.Definition.Type == StageType.Boss)
            {
                status.SetState(StageState.Cleared);
                spawner.DespawnAll();
                // TODO(기획): 보스 클리어 후 흐름 (다음 챕터 / 결과창 / 003 복귀) 확정되면 여기서 처리
            }
        }

        // 킬마다 발행 → UI의 킬 진행도와 도전 버튼 갱신
        RaiseStageChanged();

    }

    private StageChangedInfo BuildStageChangedInfo()
    {
        StageDefinition definition = status.Definition;
        if (definition.StageId == 0) return default;   // 아직 한 번도 진입하지 않음

        int totalStageNum = stageTable.GetChapterStageCount(definition.Chapter);

        // UI의 "보스 스테이지 번호" 표시용
        int challengeStageNum = 0;
        if (stageTable.TryGetChallengeStageId(definition.Chapter, out int bossStageId)
            && stageTable.TryGet(bossStageId, out StageDefinition boss))
        {
            challengeStageNum = boss.IndexInChapter;
        }

        return new StageChangedInfo(
            definition.StageId,
            definition.Chapter,
            definition.DisplayName,
            definition.IndexInChapter,
            totalStageNum,
            definition.Type,
            challengeStageNum,
            CanGoNextStage);   // ★ 도전 버튼 활성 여부 = 다음 스테이지로 갈 수 있는가
    }

    private void RaiseStageChanged()
    {
        if (stageChangedChannel == null)
            return;

        StageChangedInfo info = BuildStageChangedInfo();
        stageChangedChannel.Raise(info);
        facade?.NotifyStageChanged(info);
    }


#if UNITY_EDITOR
    // [테스트용] HUD가 아직 안 붙었을 때 인스펙터 ⋮ 메뉴에서 도전 흐름만 확인. 에디터 빌드에서만 존재.
    [ContextMenu("Test/10킬 채우기")]
    private void TestFillKills()
    {
        if (!Application.isPlaying || status.State != StageState.Battle) return;
        while (!status.IsClearConditionMet && status.KillCount < status.Definition.ClearKillCount)
        {
            status.AddKill();
        }
        status.CheckClearConditionMet();
        RaiseStageChanged();
        Debug.Log($"[StageController] 테스트: {status.Definition.StageId} 10킬 처리. CanGoNextStage={CanGoNextStage}");
    }

    [ContextMenu("Test/도전 (다음 스테이지)")]
    private void TestGoNext()
    {
        if (!Application.isPlaying) return;
        Debug.Log($"[StageController] 테스트: TryGoNextStage = {TryGoNextStage()}");
    }
#endif

}
