using System;
using UnityEngine;

public class StagePresenter
{
    StageFacade model;
    StageView view;

    //스테이지 이동 이벤트 참조. 혹은 so참조

    public StagePresenter(StageView view, StageFacade model)
    {
        this.model = model;
        this.view = view;

        view.OnChallengeBtnClicked += HandleChallengeStageMove;

        //스테이지 이동 이벤트 +=HandleStageChange;

        //HandleStageChange(model.NowMapName, model.NowStageNum, model.TotalStageNum); //이건 초기화용함수
        
    }

    //핸들러의 인자는 제가 일단 완성하기 위해 쓴 것입니다.
    //제네릭으로 선언해서, Data구조를 만들어서 선언하시면 제가 알아서 바꾸면 되니 편한대로 하세요.
    //제네릭이 더 확장성 있는 방법입니다. 나중에 구조체에 멤버만 추가하고, 파사드에서 초기화 하면 끝이니까요.
    //특히 호출자가 수정본을 몰라도 됩니다.
    /*
     ex)
       public void HandleStageChange(StageData data)
     {
         view.SetName(data.nowMapname);
         view.SetChallengeStageNum(data.StageNum);
         view.SetStageProcedureText(data.StageNum, data.totalStageNum);
         view.SetStageProcedureBar(data.StageNum,data.totalStageNum);
     }
     */
    public void HandleStageChange(string nowMapname, int nowStageNum, int totalStageNum)
    {
        view.SetName(nowMapname);
        view.SetChallengeStageNum(nowStageNum);
        view.SetStageProcedureText(nowStageNum, totalStageNum);
        view.SetStageProcedureBar(nowStageNum,totalStageNum);
    }

    public void HandleChallengeStageMove()
    {
        //model.MoveToChallengeStage() 
    }


}
