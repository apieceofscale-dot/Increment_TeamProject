using UnityEngine;

public class GameSessionManager : MonoBehaviour, IBootStrapper
{
    [SerializeField] Transform spawnPosition;
    //스테이지 완료so 구독.

    public int BootOrder => (int)BootLayer.GameSessionManager;
        
    CharacterFactory characterFactory;
    UIManager uiManager;
    StageController stageManager;

    private CharacterControllers currentCharacter;
    private CharacterFacade currentCharacterFacade;
    private StageFacade currentStageFacade;

    public void IBootStrapperInject(BootstrapContext context)
    {       
        characterFactory = context.Get<CharacterFactory>();        
        uiManager = context.Get<UIManager>();
        stageManager = context.Get<StageController>(); 
    }

    public void IBootStrapperInitialize()
    {
        uiManager.OnCharacterSelected += HandleCharacterSelected;

        //stageManager.OnStageReady += HandleStageReady; 스테이지 완료 이벤트가 필요합니다. 이름 이걸로 지어주세요. 꼬임 방지용.
    }

    private void HandleCharacterSelected(int playerId)
    {
        currentCharacter = characterFactory.Create(playerId, spawnPosition.position);

        if (currentCharacter == null)
        {
            Debug.LogError("[GameSessionManager] 캐릭터 생성 실패");
            return;
        }

        stageManager.SetCharacter(currentCharacter); 

        
         //stageManager.EnterStage(int(StageId.St1)); 
    }

    //스테이지 매니저 참고용 코드
    /*
    public void SetCharacter(CharacterControllers character)
    {
        currentCharacter = character;
    }

     public void EnterStage(int id) //
    {
    ~~~~씬 로딩 등.
    ~~~~
    
        OnStageReady?.Invoke(stageFacade); 마지막에.
    }

    스테이지 완료 이벤트에, 스테이지 ㅁ

    */

    private void HandleStageReady(StageFacade stageFacade)
    {
        currentStageFacade = stageFacade;

        if (currentCharacterFacade == null)
        {
            Debug.LogError("[GameSessionManager] CharacterFacade가 없습니다.");
            return;
        }

        if (currentStageFacade == null)
        {
            Debug.LogError("[GameSessionManager] StageFacade가 없습니다.");
            return;
        }

        uiManager.BindGameplayUI(currentCharacterFacade, currentStageFacade);
    }



}
