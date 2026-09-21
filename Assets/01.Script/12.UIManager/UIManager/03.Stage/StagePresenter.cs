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

        //도전 활성화 이벤트 += HandleChallengeAvailabilityChanged;

        //HandleStageChange(model.NowMapName, model.NowStageNum, model.TotalStageNum); //이건 초기화용함수

    }

    public void HandleStageChange(StageChangedInfo info)
    {
        view.SetName(info.MapName);
        view.SetChallengeStageNum(info.ChallengeStageNum);
        view.SetStageProcedureText(info.NowStageNum, info.TotalStageNum);
        view.SetStageProcedureBar(info.NowStageNum, info.TotalStageNum);
    }

    public void HandleChallengeStageMove()
    {
        model.MoveToChallengeStage();
    }


    private void HandleChallengeAvailabilityChanged(bool available)
    {
        view.SetChallengeAvailable(available);
    }

}
