using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class CharacterFactory : MonoBehaviour, IBootStrapper
{
    [SerializeField] private int bootOrder = 5;
    public int BootOrder => bootOrder;
    private BootstrapContext bootContext;
    private bool injected;
    private bool initialized;

    [SerializeField] private CharacterControllers characterPrefab;
    [SerializeField] private Transform testSpawnPoint; // 테스트용

    [SerializeField] private PlayerList playerList;
    [SerializeField, Min(0)] private int testCharacterIndex;
    private IReadOnlyList<PlayerData> availableCharacters;
    public bool IsInitialized => initialized;

    public void IBootStrapperInject(BootstrapContext context)
    {
        bootContext = context;
        injected = true;
    }

    public void IBootStrapperInitialize()
    {
        if (initialized)
            return;

        if (!injected || DataManager.instance == null || characterPrefab == null)
            throw new System.InvalidOperationException("Factory 주입, DataManager, Character Prefab 확인");
        
        BuildCharacterCatalog();
        initialized = true;
    }

    private void BuildCharacterCatalog()
    {
        if (playerList == null || playerList.baseList == null)
            throw new InvalidOperationException("데이터 매니저와 동일한 SO를 연결");

        var entries = new List<PlayerData>();
        var ids = new HashSet<int>();

        foreach (PlayerData source in playerList.baseList)
        {
            if (source == null)
                throw new InvalidOperationException("목록에 비어 있는 캐릭터 항목 존재");

            if (!ids.Add(source.id))
                throw new InvalidOperationException($"캐릭터 ID가 중복: {source.id}");

            if (!DataManager.instance.TryGetPlayerData(source.id, out PlayerData data) || data == null)
                throw new InvalidOperationException($"{source.id} 못찾음. 부트 순서 및 SO 연결 확인");

            entries.Add(data);
        }

        if (entries.Count == 0)
            throw new InvalidOperationException("생성 가능 캐릭터 목록 없음");

        availableCharacters = entries.OrderBy(data => data.id).ToList().AsReadOnly();
    }

    public IReadOnlyList<PlayerData> GetAvailableCharacters()
    {
        if (!initialized)
            throw new InvalidOperationException("캐릭터 팩토리 초기화 이후 목록 조회");

        return availableCharacters;
    }

    public CharacterControllers Create(int playerId, Vector3 spawnPosition)
    {
        if (!initialized)
        {
            Debug.LogError("팩토리 초기화 전", this);
            return null;
        }

        CharacterControllers existing = CharacterControllers.Current;

        if (existing != null)
        {
            if (!existing.IsInitialized || existing.Data == null || existing.Data.id != playerId)
            {
                Debug.LogWarning("기존 플레이어가 초기화 중이거나 다른 캐릭터. 캐릭터 교체는 별도 절차 필요", this);
                return null;
            }
            
            existing.MoveToScenePosition(spawnPosition);
            return existing;
        }

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

        try
        {
            character.IBootStrapperInject(bootContext);
            character.Initialize(playerData);
        }
        catch (System.Exception exception)
        {
            character.gameObject.SetActive(false); // 실패한 객체가 다음 프레임에 실행되지 않게 한다.
            Destroy(character.gameObject);
            Debug.LogException(exception, this);
            return null;
        }

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

        if (!initialized)
        {
            Debug.LogWarning("Factory 부트 초기화 이후 테스트할 것", this);
            return;
        }

        IReadOnlyList<PlayerData> characters = GetAvailableCharacters();

        if (testCharacterIndex < 0 || testCharacterIndex >= characters.Count)
        {
            Debug.LogWarning($"캐릭터 목록은 0~{characters.Count - 1}에서만 사용 가능.", this);
            return;
        }

        CharacterControllers character = Create(characters[testCharacterIndex].id, spawnPosition);

        if (character == null)
        {
            Debug.LogError("생성 실패");
            return;
        }

        Debug.Log($"생성 성공 | Name : {character.name}");
    }
}
