
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
    /// [도전] 버튼 10킬을 채웠으면 다음 스테이지로 이동한다. (001→002→003→보스맵)
    /// 조건 미달·전환 중·마지막 스테이지면 false.
    public bool GoNextStage()
    {
        if (!IsReady()) return false;
        return stageController.TryGoNextStage();
    }

    /// [호환용] 기존 StagePresenter가 부르던 이름. 규칙이 "도전 = 다음 스테이지 이동"으로 확정되어 GoNextStage()로 넘긴다.
    // TODO(UI): StagePresenter에서 GoNextStage()로 바꾸면 이 메서드 삭제
    //[System.Obsolete("GoNextStage()를 사용하세요.")]
    public bool MoveToChallengeStage() => GoNextStage();



    // 엘리트 만들 일 생기면 활성화할것
    // // 엘리트, 소환조건 못 채웠을시 false
    // public bool SummonElite()
    // {
    //     if (!IsReady()) return false;
    //     return stageController.TrySummonElite();
    // }




    //---------UI용

    /// 도전 버튼 활성 여부 (Presenter 초기화 시 1회 조회용. 이후 갱신은 StageChangedEventChannel)
    public bool CanChallenge => IsReady() && stageController.CanGoNextStage;


    // 씬 전환 중일 땐 이동 버튼 잠금이어야해서
    public bool IsTransitioning => IsReady() && stageController.IsTransitioning;




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



    // 엘리트 미진행
    // // 엘리트진행 ui 표기
    // public EliteProgressInfo GetEliteProgress()
    // {
    //     return IsReady() ? stageController.EliteProgress : default;
    // }



}
