using UnityEngine;
using System.Collections.Generic;


// 스테이지 정의 임시 지정 아무값
// 스테이지데이터 csv생기면 이 클래스를 데이터매니저 통한 스테이지 데이터 조회로 교체할것
public sealed class TempStageTable
{
    private readonly Dictionary<int, StageDefinition> table;

    public TempStageTable()
    {
        table = new Dictionary<int, StageDefinition>
        {
            // 1챕터 1스테이지 — 슬라임 파밍
            [9000] = new StageDefinition(
                stageId: 9000, chapter: 1, indexInChapter: 1,
                stageCodeName: "St1", displayName: "초원 입구",
                sceneName: "Stage_1_1",
                type: StageType.Farm,
                monsterId: 2000, dropTableId: 2000, statMultiplier: 1.0f,
                clearKillCount: 20, maxAliveMonster: 6, spawnInterval: 1.5f, timeLimit: 0f,
                eliteMonsterId: 0, eliteDropTableId: 0, eliteTimeLimit: 0f,
                nextStageId: 9001, failStageId: 9000),

            // 1챕터 2스테이지 — 고블린 파밍 + 엘리트
            [9001] = new StageDefinition(
                stageId: 9001, chapter: 1, indexInChapter: 2,
                stageCodeName: "St2", displayName: "초원 안쪽",
                sceneName: "Stage_1_2",
                type: StageType.Farm,
                monsterId: 2001, dropTableId: 2001, statMultiplier: 1.25f,
                clearKillCount: 25, maxAliveMonster: 6, spawnInterval: 1.4f, timeLimit: 0f,
                eliteMonsterId: 2002, eliteDropTableId: 2002, eliteTimeLimit: 60f,
                nextStageId: 9002, failStageId: 9001),

            // 1챕터 보스 — 클리어 시 Cleared로 고정되어 무한 리스폰을 막는다
            [9002] = new StageDefinition(
                stageId: 9002, chapter: 1, indexInChapter: 3,
                stageCodeName: "St3", displayName: "오크 마을",
                sceneName: "Stage_1_3",
                type: StageType.Boss,
                monsterId: 2002, dropTableId: 2002, statMultiplier: 1.5f,
                clearKillCount: 1, maxAliveMonster: 1, spawnInterval: 3f, timeLimit: 120f,
                eliteMonsterId: 0, eliteDropTableId: 0, eliteTimeLimit: 0f,
                nextStageId: 9002, failStageId: 9002),
        };
    }

    public bool TryGet(int stageId, out StageDefinition definition)
        => table.TryGetValue(stageId, out definition);
}
