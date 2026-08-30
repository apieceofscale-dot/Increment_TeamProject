using UnityEngine;

public class DataManager : MonoBehaviour, IBootStrapper
{
    public static DataManager instance;

    [SerializeField] MonsterList monsterData;
    readonly DataRepositary<MonsterData> monsters = new DataRepositary<MonsterData>();

    [SerializeField] ItemList itemData;
    readonly DataRepositary<ItemData> items = new DataRepositary<ItemData>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public void IBootStrapperInitialize(BootstrapContext context)
    {
        if (instance == null)
        {
            instance = this;
        }

        LoadAllOFData();
        context.OnStepCompleted?.Invoke();
    }

    void LoadAllOFData()
    {
        LoadMonster();
        LoadItem();
    }

    void LoadMonster()
    {
        monsters.Clear();
        if (monsterData == null || monsterData.monsterList == null || monsterData.monsterList.Count == 0)
        {
            monsters.Load(CreateFallbackMonsters());
            return;
        }

        monsters.Load(monsterData.monsterList);
    }

    void LoadItem()
    {
        items.Clear();
        if (itemData == null || itemData.itemList == null || itemData.itemList.Count == 0)
        {
            items.Load(CreateFallbackItems());
            return;
        }

        items.Load(itemData.itemList);
    }

    public bool TryGetMonsterData(int id, out MonsterData monster)
    {
        return monsters.TryGet(id, out monster);
    }

    public bool TryGetItemData(int id, out ItemData item)
    {
        return items.TryGet(id, out item);
    }

    static System.Collections.Generic.List<MonsterData> CreateFallbackMonsters()
    {
        return new System.Collections.Generic.List<MonsterData>
        {
            new MonsterData
            {
                id = 1,
                name = "DefaultMonster",
                maxHp = 10,
                attackDamage = 1,
                moveSpeed = 1.5f,
                traceRange = 6f,
                attackRange = 1.4f,
                attackCooldown = 1f,
                dropItemId = 1,
                dropChance = 1f
            }
        };
    }

    static System.Collections.Generic.List<ItemData> CreateFallbackItems()
    {
        return new System.Collections.Generic.List<ItemData>
        {
            new ItemData
            {
                id = 1,
                name = "DefaultDrop",
                type = ItemType.Currency,
                value = 1,
                upgradeStep = 1
            }
        };
    }
}
