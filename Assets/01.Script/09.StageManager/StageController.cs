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



    public StageProgressInfo Progress => status.ToInfo();
    public EliteProgressInfo EliteProgress => status.ToEliteInfo();
    public CharacterFacade Character => character;
    public bool IsTransitioning => isTransitioning;



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
}
