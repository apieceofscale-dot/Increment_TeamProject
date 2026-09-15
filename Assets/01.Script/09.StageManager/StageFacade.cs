
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
        stageController.StartStage(stageId);
    }
    //다음스테이지 활성화용
    public bool GoNextStage()
    {
        if (!IsReady()) return false;
        //return stageController.TryGoNextStage();
        //수정예정
        return true;
    }

    public bool SummonElite()
    {
        if (!IsReady()) return false;
        // return stageController.TrySummonElite();
        //수정예정
        return true;
    }


    //UI용


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
