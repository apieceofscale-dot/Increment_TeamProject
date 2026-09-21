using UnityEngine;
using System.Collections.Generic;

// 도전 조건: clearKillCount


// 스테이지 정의 임시 보관소
// 스테이지데이터 csv생기면 이 클래스를 데이터매니저 통한 스테이지 데이터 조회로 교체할것
// 0921-> StageData.csv에 스테이지 필드(씬 이름, 킬 수 등)가 아직 없어서 코드에 하드코딩해 둔다

/// [진행 흐름 - 09-21 확정]
///   9000 Stage001 ─(10킬 → 도전)→ 9001 Stage002 ─(10킬 → 도전)→ 9002 Stage003 ─(10킬 → 도전)→ 9003 StageChallenge(보스)
///   - "도전" = NextStageId로 이동. 003의 NextStageId가 보스맵이라 보스맵 이동도 같은 경로로 처리된다.
///   - 마지막(보스) 스테이지는 NextStageId를 자기 자신으로 둔다 → StageController가 "다음 없음"으로 판단.
///   - 엘리트는 이번 범위 밖이라 eliteMonsterId를 전부 0으로 둔다.
public sealed class TempStageTable
{
    // 보스(도전)맵 id. StageId enum(St1~St3)은 StageData.csv에서 자동 생성되므로 CSV에 행이 추가되기 전까지 상수로 둔다.
    // TODO: StageData.csv에 9003(St4) 행이 추가되면 (int)StageId.St4로 교체
    public const int BossStageId = 9003;
    private readonly Dictionary<int, StageDefinition> table;


    // 챕터별 스테이지 수 / 도전(보스) 스테이지 id
    private readonly Dictionary<int, int> chapterStageCount = new Dictionary<int, int>();
    private readonly Dictionary<int, int> chapterChallengeStageId = new Dictionary<int, int>();
    public TempStageTable()
    {
        table = new Dictionary<int, StageDefinition>
        {
            // 1-1 파밍 : 슬라임
            [9000] = new StageDefinition(
                stageId: 9000, chapter: 1, indexInChapter: 1,
                stageCodeName: "St1", displayName: "초원 입구",
                sceneName: "Stage001",
                type: StageType.Farm,
                monsterId: 2000, dropTableId: 2000, statMultiplier: 1.0f,
                clearKillCount: 10, maxAliveMonster: 6, spawnInterval: 1.5f, timeLimit: 0f,
                eliteMonsterId: 0, eliteDropTableId: 0, eliteTimeLimit: 0f,
                nextStageId: 9001, failStageId: 9000),

            // 1-2 파밍 : 고블린
            [9001] = new StageDefinition(
                stageId: 9001, chapter: 1, indexInChapter: 2,
                stageCodeName: "St2", displayName: "초원 안쪽",
                sceneName: "Stage002",
                type: StageType.Farm,
                monsterId: 2001, dropTableId: 2001, statMultiplier: 1.25f,
                clearKillCount: 10, maxAliveMonster: 6, spawnInterval: 1.4f, timeLimit: 0f,
                eliteMonsterId: 0, eliteDropTableId: 0, eliteTimeLimit: 0f,
                nextStageId: 9002, failStageId: 9001),

            // 1-3 파밍 : 오크. 여기서 [도전]을 누르면 보스맵(9003)으로 간다
            [9002] = new StageDefinition(
                stageId: 9002, chapter: 1, indexInChapter: 3,
                stageCodeName: "St3", displayName: "오크 마을",
                sceneName: "Stage003",
                type: StageType.Farm,
                monsterId: 2002, dropTableId: 2002, statMultiplier: 1.5f,
                clearKillCount: 10, maxAliveMonster: 6, spawnInterval: 1.3f, timeLimit: 0f,
                eliteMonsterId: 0, eliteDropTableId: 0, eliteTimeLimit: 0f,
                nextStageId: BossStageId, failStageId: 9002),

            // 1-4 보스맵 : 보스 1마리. 처치 시 Cleared로 고정되어 더 이상 스폰하지 않는다.
            // nextStageId == stageId → "다음 스테이지 없음" (보스 클리어 후 흐름은 기획 확정 후 추가)
            [BossStageId] = new StageDefinition(
                stageId: BossStageId, chapter: 1, indexInChapter: 4,
                stageCodeName: "St4", displayName: "오크 족장의 방",
                sceneName: "StageChallenge",
                type: StageType.Boss,
                monsterId: 2002, dropTableId: 2002, statMultiplier: 3.0f,
                clearKillCount: 1, maxAliveMonster: 1, spawnInterval: 1f, timeLimit: 0f,
                eliteMonsterId: 0, eliteDropTableId: 0, eliteTimeLimit: 0f,
                nextStageId: BossStageId, failStageId: 9002),
        };

        BuildChapterIndex();
    }

    public bool TryGet(int stageId, out StageDefinition definition)
        => table.TryGetValue(stageId, out definition);




    /// <summary>
    /// UI 진행도 표시(now/total)용 해당 챕터 전체 스테이지 수
    /// </summary>
    public int GetChapterStageCount(int chapter)
        => chapterStageCount.TryGetValue(chapter, out int count) ? count : 0;


    /// <summary>
    /// 해당 챕터의 도전맵(= Boss 스테이지) id
    /// </summary>
    public bool TryGetChallengeStageId(int chapter, out int stageId)
        => chapterChallengeStageId.TryGetValue(chapter, out stageId);

    private void BuildChapterIndex()
    {
        foreach (StageDefinition definition in table.Values)
        {
            chapterStageCount.TryGetValue(definition.Chapter, out int count);
            chapterStageCount[definition.Chapter] = count + 1;

            if (definition.Type != StageType.Boss) continue;

            if (chapterChallengeStageId.ContainsKey(definition.Chapter))
            {
                Debug.LogWarning(
                    $"[TempStageTable] 챕터 {definition.Chapter}에 보스 스테이지가 2개 이상입니다. " +
                    $"먼저 등록된 것을 유지합니다. 무시된 stageId={definition.StageId}");
                continue;
            }

            chapterChallengeStageId[definition.Chapter] = definition.StageId;
        }
    }
}
