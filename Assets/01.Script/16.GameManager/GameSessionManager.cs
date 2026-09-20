using UnityEngine;

public class GameSessionManager : MonoBehaviour, IBootStrapper
{
    [SerializeField] Transform spawnPosition;
    [SerializeField] int firstStageId = 9000;

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
            return;

        currentCharacterFacade = currentCharacter.GetComponent<CharacterFacade>();
        if (currentCharacterFacade == null)
        {
            Debug.LogError("[GameSessionManager] CharacterFacade�� �����ϴ�.");
            return;
        }

        stageManager.SetCharacter(currentCharacterFacade);
        stageManager.EnterStage(firstStageId);
    }

    private void HandleStageReady(StageFacade stageFacade)
    {
        currentStageFacade = stageFacade;

        uiManager.BindGameplayUI(currentCharacterFacade, currentStageFacade);
    }

    /// <summary>Ŭ���� ���� ���� �� ���� ���������� �̵�. UI���������� ��ư���� ȣ��.</summary>
    public bool TryGoToNextStage()
    {
        if (currentStageFacade == null)
            return false;

        return currentStageFacade.GoNextStage();
    }

    public bool CanGoToNextStage =>
        currentStageFacade != null && stageManager != null && stageManager.CanGoNextStage;
}
