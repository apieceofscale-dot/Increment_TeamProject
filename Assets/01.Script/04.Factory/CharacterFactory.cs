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

        character.Initialize(playerData); // 왠지 내가 다른 파일 잘못 건드린 것 같아서 아래에는 테스트용 임시 데이터 사용

        return character;
    }

    [ContextMenu("생성 테스트")]
    private void TestCreateCharacter()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("플레이모드에서 실행할 것");
            return;
        }

        Vector3 spawnPosition = testSpawnPoint != null ? testSpawnPoint.position : Vector3.zero;

        PlayerData testData = new PlayerData // 테스트용 임시 데이터, 캐릭터 스탯 등은 Status의 생성자 수치를 적용
        {
            id = -1,
            codeName = "TestPlayer",
            displayName = "테스트 플레이어",
            description = "팩토리 생성 테스트용"
        };

        CharacterFacade character = Create(testData, spawnPosition);

        if (character == null)
        {
            Debug.LogError("생성 실패");
            return;
        }

        Debug.Log($"생성 성공 | Name : {character.name}");
    }
}
