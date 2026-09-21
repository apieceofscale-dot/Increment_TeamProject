
using System;
using UnityEngine;
// [추가] 메인 UI 버튼에 프로젝트의 Input System 입력을 전달한다.
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class UIManager : MonoBehaviour, IBootStrapper
{
    [SerializeField] private StageChangedEventChannelSO stageChangedChannel;

    public int BootOrder => (int)BootLayer.UIManager;

    [Header("UI Prefab")]
    [SerializeField] GameObject playerHudPrefab;
    [SerializeField] GameObject mainHudPrefab;
    [SerializeField] GameObject systemHudPrefab;

    private CharacterFacade characterFacade;  
    private StageFacade stageFacade;
    private ItemFacade itemFacade;

    GameObject playerHud;
    GameObject mainHud;
    //GameObject systemHud;  

    MonoBehaviour[] uiComponents;

    PortraitPresenter portraitPresenter;
    PlayerStatusBarPresenter playerStatusBarPresenter;
    StagePresenter stagePresenter;
    ///캐릭터 세팅은 프리젠터 없음
    SkillPresenter skillPresenter;  
    AutoFarmingPresenter autoFarmingPresenter;

    CharacterPopupPresenter characterPopupPresenter;
    EquipmentPopupPresenter equipmentPopupPresenter;
    SkillPopupPresenter skillPopupPresenter;
    EnchatPopupPresenter enchatPopupPresenter;
    MainPresenter mainPresenter;


    public event Action<int> OnCharacterSelected;


    public void IBootStrapperInject(BootstrapContext context)
    {
        //빈 게 맞음.
    }
    public void IBootStrapperInitialize()
    {
        // [추가] 씬에 EventSystem이 없어도 시작/캐릭터 선택 버튼이 입력을 받도록 준비한다.
        EnsureEventSystem();
        CreateMainHud();
        CreatPlayerHud();
    }

    // [추가] 기존 EventSystem은 재사용하고, 없으면 DDOL 매니저 아래에 생성해 유지한다.
    private void EnsureEventSystem()
    {
        EventSystem eventSystem = FindFirstObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject inputRoot = new GameObject("EventSystem");
            inputRoot.transform.SetParent(transform, false);
            eventSystem = inputRoot.AddComponent<EventSystem>();
        }

        // [추가] 새 Input System 전용 설정에 맞는 모듈을 사용한다.
        // 모듈은 OnEnable에서 기본 UI 액션(클릭/포인터/확인)을 자동으로 연결한다.
        if (eventSystem.GetComponent<BaseInputModule>() == null)
            eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
    }

   public void CreateMainHud()
    {
        if (mainHud != null) return;

        mainHud = Instantiate(mainHudPrefab);

        InitializeViews(mainHud);

        MainView mainView = GetView<MainView>(mainHud);

        mainPresenter = new MainPresenter(mainView);

        mainPresenter.OnCharacterSelected += HandleCharacterSelected;


        mainHud.SetActive(true);
    }

    private void HandleCharacterSelected(int playerId)
    {
        OnCharacterSelected?.Invoke(playerId);
    }


    public void CreatPlayerHud()
    {
        if (playerHud != null) return;

        playerHud = Instantiate(playerHudPrefab);

        InitializeViews(playerHud);        

        playerHud.SetActive(false);
       
    }

    // 이 함수를 main ui-> 캐릭터 생성 -> 스테이지 생성 -> 이후 받으면 됨.
    public void BindGameplayUI(CharacterFacade characterFacade, StageFacade stageFacade)
    {
        playerHud.SetActive(true);
        mainHud.SetActive(false);


        this.characterFacade = characterFacade;
        this.stageFacade = stageFacade;

        InitializePresneter();
    }

    private void InitializeViews(GameObject hud)
    {
        uiComponents = hud.GetComponentsInChildren<MonoBehaviour>(true);

        foreach (MonoBehaviour behaviour in uiComponents)
        {
            if (behaviour is IUIViewInitialize initializable)
            {
                initializable.InitializeView();
            }
        }
    }

    private void InitializePresneter()
    {
        PortraitView portraitView = GetView<PortraitView>(playerHud);
        PlayerStatusBarView playerStatusBarView = GetView<PlayerStatusBarView>(playerHud);
        StageView stageView = GetView<StageView>(playerHud);
        //CharacterSettingView characterSettingView = GetView<CharacterSettingView>(); 이건 없음.
        SkillView skillView = GetView<SkillView>(playerHud);            
        AutoFarmingView autoFarmingView = GetView<AutoFarmingView>(playerHud);

        //나머지 나중에 선언
       // EquipmentPopupView equipmentPopupView = GetView<EquipmentPopupView>(playerHud);
        //EnchatPopupView enchatPopupView = GetView<EnchatPopupView>(playerHud);

        if (itemFacade == null)
        {
            itemFacade = FindFirstObjectByType<ItemFacade>();
        }

        MainView mainView = GetView<MainView>(mainHud);


        // Presenter 연결은 어차피 new자동화가 안되서 걍 하나씩 씀.
        portraitPresenter = new PortraitPresenter (portraitView,characterFacade);
        playerStatusBarPresenter = new PlayerStatusBarPresenter(playerStatusBarView, characterFacade);
        stagePresenter = new StagePresenter(stageView, stageFacade, stageChangedChannel);
        skillPresenter = new SkillPresenter(skillView, characterFacade);
        autoFarmingPresenter = new AutoFarmingPresenter(autoFarmingView, characterFacade);

        characterPopupPresenter = new CharacterPopupPresenter();
       // equipmentPopupPresenter = new EquipmentPopupPresenter(equipmentPopupView, characterFacade);
        skillPopupPresenter = new SkillPopupPresenter();
       // enchatPopupPresenter = new EnchatPopupPresenter(enchatPopupView, characterFacade, itemFacade);
      
    }

    private T GetView<T>(GameObject hud) where T : MonoBehaviour
    {
        T view = hud.GetComponentInChildren<T>(true);


        if (view == null)
        {
            throw new InvalidOperationException($"[UIManager] {hud.name}�� {typeof(T).Name}�� �����ϴ�.");
        }

       

        return view;
    }

}

