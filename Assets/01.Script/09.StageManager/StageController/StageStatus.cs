using UnityEngine;


// 런타임 상태 보관
public sealed class StageStatus
{
    public StageDefinition Definition { get; private set; }
    public StageState State { get; private set; } = StageState.None;

    public int KillCount { get; private set; }
    public int AliveCount { get; private set; }
    public float RemainTime { get; private set; }


    public int HighestClearedStageId { get; private set; }

    // 스테이지 진입 초기화
    public void Reset(in StageDefinition definition)
    {
        Definition = definition;
        State = StageState.Battle;
        KillCount = 0;
        AliveCount = 0;
        RemainTime = definition.TimeLimit;
    }

    public void SetState(StageState state) => State = state;

    public void AddSpawned() => AliveCount++;

    public void AddKill()
    {
        AliveCount = Mathf.Max(0, AliveCount - 1);
        KillCount++;
    }

    public bool TickTime(float deltaTime)
    {

        // 중간보스 등 시간제한 있는 곳에서
        return RemainTime <= 0f;
    }

    public StageProgressInfo ToInfo() => new StageProgressInfo(Definition.StageId, Definition.Chapter, Definition.IndexInChapter, Definition.DisplayName,
        Definition.Type, State,
        KillCount, Definition.ClearKillCount,
        Definition.HasTimeLimit, RemainTime, Definition.NextStageId);


}
