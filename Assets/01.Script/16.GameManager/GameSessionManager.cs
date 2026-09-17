using UnityEngine;

public class GameSessionManager : MonoBehaviour, IBootStrapper
{
    [SerializeField] Transform spawnPosition;

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
    }

    private void HandleCharacterSelected(int playerId)
    {
        

        currentCharacter = characterFactory.Create(playerId, spawnPosition.position);

        if (currentCharacter == null) return;

        // 스테이지 진입 요청
        // stageManager.EnterStage(...);
    }

    private void HandleStageReady(StageFacade stageFacade)
    {
        currentStageFacade = stageFacade;

        uiManager.BindGameplayUI(currentCharacterFacade, currentStageFacade);
    }



}
