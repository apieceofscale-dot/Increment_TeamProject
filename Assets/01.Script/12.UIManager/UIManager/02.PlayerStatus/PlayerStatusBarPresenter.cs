using UnityEngine;

public class PlayerStatusBarPresenter
{
    private PlayerStatusBarView view;
    private readonly CharacterFacade model;

    //참조 : 파사드에서 나온 체력 변경 이벤트
    //참조 : 파사드에서 나온 마나 변경 이벤트
    //참조 : 파사드에서 나온 경험치 변경 이벤트
    //참조 : 파사드에서 나온 직업 변경 이벤트
    //참조 : 파사드에서 나온 레벨업 이벤트.

    public PlayerStatusBarPresenter(PlayerStatusBarView view, CharacterFacade model)//ui매니저에서 호출.
    {
        this.view = view;
        this.model = model;


        model.HpChanged += HandleHpChanged;
        model.MpChanged += HandleMpChanged;
        model.LevelChanged += HandleLevelChanged;
        model.JobNameChanged += HandleJobChanged;
        model.ExpChanged += HandleExpChanged;
       

        HandleHpChanged(model.CurrentHp, model.MaxHp);
        HandleMpChanged(model.CurrentMp, model.MaxMp);
        HandleLevelChanged(model.Level);
        HandleJobChanged(model.JobName);
        HandleLevelChanged(model.Level);



    }

    //여긴 너무 많으니 되도록이면 구조체를 선언해주세요.

    private void HandleHpChanged(long nowHp, long totalHp)
    {
        view.SetHpBar(nowHp, totalHp);
    }
    private void HandleMpChanged(int nowMp, int totalMp)
    {
        view.SetMpBar(nowMp, totalMp);
    }
    private void HandleExpChanged(long nowExp, long totalExp)
    {
        view.SetExpBar(nowExp, totalExp);
    }
    private void HandleLevelChanged(int level)
    {
        view.SetLevel(level);
    }
    private void HandleJobChanged(string job)
    {
        view.SetJobName(job);
    }

    
}
    

