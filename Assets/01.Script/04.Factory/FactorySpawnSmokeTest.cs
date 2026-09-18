using UnityEngine;

/// <summary>
/// Play 모드에서 DataManager SO → Factory/Facade 스폰 API를 한 번 검증한다.
/// TestScene BootStrapper 또는 MainScene에 붙여 사용.
/// </summary>
public class FactorySpawnSmokeTest : MonoBehaviour
{
    [SerializeField] bool runOnPlay = true;
    [SerializeField] int monsterId = (int)MonsterId.Slime;
    [SerializeField] int itemId = (int)ItemId.HpPotion;
    [SerializeField] Vector3 monsterSpawnOffset = new Vector3(2f, 0f, 0f);
    [SerializeField] Vector3 itemSpawnOffset = new Vector3(-2f, 0f, 0f);
    [SerializeField] bool requestDropAfterDataCheck;

    void Start()
    {
        if (!runOnPlay)
        {
            return;
        }

        RunSmokeTest();
    }

    void RunSmokeTest()
    {
        if (DataManager.instance == null)
        {
            Debug.LogError("[FactorySpawnSmokeTest] DataManager.instance is null.");
            return;
        }

        if (!DataManager.instance.TryGetMonsterData(monsterId, out MonsterData monsterData))
        {
            Debug.LogError($"[FactorySpawnSmokeTest] MonsterData 없음. id={monsterId}");
            return;
        }

        if (!DataManager.instance.TryGetItemData(itemId, out ItemData itemData))
        {
            Debug.LogError($"[FactorySpawnSmokeTest] ItemData 없음. id={itemId}");
            return;
        }

        Debug.Log(
            $"[FactorySpawnSmokeTest] SO 로드 OK — monster={monsterData.displayName}, dropTableId={monsterData.dropTableId}, item={itemData.displayName}, icon={itemData.icon != null}, worldSprite={itemData.worldSprite != null}");

        Vector3 origin = transform.position;

        MonsterController spawnedMonster = TrySpawnMonster(origin + monsterSpawnOffset);
        ItemController spawnedItem = TrySpawnItem(origin + itemSpawnOffset);

        if (requestDropAfterDataCheck && monsterData.dropTableId > 0)
        {
            ItemDropFacade dropFacade = FindFirstObjectByType<ItemDropFacade>();
            if (dropFacade == null)
            {
                Debug.LogWarning("[FactorySpawnSmokeTest] ItemDropFacade 없음 — 드랍 API 스킵.");
            }
            else
            {
                dropFacade.RequestDrop(monsterData.dropTableId, origin);
                Debug.Log($"[FactorySpawnSmokeTest] ItemDropFacade.RequestDrop({monsterData.dropTableId}) enqueued.");
            }
        }

        if (spawnedMonster == null && spawnedItem == null)
        {
            Debug.LogWarning("[FactorySpawnSmokeTest] 씬에 MonsterFactory/ItemFactory(또는 Facade)를 배치했는지 확인하세요.");
        }
    }

    MonsterController TrySpawnMonster(Vector3 position)
    {
        MonsterFacade facade = FindFirstObjectByType<MonsterFacade>();
        if (facade != null)
        {
            MonsterController monster = facade.Spawn(monsterId, position, Quaternion.identity);
            if (monster != null)
            {
                Debug.Log($"[FactorySpawnSmokeTest] MonsterFacade.Spawn OK — DropTableId={monster.DropTableId}");
            }
            else
            {
                Debug.LogError("[FactorySpawnSmokeTest] MonsterFacade.Spawn 실패 (defaultPrefab·SO 확인).");
            }

            return monster;
        }

        if (MonsterFactory.Instance == null)
        {
            return null;
        }

        MonsterController fromFactory = MonsterFactory.Instance.Create(monsterId, position, Quaternion.identity);
        if (fromFactory != null)
        {
            Debug.Log("[FactorySpawnSmokeTest] MonsterFactory.Create OK");
        }

        return fromFactory;
    }

    ItemController TrySpawnItem(Vector3 position)
    {
        ItemFacade facade = FindFirstObjectByType<ItemFacade>();
        if (facade != null)
        {
            ItemController item = facade.Spawn(itemId, position, Quaternion.identity);
            if (item != null)
            {
                Debug.Log("[FactorySpawnSmokeTest] ItemFacade.Spawn OK");
            }
            else
            {
                Debug.LogError("[FactorySpawnSmokeTest] ItemFacade.Spawn 실패 (defaultPrefab·SO 확인).");
            }

            return item;
        }

        if (ItemFactory.Instance == null)
        {
            return null;
        }

        ItemController fromFactory = ItemFactory.Instance.Create(itemId, position, Quaternion.identity);
        if (fromFactory != null)
        {
            Debug.Log("[FactorySpawnSmokeTest] ItemFactory.Create OK");
        }

        return fromFactory;
    }
}
