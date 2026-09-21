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

        //도전 활성화 이벤트 += HandleChallengeAvailabilityChanged;

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
        view.SetChallengeStageNum(info.ChallengeStageNum.ToString()); //사실 이름인데 잘못넣엇는데 걍 넘어감.
        view.SetStageProcedureText(info.NowStageNum, info.TotalStageNum);
        view.SetStageProcedureBar(info.NowStageNum, Math.Max(1, info.TotalStageNum));

        view.SetChallengeAvailable(info.CanChallenge);

        HandleChallengeAvailabilityChanged(info.CanChallenge);
    }

    public void HandleChallengeStageMove()
    {
        if (model.IsTransitioning) return;

        // TODO: 스테이지 담당자가 제공할 "현재 스테이지 도전 시작" API 연결.
        //
        // 조건 1 완료 → CanChallenge = true 수신 → 버튼 활성화
        // 버튼 클릭 → 여기서 도전 시작 요청        
        // 도전 시작 → CanChallenge = false 수신 → 버튼 비활성화
        // 조건 2 완료 → 스테이지 쪽에서 다음 스테이지로 이동
        // 변경된 스테이지 정보 수신 → HandleStageChange에서 화면 갱신
        //
        // 기존 model.MoveToChallengeStage()는 별도 보스 맵으로 이동하므로
        // 현재 의도에 맞는 동작이 준비되기 전까지 호출하지 않음.
        model.MoveToChallengeStage();
    }


    private void HandleChallengeAvailabilityChanged(bool available)
    {
        view.SetChallengeAvailable(available);
    }

   

}
