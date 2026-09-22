using System.Collections.Generic;
using UnityEngine;

//���丮�� ���� ������ ���� å���� ��� �ִµ�, �̰� AssetManager�� �и��Ͽ� ��� �־�� ��. 
public partial class DataManager : MonoBehaviour, IBootStrapper
{
    public static DataManager instance;
    public int BootOrder => (int)BootLayer.DataManager;

    //�ڵ�����
    partial void LoadAllOfDataGenerated();
    private void LoadData<T>(DataRepositary<T> repositary, List<T> dataList) where T : BaseData
    {
        repositary.Clear();

        if(dataList == null )
        {
            Debug.Log($"{dataList} ������ Null");
            return;
        }

        repositary.Load(dataList);
    }


    //���丮 ȣ���    
    public bool TryGetMonsterData(int id, out MonsterData monsterData)//�Ʒ�tryget�� ���� ������� ������. ���⿡ out �����ϱ� ��� �ȵ�.
    {
        return monsterRepository.TryGet(id, out monsterData);
    }
    public bool TryGetPlayerData(int id, out PlayerData playerData)
    {
        return playerRepository.TryGet(id, out playerData);
    }
    public bool TryGetStageData(int id, out StageData stageData)
    {
        return stageRepository.TryGet(id, out stageData);
    }
    public bool TryGetItemData(int id, out ItemData itemData)
    {
        return itemRepository.TryGet(id, out itemData);
    }

    /// <summary>팩토리 등에서 SO 직렬화 없이 플레이어 목록 id를 순회할 때 사용.</summary>
    public bool TryGetPlayerCatalogEntries(out System.Collections.Generic.IReadOnlyList<PlayerData> entries)
    {
        entries = null;
        if (playerList == null || playerList.baseList == null || playerList.baseList.Count == 0)
            return false;

        entries = playerList.baseList;
        return true;
    }



    //��Ʈ ��Ʈ���ۿ�.
    public void IBootStrapperInject(BootstrapContext context)
    {        
        instance = this;
     
    }

    public void IBootStrapperInitialize()
    {
        LoadAllOfDataGenerated();
    }

}






/*
 * public static DataManager instance;

  
   // private readonly List<Monster> thisMonsterList = new List<Monster>(); //readonly�� ���� const�� ���. 
    //private readonly Dictionary<int, Monster> monsters = new Dictionary<int, Monster>();
    [SerializeField] private MonsterList monsterData;  
    private readonly DataRepositary<Monster> monsters = new DataRepositary<Monster>();

    [SerializeField] private PlayerList playerData;
    private readonly DataRepositary<Player> players = new DataRepositary<Player>();

    [SerializeField] private WeaponList weaponData;
    private readonly DataRepositary<Weapon> weapons = new DataRepositary<Weapon>();

    //���⿡ ������ �ϳ��� ���� ���� ������� �ֱ�. �Ʒ�  LoadAllOFData()���� �ֱ� . ���߿� ��ũ���ͺ� ������Ʈ��



    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
        

        LoadAllOFData();
    }

    private void LoadAllOFData() //�� ������ ��.
    {
        LoadWeapon();
        LoadMonster();
        LoadPlayer();
        
    }


    private void LoadMonster()
    {        
        monsters.Clear();

        if(monsterData == null)
        {
            Debug.Log("�ν����Ϳ��� ���� ������ �־����");
            return;
        }
        monsters.Load(monsterData.monsterList); 
    }

    private void LoadPlayer()
    {
        players.Clear();

        if(playerData == null)
        {
            Debug.Log("�ν����Ϳ��� �÷��̾� ������ �־����");
            return;
        }
        players.Load(playerData.playerList); 
    }

    private void LoadWeapon()
    {
        weapons.Clear();
        if(weaponData == null)
        {
            Debug.Log("�ν����Ϳ��� ���� ������ �־����");
        }

        weapons.Load(weaponData.weaponList);
    }



    public bool TryGetMonsterData(int id, out Monster monsterData)//�Ʒ�tryget�� ���� ������� ������. ���⿡ out �����ϱ� ��� �ȵ�.
    {
        return monsters.TryGet(id, out monsterData);
    }
    public bool TryGetPlayerData(int id, out Player playerData)
    {
        return players.TryGet(id, out playerData);
    }
    public bool TryGetWeaponData(int id, out Weapon weaponData)
    {
        return  weapons.TryGet(id, out weaponData);
    }
}
 */