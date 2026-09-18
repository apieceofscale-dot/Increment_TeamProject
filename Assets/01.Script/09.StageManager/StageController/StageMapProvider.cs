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


    private void OnDrawGizmos()
    {
        if (playerStart != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(playerStart.position, 0.4f);
        }

        if (monsterSpawnPoints != null)
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < monsterSpawnPoints.Length; i++)
            {
                if (monsterSpawnPoints[i] != null)
                    Gizmos.DrawWireSphere(monsterSpawnPoints[i].position, 0.3f);
            }
        }

        if (bossSpawnPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(bossSpawnPoint.position, Vector3.one * 0.8f);
        }
    }

    private void CollectSpawnPoints()
    {
        System.Collections.Generic.List<Transform> found = new System.Collections.Generic.List<Transform>();

        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            if (child != transform && child.name.StartsWith("SpawnPoint"))
                found.Add(child);
        }

        monsterSpawnPoints = found.ToArray();
        Debug.Log($"[StageMapProvider] 스폰 포인트 {found.Count}개 수집", this);
    }
}
