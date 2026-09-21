using UnityEngine;

public class GameSessionManager : MonoBehaviour, IBootStrapper
{
    [SerializeField] Transform spawnPosition;
    //스테이지 완료so 구독. ???

    private bool gameplayUiBound;

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

        stageManager.OnStageReady += HandleStageReady;
    }

    private void HandleCharacterSelected(int playerId)
    {
        currentCharacter = characterFactory.Create(playerId, spawnPosition.position);

        if (currentCharacter == null)
        {
            Debug.LogError("[GameSessionManager] 캐릭터 생성 실패");
            return;
        }
        currentCharacterFacade = currentCharacter.GetComponent<CharacterFacade>();

        if (currentCharacterFacade == null)
        {
            Debug.LogError("[GameSessionManager] CharacterFacade가 없습니다.");
            return;
        }

        stageManager.SetCharacter(currentCharacter); 
        
        stageManager.EnterStage((int)StageId.St1); 
    }

 
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

        if (gameplayUiBound) return;

        uiManager.BindGameplayUI(currentCharacterFacade, currentStageFacade);

        gameplayUiBound = true;

    }



}
