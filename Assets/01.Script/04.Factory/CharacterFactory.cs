using UnityEngine;

public class CharacterFactory : MonoBehaviour
{
    [SerializeField] private CharacterFacade characterPrefab;
    [SerializeField] PlayerData playerData;
    public CharacterFacade Create(int playerDataId, Vector3 spawnPosition)
    {
        if (characterPrefab == null)
        {
            Debug.LogError("팩토리에 캐릭터 프리팹 없음");
            return null;
        }

        if (DataManager.instance == null)
        {
            Debug.LogError("데이터 매니저 없음");
            return null;
        }

        CharacterFacade character = Instantiate(characterPrefab, spawnPosition, Quaternion.identity);

        character.Initialize(playerData);

        return character;
    }
}
