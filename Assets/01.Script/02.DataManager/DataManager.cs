using System.Collections.Generic;
using UnityEngine;

public partial class DataManager : MonoBehaviour
{
    public static DataManager instance;


    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
       
        LoadAllOfDataGenerated();
    }
    //자동구현
    partial void LoadAllOfDataGenerated();

    private void LoadData<T>(DataRepositary<T> repositary, List<T> dataList) where T : BaseData
    {
        repositary.Clear();

        if(dataList == null )
        {
            Debug.Log($"{dataList} 데이터 Null");
        }

        repositary.Load(dataList);
    }





    /*
    public bool TryGetMonsterData(int id, out MonsterData monsterData)//아래tryget랑 같은 방식으로 쓴거임. 여기에 out 없으니까 출력 안됨.
    {
        return monsters.TryGet(id, out monsterData);
    }
    public bool TryGetPlayerData(int id, out PlayerData playerData)
    {
        return players.TryGet(id, out playerData);
    }
    public bool TryGetWeaponData(int id, out StageData stageData)
    {
        return stages.TryGet(id, out stageData);
    }
    public bool TryGetItemData(int id, out ItemData itemData)
    {
        return items.TryGet(id, out itemData);
    }
    */
}






/*
 * public static DataManager instance;

  
   // private readonly List<Monster> thisMonsterList = new List<Monster>(); //readonly는 대충 const랑 비슷. 
    //private readonly Dictionary<int, Monster> monsters = new Dictionary<int, Monster>();
    [SerializeField] private MonsterList monsterData;  
    private readonly DataRepositary<Monster> monsters = new DataRepositary<Monster>();

    [SerializeField] private PlayerList playerData;
    private readonly DataRepositary<Player> players = new DataRepositary<Player>();

    [SerializeField] private WeaponList weaponData;
    private readonly DataRepositary<Weapon> weapons = new DataRepositary<Weapon>();

    //여기에 데이터 하나씩 위랑 같은 방식으로 넣기. 아래  LoadAllOFData()에도 넣기 . 나중에 스크립터블 오브젝트로



    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
        

        LoadAllOFData();
    }

    private void LoadAllOFData() //걍 정리한 거.
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
            Debug.Log("인스펙터에서 몬스터 데이터 넣어라좀");
            return;
        }
        monsters.Load(monsterData.monsterList); 
    }

    private void LoadPlayer()
    {
        players.Clear();

        if(playerData == null)
        {
            Debug.Log("인스펙터에서 플레이어 데이터 넣어라좀");
            return;
        }
        players.Load(playerData.playerList); 
    }

    private void LoadWeapon()
    {
        weapons.Clear();
        if(weaponData == null)
        {
            Debug.Log("인스펙터에서 무기 데이터 넣어라좀");
        }

        weapons.Load(weaponData.weaponList);
    }



    public bool TryGetMonsterData(int id, out Monster monsterData)//아래tryget랑 같은 방식으로 쓴거임. 여기에 out 없으니까 출력 안됨.
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