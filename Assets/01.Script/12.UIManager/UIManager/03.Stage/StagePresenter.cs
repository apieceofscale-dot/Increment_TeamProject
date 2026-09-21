using System;
using UnityEngine;

public class StagePresenter
{
    StageFacade model;
    StageView view;

    private readonly StageChangedEventChannelSO stageChangedChannel;

    public StagePresenter(StageView view, StageFacade model, StageChangedEventChannelSO stageChangedChannel)
    {
        if (view == null) throw new ArgumentNullException(nameof(view));
        if (model == null) throw new ArgumentNullException(nameof(model));
        if (stageChangedChannel == null) throw new ArgumentNullException(nameof(stageChangedChannel));

        this.model = model;
        this.view = view;
        this.stageChangedChannel = stageChangedChannel;

        view.OnChallengeBtnClicked += HandleChallengeStageMove;
        stageChangedChannel.OnRaised += HandleStageChange;
        model.StageChanged += HandleStageChange;

        HandleStageChange(model.GetStageInfo());
    }

    public void HandleStageChange(StageChangedInfo info)
    {
        if (!info.IsValid)
        {
            view.SetChallengeAvailable(false);
            return;
        }

        view.SetName(info.MapName);
        view.SetChallengeStageNum(info.ChallengeStageNum.ToString());
        view.SetStageProcedureText(info.NowStageNum, info.TotalStageNum);
        view.SetStageProcedureBar(info.NowStageNum, Math.Max(1, info.TotalStageNum));
        view.SetChallengeAvailable(info.CanChallenge);
    }

    public void HandleChallengeStageMove()
    {
        if (model.IsTransitioning)
            return;

        model.MoveToChallengeStage();
    }
}
