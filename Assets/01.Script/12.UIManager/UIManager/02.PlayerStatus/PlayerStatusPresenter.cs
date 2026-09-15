using UnityEngine;

public class PlayerStatusPresenter
{
    private PlayerStatusBarView view;
    private readonly CharacterFacade model;

    //참조 : 파사드에서 나온 체력 변경 이벤트
    //참조 : 파사드에서 나온 마나 변경 이벤트
    //참조 : 파사드에서 나온 경험치 변경 이벤트
    //참조 : 파사드에서 나온 직업 변경 이벤트
    //참조 : 파사드에서 나온 레벨업 이벤트.

    private PlayerStatusPresenter(PlayerStatusBarView view, CharacterFacade model)//ui매니저에서 호출.
    {
        this.view = view;
        this.model = model;

        //체력변경 이벤트 +=HandleHpChanged
        //~~~

        //Initialize();

    }

    //여긴 너무 많으니 되도록이면 구조체를 선언해주세요.

    private void HandleHpChanged(float nowHp, float totalHp)
    {
        view.SetHpBar(nowHp, totalHp);
    }
    private void HandleMpChange(float nowMp, float totalMp)
    {
        view.SetMpBar(nowMp, totalMp);
    }
    private void HandleExpChanged(float nowExp, float totalExp)
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

    /*
    private void Initialize()
    {
        
    }
    */
}
    

