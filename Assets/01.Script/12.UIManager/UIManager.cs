using UnityEngine;

public class UIManager : MonoBehaviour, IBootStrapper
{
    
    public int BootOrder => (int)BootLayer.UIManager;

    [Header("UI Prefab")]
    [SerializeField] GameObject playerHudPrefab;
    [SerializeField] GameObject mainHudPrefab;
    [SerializeField] GameObject systemHudPrefab;

    private CharacterFacade characterFacade;  
    private StageFacade stageFacade;

    GameObject playerHud;
    GameObject mainHud;
    //GameObject systemHud;

    public void IBootStrapperInject(BootstrapContext context)
    {

    }
    public void IBootStrapperInitialize()
    {
        CreateMainHud();
    }

   public void CreateMainHud()
    {
        if (mainHud != null) return;

        mainHud = Instantiate(mainHudPrefab);

        MainView mainView =  mainHud.GetComponentInChildren<MainView>(true);
    }

    /*
    public void CreatPlayerHud()
    {
        if (playerHud != null)
            return;

        playerHud = Instantiate(playerHudPrefab);

        PlayerStatusBarView playerStatusView = playerHud.GetComponentInChildren<PlayerStatusBarView>(true);

        SkillView skillView = playerHud.GetComponentInChildren<SkillView>(true);

        AutoFarmingView autoFarmingView = playerHud.GetComponentInChildren<AutoFarmingView>(true);

        EquipmentPopupView equipmentPopupView = playerHud.GetComponentInChildren<EquipmentPopupView>(true);

        CharacterSettingView characterSettingView = playerHud.GetComponentInChildren<CharacterSettingView>(true);

        StageView stageView = playerHud.GetComponentInChildren<StageView>(true);

        // View 자체 초기화
        characterSettingView.InitializePopUp();

        // Presenter 연결
        playerStatusPresenter = new PlayerStatusPresenter(characterFacade, playerStatusView);

        skillPresenter = new SkillPresenter(characterFacade, skillView);

        autoFarmingPresenter = new AutoFarmingPresenter(autoFarmingView, characterFacade);

        equipmentPopupPresenter = new EquipmentPopupPresenter(equipmentPopupView, characterFacade);

        stagePresenter = new StagePresenter(stageFacade, stageView);
    }
    */
}

