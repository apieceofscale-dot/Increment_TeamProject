using UnityEngine;


// 런타임 상태 보관
public sealed class StageStatus
{
    public StageDefinition Definition { get; private set; }
    public StageState State { get; private set; } = StageState.None;

    public int KillCount { get; private set; } // 엘리트 포함x
    public float RemainTime { get; private set; }


    // 클리어 조건 달성 여부인데, 클리어 하고도 계속 파밍하는 경우 체크 위해 별도로 만든 필드
    public bool IsClearConditionMet { get; private set; }

    public int HighestClearedStageId { get; private set; }


    public bool IsEliteActive { get; private set; }
    public float EliteRemainTime { get; private set; }

    // 스테이지 진입 초기화
    public void Reset(in StageDefinition definition)
    {
        Definition = definition;
        State = StageState.Battle;
        KillCount = 0;
        RemainTime = definition.TimeLimit;
        IsClearConditionMet = false;

        IsEliteActive = false;
        EliteRemainTime = 0f;
    }

    public void SetState(StageState state) => State = state;


    public void AddKill() => KillCount++;


    // 보스 스테이지, 경험치 던전에 시간제한이 있으면 살리고 아닐 시 아래 엘리트용 시간계산기만 살릴것 
    // public bool TickTime(float deltaTime)
    // {
    //     if (Definition.TimeLimit <= 0f) return false; // 파민스테이지는 건너뜀
    //     // 중간보스 등 시간제한 있는 곳에서만        
    //     RemainTime -= deltaTime;
    //     return RemainTime <= 0f;
    // }

    public void CheckClearConditionMet()
    {
        IsClearConditionMet = true;
        if (Definition.StageId > HighestClearedStageId)
            HighestClearedStageId = Definition.StageId;
    }

    public void BeginElite()
    {
        IsEliteActive = true;
        EliteRemainTime = Definition.EliteTimeLimit;
    }

    public void EndElite()
    {
        IsEliteActive = false;
        EliteRemainTime = 0f;
    }

    // 엘리트 제한시간 감소, true = 시간초과(도망)
    public bool TickEliteTime(float deltaTime)
    {
        if (!IsEliteActive) return false;
        if (Definition.EliteTimeLimit <= 0f) return false;
        EliteRemainTime -= deltaTime;
        return EliteRemainTime <= 0f;
    }


    // 진행상황 전달
    public StageProgressInfo ToInfo() => new StageProgressInfo(
            Definition.StageId, Definition.Chapter, Definition.IndexInChapter, Definition.DisplayName,
            Definition.Type, State,
            KillCount, Definition.ClearKillCount, RemainTime, IsClearConditionMet);

    public EliteProgressInfo ToEliteInfo() => new EliteProgressInfo(
        IsEliteActive,
        Definition.HasElite && !IsEliteActive && State == StageState.Battle,
        EliteRemainTime,
        Definition.EliteTimeLimit);

}
