using UnityEngine;
using UnityEngine.Tilemaps;


public class StageMapProvider : MonoBehaviour
{

    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Navi2DPathFinder pathFinder;
    [SerializeField] private Transform playerStart;
    [SerializeField] private Transform[] monsterSpawnPoints;
    [SerializeField] private Transform bossSpawnPoint; // 보스 맵 아닐시 비우기

    public StageMapParts ToParts()
    {
        if (playerStart == null)
        {
            Debug.LogError($"[StageMapProvider] PlayerStart가 없습니다. scene={gameObject.scene.name}", this);
        }

        if (monsterSpawnPoints == null || monsterSpawnPoints.Length == 0)
        {
            Debug.LogWarning($"[StageMapProvider] 몬스터 스폰 포인트가 없습니다. scene={gameObject.scene.name}", this);
        }

        return new StageMapParts(
            transform, groundTilemap, pathFinder,
            playerStart, monsterSpawnPoints, bossSpawnPoint);
    }
}
