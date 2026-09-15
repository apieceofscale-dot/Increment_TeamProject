using UnityEngine;
using System;
using UnityEngine.SceneManagement;

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
    private void CreateCharacter(int playerId)
    {

    }


    public void StartStage(int stageId)
    {


        spawner.DespawnAll();
        activeElite = null;

    }
}
