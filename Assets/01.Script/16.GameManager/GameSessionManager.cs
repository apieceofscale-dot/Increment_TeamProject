using UnityEngine;

public class GameSessionManager : MonoBehaviour, IBootStrapper
{
    [SerializeField] Transform spawnPosition;
    [SerializeField] int firstStageId = 9000;

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

    private void OnDestroy()
    {
        if (uiManager != null)
            uiManager.OnCharacterSelected -= HandleCharacterSelected;

        if (stageManager != null)
            stageManager.OnStageReady -= HandleStageReady;
    }

    private void HandleCharacterSelected(int playerId)
    {
        currentCharacter = characterFactory.Create(playerId, spawnPosition.position);

        if (currentCharacter == null)
        {
            Debug.LogError("[GameSessionManager] Character spawn failed.");
            return;
        }

        currentCharacterFacade = currentCharacter.GetComponent<CharacterFacade>();

        if (currentCharacterFacade == null)
        {
            Debug.LogError("[GameSessionManager] CharacterFacade is missing.");
            return;
        }

        stageManager.SetCharacter(currentCharacterFacade);
        stageManager.EnterStage(firstStageId);
    }

    private void HandleStageReady(StageFacade stageFacade)
    {
        currentStageFacade = stageFacade;

        if (currentCharacterFacade == null)
        {
            Debug.LogError("[GameSessionManager] CharacterFacade is missing.");
            return;
        }

        if (currentStageFacade == null)
        {
            Debug.LogError("[GameSessionManager] StageFacade is missing.");
            return;
        }

        if (gameplayUiBound)
            return;

        uiManager.BindGameplayUI(currentCharacterFacade, currentStageFacade);

        gameplayUiBound = true;
    }

    /// <summary>??? ?? ?? ?? ?? ? ?? ????? ??. UI/??? ???? ??.</summary>
    public bool TryGoToNextStage()
    {
        if (currentStageFacade == null)
            return false;

        return currentStageFacade.GoNextStage();
    }

    public bool CanGoToNextStage =>
        currentStageFacade != null && stageManager != null && stageManager.CanGoNextStage;
}
