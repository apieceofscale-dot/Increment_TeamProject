
using System;
using System.Collections.Generic;
using UnityEngine;



//제네릭화 -> 자식 스크립트로 비 제네릭 몬스터/미사일 풀링 매니저 따로 만들기.

public class ObjectPoolManager<T> : MonoBehaviour where T : Component
{
    public static ObjectPoolManager<T> instance;

    List<T> objList; //스테이지 매니저가 초기화
    //실제 오브젝트 저장 위치
    Dictionary<T, Queue<T>> pools = new Dictionary<T, Queue<T>>();
    //즉 string->이름, Queue ->저장 형태가 됨. 
    Dictionary<T, Transform> poolsParents = new Dictionary<T, Transform>();
    //프리팹의 원본이 뭔지 저장할 것. TryGetValue
    Dictionary<T, T> originPrefabs = new Dictionary<T, T>();
    //살아있는 오브젝트들 다 돌려받는용.
    private HashSet<T> activedObjects = new HashSet<T>();




    int poolSize;

    protected virtual void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        objList = new List<T>();
    }

    public void MakeFirstPools(List<T> stageManagerList) // func to get List from StageManager, and called by this too
    {
        foreach (T obj in stageManagerList)
        {
            objList.Add(obj); //? 이거 게임 오브젝트 아닌가?
        }

        poolSize = 30;
        foreach (T obj in objList)
        {
            ObjectPoolMaker(obj, poolSize);
        }
    }


    public void ObjectPoolMaker(T prefab, int n)
    {
        if (pools.ContainsKey(prefab))
            return;

        poolSize = n;

        GameObject parentPool = new GameObject($"{prefab.name}_Pool");   // 일단 정리를 위해 {obj.name}_Pool 이름의 빈 게임 오브젝트를생성.
        parentPool.transform.SetParent(transform);                  //그리고 이 parentPool의 부모를 이 오브젝트로 함.
                                                                    //즉 이  오브젝트 아래에 {obj.name}_Pool가 있게됨.
        poolsParents[prefab] = parentPool.transform;                // 그리고 그 위치도 저장.
        pools[prefab] = new Queue<T>();                    //딕셔너리의 키로 이름을 넣고, 큐를 선언해줌. 

        for (int i = 0; i < poolSize; i++)
        {
            T go = CreatPooledObject(prefab);
            go.gameObject.SetActive(false);
            pools[prefab].Enqueue(go);
        }
    }

    public T CreatPooledObject(T prefab) //부족해서 생성해야 할 경우와 코드가 중복되서 분리함.
    {
        T go = Instantiate(prefab, poolsParents[prefab]);

        originPrefabs[go] = prefab; //생성된 프리팹을 저장.

        return go;
    }

    public T GetObject(T prefab) // 
    {
        if (prefab == null)
        {
            Debug.Log("???");
            return null; //그럼 안 쏘게????
        }

        if (!pools.ContainsKey(prefab)) //오브젝트 풀 자체가 없을 경우 부모+저장위치 까지 같이 생성. 무기를 도중에 얻을 때 필요함.
        {
            ObjectPoolMaker(prefab, 30);
        }

        T go;
        if (pools[prefab].Count > 0)
        {
            go = pools[prefab].Dequeue();

        }
        else //풀은 있는데 부족할 경우 생성. 이때 저장 위치 기억.
        {
            go = CreatPooledObject(prefab);

        }

        activedObjects.Add(go);
        go.gameObject.SetActive(true);

        return go;
    }




    public void ReturnObject(T go)
    {
        if (go == null)
        {
            Debug.Log("???");
            return;
        }
        if (!activedObjects.Remove(go))
        {
            Debug.Log($"???");
            return;
        }

        if (!originPrefabs.TryGetValue(go, out T originPrefab)) // int tryparse했던 거랑 같은 거.
                                                                // 여기서는 원본 프리팹을 찾는다.
        {
            Debug.Log("???");
            activedObjects.Add(go);
            return;
        }

        go.gameObject.SetActive(false);
        go.transform.SetParent(poolsParents[originPrefab]); //저장 위치 찾아가기.
        pools[originPrefab].Enqueue(go);
        activedObjects.Remove(go);


    }

    public void ReturnAllobject()
    {
        T[] objects = new T[activedObjects.Count];
        activedObjects.CopyTo(objects);

        foreach (T obj in objects)
        {
            if (obj == null)
            {
                continue;
            }

            ReturnObject(obj);
        }
    }
}





