using System;
using UnityEngine;

public sealed class StageFacade : MonoBehaviour
{
    //세이브매니저만들어지면 완성되면 이 값을 false로 두고 SaveManager가 StartStage를 호출
    //[SerializeField] private bool autoStartOnBoot = true;

    private StageController stageController;

    public void Bind(StageController controller)
    {
        stageController = controller;
    }

    /// <summary>스테이지 진입·클리어 등 진행 변경 시 UI 갱신용.</summary>
    public event Action<StageChangedInfo> StageChanged;

    internal void NotifyStageChanged(in StageChangedInfo info)
    {
        StageChanged?.Invoke(info);
    }

    private bool IsReady()
    {
        if (stageController == null)
        {
            Debug.LogWarning("[StageFacade] 아직 초기화되지 않았습니다. 부트 시퀀스를 확인하세요.");
            return false;
        }
        return true;
    }

    // 버튼용

    // 지정 스테이지 시작
    public void StartStage(int stageId)
    {
        if (!IsReady()) return;
        stageController.EnterStage(stageId);
    }
    //다음스테이지 활성화용
    public bool GoNextStage()
    {
        if (!IsReady()) return false;
        return stageController.TryGoNextStage();
        //수정예정
    }

    // 도전맵(보스맵) 이동, 이동이 시작됐을 때만 true
    public bool MoveToChallengeStage()
    {
        if (!IsReady()) return false;
        return stageController.TryMoveToChallengeStage();
    }

    // 엘리트, 소환조건 못 채웠을시 false
    public bool SummonElite()
    {
        if (!IsReady()) return false;
        return stageController.TrySummonElite();

    }


    // 씬 전환 중일 땐 이동 버튼 잠금이어야해서
    public bool IsTransitioning => IsReady() && stageController.IsTransitioning;


    //UI용


    //현재스테이지스냅샹, presenter초기화용 
    public StageChangedInfo GetStageInfo()
    {
        return IsReady() ? stageController.StageInfo : default;
    }


    // 진행 상황 ui표시용 
    public StageProgressInfo GetProgress()
    {
        return IsReady() ? stageController.Progress : default;
    }

    // 엘리트진행 ui 표기
    public EliteProgressInfo GetEliteProgress()
    {
        return IsReady() ? stageController.EliteProgress : default;
    }



}
