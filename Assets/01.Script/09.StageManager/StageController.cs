using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// 스테이지 진입 이후의 일을 합니다
/// 1. 캐릭터 생성x 세션이 넘겨준 캐릭터 보관o
/// 2,. 스테이지 씬을 로드/언로드 + 맵 요소 확보
///3. 몬스터 스폰 지시/사망 받기 -> 집계/경험치/드랍 요청 진행
/// </summary>

public class StageController : MonoBehaviour, IBootStrapper
{
    public int BootOrder => (int)BootLayer.StageManager;

    private int startPlayerId = 1000;   // 테스트용
    private int startStageId = 9000;
    private bool autoStartOnBoot = true; // SaveManager 완성되면 false로 두고 세이브가 호출


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
    public EliteProgressInfo EliteProgress => status.ToEliteInfo();
    public StageChangedInfo StageInfo => BuildStageChangedInfo();
    public CharacterFacade Character => character;
    public bool IsTransitioning => isTransitioning;


    // ui버튼 활성 판단용, 실제 이동은 TryGoNextStage가 다시 검사
    public bool CanGoNextStage
    {
        get
        {
            if (isTransitioning) return false;
            if (!status.IsClearConditionMet) return false;

            int nextStageId = status.Definition.NextStageId;
            if (nextStageId == status.Definition.StageId) return false;   // 마지막 스테이지

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
            throw new System.InvalidOperationException(
                "[StageController] 오브젝트에 MonsterSpawner가 없습니다");
        }

        spawner.Initialize(monsterFactory);

        facade = GetComponent<StageFacade>();
        if (facade == null)
        {
            throw new System.InvalidOperationException(
                "[StageController] 오브젝트에 StageFacade가 없습니다.");
        }
        facade.Bind(this);

        MonsterFacade.MonsterDied += HandleMonsterDied;


    }

    private void OnDestroy()
    {
        MonsterFacade.MonsterDied -= HandleMonsterDied;
        OnStageReady = null;

    }


    // 외부 호출 api

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




    // 스테이지 넘김 조건 체크 
    public bool TryGoNextStage()
    {
        if (!CanGoNextStage) return false;

        EnterStage(status.Definition.NextStageId);
        return true;
    }

    // 엘리트 소환 조건 체크
    public bool TrySummonElite()
    {
        if (isTransitioning) return false;
        if (!status.ToEliteInfo().CanSummon) return false;

        activeElite = spawner.SpawnElite(status.Definition);
        if (activeElite == null) return false;

        status.BeginElite();
        return true;
    }

    public bool TryMoveToChallengeStage()
    {
        if (isTransitioning) return false;
        if (status.Definition.StageId == 0) return false;   // 아직 스테이지에 들어간 적 없음

        int chapter = status.Definition.Chapter;

        if (!stageTable.TryGetChallengeStageId(chapter, out int challengeStageId))
        {

            //스테이지쪽에 도전맵있어어ㅑ함
            Debug.LogWarning($"[StageController] 챕터 {chapter}에 도전맵(Boss 스테이지)이 없습니다.");
            return false;
        }



        // 이미 도전맵이면 재진입시키지 않는다(진행도 초기화 방지)
        if (challengeStageId == status.Definition.StageId) return false;

        // 클리어 조건을 걸고 싶으면 아래 줄을 살린다 (기획 확정 전까지는 항상 도전 허용)
        // if (!status.IsClearConditionMet) return false;

        EnterStage(challengeStageId);
        return true;
    }



    // 내부용 로직


    private IEnumerator StartStageRoutine(StageDefinition definition)
    {
        isTransitioning = true;

        // 1.이전 스테이지 정리
        spawner.DespawnAll();
        dropFacade.CancelPendingDrops();
        activeElite = null;
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
                    $"(Build Settings 등록 여부와 스펠링 확인하세요) stageId={definition.StageId}");
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
        spawner.SetMap(map);
        spawner.ResetTimer();

        // 4. z캐릭터 배치
        if (character != null && map.PlayerStart != null)
        {
            character.transform.position = map.PlayerStartPosition;
        }

        // 5. 상태 초기화
        status.Reset(definition);

        isTransitioning = false;


        // 6. 진입 끝난 후 알리기
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
        if (isTransitioning) return;
        if (status.State != StageState.Battle) return;


        float deltaTime = Time.deltaTime;

        spawner.TickSpawn(status.Definition, deltaTime);


        // 엘리트 제한시간 초과시 도망
        if (status.TickEliteTime(deltaTime))
        {
            if (activeElite != null)
            {
                spawner.Despawn(activeElite);
                activeElite = null;
            }
            status.EndElite();
        }

    }

    // 스테이지는 드랍 테이블 id만 알고, 테이블 내용과 확률 판정은 아이템 쪽이 
    private void HandleMonsterDied(MonsterDiedInfo info)
    {
        if (status.State != StageState.Battle) return;

        bool wasElite = activeElite != null && ReferenceEquals(activeElite, info.Source);

        // 드랍 —DropTableId가 아직 안 채워졌으면 MonsterId로 
        // 몬스터 담당이 DropTableId를 채우면 폴백 제거
        int dropTableId = info.DropTableId != 0 ? info.DropTableId : info.MonsterId;
        dropFacade.RequestDrop(dropTableId, info.Position);

        // 경험치
        if (character != null && info.ExpReward > 0)
        {
            character.GainExp(info.ExpReward);
        }

        if (wasElite)
        {
            activeElite = null;
            status.EndElite();
            return;   // 엘리트는 킬 카운트에 넣지 않는다
        }

        status.AddKill();

        if (!status.IsClearConditionMet && status.KillCount >= status.Definition.ClearKillCount)
        {
            status.CheckClearConditionMet();

            // 보스는 Cleared로 고정해 무한 리스폰을 막는다.
            // 파밍 스테이지는 Battle을 유지해 계속 파밍할 수 있게 둔다.
            if (status.Definition.Type == StageType.Boss)
            {
                status.SetState(StageState.Cleared);
                spawner.DespawnAll();
            }
        }
        // 클리어 순간에도 ui갱신 가능
        RaiseStageChanged();

    }

    private StageChangedInfo BuildStageChangedInfo()
    {
        StageDefinition definition = status.Definition;
        if (definition.StageId == 0) return default;

        int totalStageNum = stageTable.GetChapterStageCount(definition.Chapter);

        bool hasChallenge = stageTable.TryGetChallengeStageId(definition.Chapter, out int challengeStageId);
        int challengeStageNum = 0;
        if (hasChallenge && stageTable.TryGet(challengeStageId, out StageDefinition challenge))
        {
            challengeStageNum = challenge.IndexInChapter;
        }

        return new StageChangedInfo(
            definition.StageId,
            definition.Chapter,
            definition.DisplayName,
            definition.IndexInChapter,
            totalStageNum,
            definition.Type,
            challengeStageNum,
            hasChallenge && challengeStageId != definition.StageId);
    }

    private void RaiseStageChanged()
    {
        if (stageChangedChannel == null)
        {
            Debug.LogWarning("[StageController] StageChangedEventChannel이 인스펙터에 물려있지 않습니다.");
            return;
        }

        stageChangedChannel.Raise(BuildStageChangedInfo());
    }

}
