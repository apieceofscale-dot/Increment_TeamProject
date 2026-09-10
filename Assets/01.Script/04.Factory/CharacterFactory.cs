using UnityEngine;

public class CharacterFactory : MonoBehaviour
{
    [SerializeField] private CharacterFacade characterPrefab;
    [SerializeField] private Transform testSpawnPoint; // 테스트용
    
    public CharacterFacade Create(PlayerData playerData, Vector3 spawnPosition)
    {
        if (characterPrefab == null)
        {
            Debug.LogError("캐릭터 프리팹 없음");
            return null;
        }

        if (!characterPrefab.TryGetComponent(out CharacterControllers _))
        {
            Debug.LogError("캐릭터 프리팹에 컨트롤러 없음");
            return null;
        }

        if (playerData == null)
        {
            Debug.LogError("플레이어 데이터 없음");
            return null;
        }

        CharacterFacade character = Instantiate(characterPrefab, spawnPosition, Quaternion.identity);

        character.Initialize(playerData);

        return character;
    }
    
    public CharacterFacade Create(int playerId, Vector3 spawnPosition)
    {
        if (DataManager.instance == null)
        {
            Debug.LogError("DataManager가 없습니다.");
            return null;
        }

        if (!DataManager.instance.TryGetPlayerData(playerId, out PlayerData playerData))
        {
            Debug.LogError($"플레이어 데이터를 찾을 수 없습니다. ID: {playerId}");
            return null;
        }

        return Create(playerData, spawnPosition);
    }

    [ContextMenu("생성 테스트")]
    private void TestCreateCharacter()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("플레이모드에서 실행할 것");
            return;
        }

        Vector3 spawnPosition = testSpawnPoint != null ? testSpawnPoint.position : Vector3.zero; // 지정한 스폰포인트가 없을 시 0,0,0에서 생성

        CharacterFacade character = Create(1000, spawnPosition);

        if (character == null)
        {
            Debug.LogError("생성 실패");
            return;
        }

        Debug.Log($"생성 성공 | Name : {character.name}");
    }
}
