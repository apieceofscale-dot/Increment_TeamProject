using UnityEngine;

public class StagePresenter
{
    StageFacade model;
    StageView view;

    public StagePresenter(StageView view, StageFacade model)
    {
        this.model = model;
        this.view = view;

        view.OnChallengeBtnClicked += HandleChallengeStageMove;

        model.StageChanged += HandleStageChanged;

        StageChangedInfo info = model.GetStageInfo();
        if (info.IsValid)
            ApplyStageInfo(info);
    }

    private void HandleStageChanged(StageChangedInfo data)
    {
        if (!data.IsValid)
            return;

        ApplyStageInfo(data);
    }

    private void ApplyStageInfo(StageChangedInfo data)
    {
        HandleStageChange(data.MapName, data.NowStageNum, data.TotalStageNum);
    }

    public void HandleStageChange(string nowMapname, int nowStageNum, int totalStageNum)
    {
        view.SetName(nowMapname);
        view.SetChallengeStageNum(nowStageNum);
        view.SetStageProcedureText(nowStageNum, totalStageNum);
        view.SetStageProcedureBar(nowStageNum, totalStageNum);
    }

    public void HandleChallengeStageMove()
    {
        model.MoveToChallengeStage();
    }
}
