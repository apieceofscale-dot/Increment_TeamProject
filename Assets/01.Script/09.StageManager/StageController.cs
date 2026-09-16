using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// 1. 캐릭터 만들어서 보관
/// 2,. 스테이지 씬을 로드/언로드
///3. 몬스터 사망 받아서 킬 집계 / 경험치 / 드랍요청
/// </summary>

public class StageController : MonoBehaviour, IBootStrapper
{
    public int BootOrder => (int)BootLayer.StageManager;

    private int startPlayerId = 1000;   // 테스트용
    private int startStageId = 9000;
    private bool autoStartOnBoot = true; // SaveManager 완성되면 false로 두고 세이브가 호출


    // 부트 주입
    private CharacterFactory characterFactory;
    private MonsterFactory monsterFactory;
    private ItemDropManager itemDropManager;


    // 초기화 확보
    private ItemDropFacade dropFacade;
    private MonsterSpawner spawner;
    private StageFacade facade;
    private CharacterFacade character;


    private readonly StageStatus status = new StageStatus();
    private readonly TempStageTable stageTable = new TempStageTable();


    private MonsterController activeElite;
    private string loadedSceneName;
    private bool isTransitioning;


    [SerializeField] private StageChangedEventChannelSO stageChangedChannel;

    public StageProgressInfo Progress => status.ToInfo();
    public EliteProgressInfo EliteProgress => status.ToEliteInfo();
    public CharacterFacade Character => character;
    public bool IsTransitioning => isTransitioning;

    public StageFacade Facade => facade;

    public StageChangedInfo StageInfo => BuildStageChangedInfo();



    public void IBootStrapperInject(BootstrapContext context)
    {
        characterFactory = context.Get<CharacterFactory>();
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

        CreateCharacter(startPlayerId);


        if (autoStartOnBoot)
        {
            StartStage(startStageId);
        }
    }

    private void OnDestroy()
    {
        //MonsterFacade.MonsterDied -= HandleMonsterDied;
    }


    // 외부 호출 (StageFacade통해)

    /// <summary>스테이지를 시작하고 씬이 다르면 씬 전환까지 처리</summary>
    public void StartStage(int stageId)
    {
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
        if (!status.IsClearConditionMet) return false;
        if (status.Definition.NextStageId == status.Definition.StageId) return false;

        StartStage(status.Definition.NextStageId);
        return true;
    }

    // 엘리트 소환 조건 체크
    public bool TrySummonElite()
    {
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

        StartStage(challengeStageId);
        return true;
    }



    // 내부용 로직

    private void CreateCharacter(int playerId)
    {

        //스테이지가 바뀔 때는ㄴ위치만 플레이어스타트로 옮기면 된다
        CharacterControllers controllers = characterFactory.Create(playerId, Vector3.zero);
        if (controllers == null)
        {
            Debug.LogError($"[StageController] 캐릭터 생성 실패. playerId={playerId}");
            return;
        }

        // 파사드말고 컨트롤러 ok
        character = controllers.GetComponent<CharacterFacade>();
        if (character == null)
        {
            Debug.LogError("[StageController] 캐릭터 프리팹에 CharacterFacade가 없습니다.");
        }
    }
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

        // StageMapParts map = provider.ToParts();
        // spawner.SetMap(map);
        // spawner.ResetTimer();

        // 4. z캐릭터 배치
        // if (character != null)
        // {
        //     character.transform.position = map.PlayerStartPosition;
        // }

        // 5. 상태 초기화
        status.Reset(definition);

        isTransitioning = false;


        // 6. 진입 끝난 후 알리기
        RaiseStageChanged();
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
                // monsterFactory.Despawn(activeElite);
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
