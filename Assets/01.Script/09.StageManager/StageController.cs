using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Collections;

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


    // [변경] 몬스터/아이템 생성 시스템은 현재 게임에서 사용하지 않으므로
    // StageController의 필수 부트 의존성에서 제외한다.
    // private MonsterFactory monsterFactory;
    // private ItemDropManager itemDropManager;


    // 초기화 확보
    // [변경] 아이템 드랍 시스템을 다시 사용할 때 복구한다.
    // private ItemDropFacade dropFacade;
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
        // [변경] 현재 StageController에는 필수로 주입받을 외부 시스템이 없다.
        // 아래 두 시스템이 없어도 부트 시퀀스와 스테이지 진입이 계속되어야 한다.
        // monsterFactory = context.Get<MonsterFactory>();
        // itemDropManager = context.Get<ItemDropManager>();
    }

    public void IBootStrapperInitialize()
    {
        // [변경] MonsterSpawner는 현재 사용하지 않지만 기존 배치를 확인할 수 있도록 참조만 보관한다.
        spawner = GetComponent<MonsterSpawner>();
        // [변경] MonsterFactory가 제거되어 초기화할 수 없으므로 기존 필수 초기화는 보존만 한다.
        // [변경2 - 효림] 경고만 하고 이후 진행되도록 유지하겠습니다. 
        if (spawner == null)
        {
            Debug.LogWarning("[StageController] MonsterSpawner가 없습니다. 몬스터가 스폰되지 않습니다.", this);
        }
        if (!spawner.Initialize())
        {
            spawner = null;   // 프리팹 누락 → 스폰 비활성(null 체크 경로로 동작)
        }




        facade = GetComponent<StageFacade>();
        if (facade == null)
        {
            // [추가] StageFacade는 세션과 UI가 StageController에 접근하는 필수 창구이므로
            // 메인 씬 배치에서 빠졌을 때 같은 오브젝트에 자동으로 보완한다.
            facade = gameObject.AddComponent<StageFacade>();

            // [변경] Facade 누락으로 전체 부트를 중단하던 기존 처리는 보존한다.
            // throw new InvalidOperationException(
            //     "[StageController] 오브젝트에 StageFacade가 없습니다.");
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
        if (!CanGoNextStage) return false;

        EnterStage(status.Definition.NextStageId);
        return true;
    }




    // 내부용 로직----------------------------


    private IEnumerator StartStageRoutine(StageDefinition definition)
    {
        isTransitioning = true;
        RaiseStageChanged();   // 전환 시작 → UI가 도전 버튼을 바로 잠그도록 (연타 방지)

        // 이전 씬의 PathFinder는 곧 언로드되어 파괴된다. 언로드 전에 끊어서
        // 전환 중에 캐릭터가 파괴된 PathFinder로 길찾기를 시도하지 않게 한다.
        if (character != null) character.SetPathFinder(null);

        // 1.이전 스테이지 정리
        // [변경] 몬스터 스폰/아이템 드랍 시스템이 배치된 경우에만 이전 상태를 정리한다.
        if (spawner != null) spawner.DespawnAll();
        // dropFacade.CancelPendingDrops();
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

        //  3.맵 요소 확보
        if (!TryFindMapProvider(loadedSceneName, out StageMapProvider provider))
        {
            Debug.LogError($"[StageController] 씬에 StageMapProvider가 없습니다. sceneName={loadedSceneName}");
            status.SetState(StageState.Failed);
            isTransitioning = false;
            yield break;
        }

        StageMapParts map = provider.ToParts();
        // [변경] MonsterSpawner는 현재 선택 기능이므로 존재할 때만 맵 정보를 전달한다.
        if (spawner != null)
        {
            spawner.SetMap(map);
            spawner.ResetTimer();
        }

        // 4. z캐릭터 배치
        if (character != null && map.PlayerStart != null)
        {
            character.transform.position = map.PlayerStartPosition;
        }

        // 씬마다 PathFinder가 다르므로 씬을 로드한 쪽(스테이지)이 새 맵의 PathFinder를 넣어준다. (규칙 6)
        // 캐릭터는 스테이지 로드 전에 메인 씬에서 생성되므로 Navi2DAgent.Awake에서는 찾지 못한다.
        if (character != null) character.SetPathFinder(map.PathFinder);

        // 5. 상태 초기화(State = Battle → Update에서 스폰 시작)
        status.Reset(definition);
        isTransitioning = false;


        // 6. 진입 완료 알림 (UI 갱신 → 세션 HUD 연결 순)
        RaiseStageChanged();
        OnStageReady?.Invoke(facade);
    }

    // 로드된 씬 안에서만 StageMapProvider를 찾아서 로드 직후 1회만 돈다
    private bool TryFindMapProvider(string sceneName, out StageMapProvider provider)
    {
        provider = null;
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.IsValid() || !scene.isLoaded) return false;

        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            provider = roots[i].GetComponentInChildren<StageMapProvider>(true);
            if (provider != null) return true;
        }

        return false;

    }


    private void Update()
    {
        // 스폰은 파밍/보스 전투 중에만. Cleared(보스 처치 후)·None(전환 중)·Failed에서는 멈춘다.
        if (isTransitioning) return;
        if (status.State != StageState.Battle) return;


        // [변경] MonsterFactory가 없는 현재 구조에서는 자동 몬스터 생성을 실행하지 않는다.
        // spawner.TickSpawn(status.Definition, Time.deltaTime);

        if (spawner != null) spawner.TickSpawn(status.Definition, Time.deltaTime);

    }

    // 몬스터 사망 수신. 스테이지는 드랍 테이블 id만 알고, 테이블 내용과 확률 판정은 아이템 쪽이 한다.
    private void HandleMonsterDied(MonsterDiedInfo info)
    {
        // ★ 죽은 몬스터 정리는 스테이지가 한다. 씬에 MonsterFacade가 없으므로 여기서 안 하면
        //   시체가 남고 CountAlive가 줄지 않아 스폰이 멈춘다.
        // [변경] MonsterSpawner가 없어도 씬에 배치된 몬스터 사망 처리는 완료한다.
        if (info.Source != null)
        {
            if (spawner != null) spawner.Despawn(info.Source);
            else info.Source.ReturnToPool();
        }

        if (status.State != StageState.Battle) return;

        // 드랍 — DropTableId가 아직 안 채워졌으면 MonsterId로 폴백 (몬스터 담당이 채우면 폴백 제거)
        // [변경] ItemFactory와 드랍 시스템을 현재 사용하지 않으므로 드랍 요청을 중단한다.
        // int dropTableId = info.DropTableId != 0 ? info.DropTableId : info.MonsterId;
        // dropFacade.RequestDrop(dropTableId, info.Position);

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
                // [변경] MonsterSpawner가 없는 씬에서도 보스 클리어 처리가 계속된다.
                if (spawner != null) spawner.DespawnAll();
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

        // [변경] 챕터 보스가 아닌 NextStageId가 가리키는 실제 다음 스테이지를 조회한다.
        // 마지막 스테이지의 자기 자신 참조는 다음 스테이지 없음으로 처리한다.
        int nextStageId = 0;
        string nextStageName = string.Empty;
        if (definition.NextStageId > 0 && definition.NextStageId != definition.StageId
            && stageTable.TryGet(definition.NextStageId, out StageDefinition nextStage))
        {
            nextStageId = nextStage.StageId;
            nextStageName = nextStage.DisplayName;
        }

        return new StageChangedInfo(
            definition.StageId,
            definition.Chapter,
            definition.DisplayName,
            definition.IndexInChapter,
            totalStageNum,
            definition.Type,
            challengeStageNum,
            // [변경] 기존 버튼 활성 조건을 유지하면서 다음 스테이지 정보도 전달한다.
            // CanGoNextStage);
            CanGoNextStage, nextStageId, nextStageName);   // ★ 도전 버튼 활성 여부 = 다음 스테이지로 갈 수 있는가
    }

    private void RaiseStageChanged()
    {
        if (stageChangedChannel == null) return;   // 누락 경고는 초기화 때 1회만
        stageChangedChannel.Raise(BuildStageChangedInfo());
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
