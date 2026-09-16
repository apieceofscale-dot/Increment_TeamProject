using UnityEngine;
using System.Collections.Generic;


// 스테이지 정의 임시 지정 아무값
// 스테이지데이터 csv생기면 이 클래스를 데이터매니저 통한 스테이지 데이터 조회로 교체할것
public sealed class TempStageTable
{
    private readonly Dictionary<int, StageDefinition> table;


    // 챕터별 스테이지 수 / 도전(보스) 스테이지 id
    private readonly Dictionary<int, int> chapterStageCount = new Dictionary<int, int>();
    private readonly Dictionary<int, int> chapterChallengeStageId = new Dictionary<int, int>();

    public TempStageTable()
    {
        table = new Dictionary<int, StageDefinition>
        {
            // 1챕터 1스테이지 — 슬라임 파밍
            [9000] = new StageDefinition(
                stageId: 9000, chapter: 1, indexInChapter: 1,
                stageCodeName: "St1", displayName: "초원 입구",
                sceneName: "Stage01",                       // ★ 실제 씬 파일명과 일치
                type: StageType.Farm,
                monsterId: 2000, dropTableId: 2000, statMultiplier: 1.0f,
                clearKillCount: 20, maxAliveMonster: 6, spawnInterval: 1.5f, timeLimit: 0f,
                eliteMonsterId: 0, eliteDropTableId: 0, eliteTimeLimit: 0f,
                nextStageId: 9001, failStageId: 9000),

            // 1챕터 2스테이지 — 고블린 파밍 + 엘리트
            [9001] = new StageDefinition(
                stageId: 9001, chapter: 1, indexInChapter: 2,
                stageCodeName: "St2", displayName: "초원 안쪽",
                sceneName: "Stage02",                       // ★
                type: StageType.Farm,
                monsterId: 2001, dropTableId: 2001, statMultiplier: 1.25f,
                clearKillCount: 25, maxAliveMonster: 6, spawnInterval: 1.4f, timeLimit: 0f,
                eliteMonsterId: 2002, eliteDropTableId: 2002, eliteTimeLimit: 60f,
                nextStageId: 9002, failStageId: 9001),

            // 1챕터 보스 = 도전맵. 클리어 시 Cleared로 고정되어 무한 리스폰을 막는다
            [9002] = new StageDefinition(
                stageId: 9002, chapter: 1, indexInChapter: 3,
                stageCodeName: "St3", displayName: "오크 마을",
                sceneName: "Stage03",                       // ★ 도전맵 씬 (이미 존재)
                type: StageType.Boss,
                monsterId: 2002, dropTableId: 2002, statMultiplier: 1.5f,
                clearKillCount: 1, maxAliveMonster: 1, spawnInterval: 3f, timeLimit: 120f,
                eliteMonsterId: 0, eliteDropTableId: 0, eliteTimeLimit: 0f,
                nextStageId: 9002, failStageId: 9002),
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
                    $"[TempStageTable] 챕터 {definition.Chapter}에 보스 스테이지가 2개 이상입니다" +
                    $"먼저 등록된 것을 유지합니다. 무시된 stageId={definition.StageId}");
                continue;
            }


            chapterChallengeStageId[definition.Chapter] = definition.StageId;

        }
    }
}
