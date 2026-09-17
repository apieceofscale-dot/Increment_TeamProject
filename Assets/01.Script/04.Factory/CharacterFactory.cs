using UnityEngine;
//살짝 아쉽습니다. 왜나면 분명히 모든 캐릭터의 목록을 불러 올 수 있는
//딕셔너리에 넣어두면 그것 만으로도 불러 올 수 있는데, 안 그러면 자신이 여기서 모든 api 를 제공해야 하거든요
//무엇보다 호출자가 ID자체에 대해 알아야 합니다. 하지만 딕셔너리를 쓰고 oderby 해놓으면 그럴 필요가 없어요.

public class CharacterFactory : MonoBehaviour, IBootStrapper
{
    public int BootOrder { get; }

    [SerializeField] private CharacterControllers characterPrefab;
    [SerializeField] private Transform testSpawnPoint; // 테스트용

    public void IBootStrapperInject(BootstrapContext context)
    {

    }
    public void IBootStrapperInitialize()
    {

    }
    public CharacterControllers Create(int playerId, Vector3 spawnPosition)
    {
        if (DataManager.instance == null)
        {
            Debug.LogError("데이터 매니저 없음", this);
            return null;
        }

        if (!DataManager.instance.TryGetPlayerData(playerId, out PlayerData playerData) || playerData == null)
        {
            Debug.LogError($"플레이어 데이터를 찾을 수 없음 ID: {playerId}", this);
            return null;
        }

        if (characterPrefab == null)
        {
            Debug.LogError("캐릭터 프리팹 없음", this);
            return null;
        }

        CharacterControllers character = Instantiate(characterPrefab, spawnPosition, Quaternion.identity);

        character.Initialize(playerData);

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

        Vector3 spawnPosition = testSpawnPoint != null ? testSpawnPoint.position : Vector3.zero; // 지정한 스폰포인트가 없을 시 0,0,0에서 생성

        CharacterControllers character = Create(1000, spawnPosition);

        if (character == null)
        {
            Debug.LogError("생성 실패");
            return;
        }

        Debug.Log($"생성 성공 | Name : {character.name}");
    }
}
