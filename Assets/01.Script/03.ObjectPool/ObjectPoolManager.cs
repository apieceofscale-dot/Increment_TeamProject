
using System;
using System.Collections.Generic;
using UnityEngine;



//���׸�ȭ -> �ڽ� ��ũ��Ʈ�� �� ���׸� ����/�̻��� Ǯ�� �Ŵ��� ���� �����.

public class ObjectPoolManager<T> : MonoBehaviour where T : Component
{
    public static ObjectPoolManager<T> instance;

    List<T> objList; //�������� �Ŵ����� �ʱ�ȭ
    //���� ������Ʈ ���� ��ġ
    Dictionary<T, Queue<T>> pools = new Dictionary<T, Queue<T>>();
    //�� string->�̸�, Queue ->���� ���°� ��. 
    Dictionary<T, Transform> poolsParents = new Dictionary<T, Transform>();
    //�������� ������ ���� ������ ��. TryGetValue
    Dictionary<T, T> originPrefabs = new Dictionary<T, T>();
    //����ִ� ������Ʈ�� �� �����޴¿�.
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
            objList.Add(obj); //? �̰� ���� ������Ʈ �ƴѰ�?
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

        GameObject parentPool = new GameObject($"{prefab.name}_Pool");   // �ϴ� ������ ���� {obj.name}_Pool �̸��� �� ���� ������Ʈ������.
        parentPool.transform.SetParent(transform);                  //�׸��� �� parentPool�� �θ� �� ������Ʈ�� ��.
                                                                    //�� ��  ������Ʈ �Ʒ��� {obj.name}_Pool�� �ְԵ�.
        poolsParents[prefab] = parentPool.transform;                // �׸��� �� ��ġ�� ����.
        pools[prefab] = new Queue<T>();                    //��ųʸ��� Ű�� �̸��� �ְ�, ť�� ��������. 

        for (int i = 0; i < poolSize; i++)
        {
            T go = CreatePooledObject(prefab);
            go.gameObject.SetActive(false);
            pools[prefab].Enqueue(go);
        }
    }

    public T CreatePooledObject(T prefab) //�����ؼ� �����ؾ� �� ���� �ڵ尡 �ߺ��Ǽ� �и���.
    {
        T go = Instantiate(prefab, poolsParents[prefab]);

        originPrefabs[go] = prefab; //������ �������� ����.

        if (go is IPoolable poolable)
        {
            poolable.InitializePoolObj(() => ReturnObject(go));
        }

        return go;
    }

    public T GetObject(T prefab) // 
    {
        if (prefab == null)
        {
            Debug.Log("???");
            return null; //�׷� �� ���????
        }

        if (!pools.ContainsKey(prefab)) //������Ʈ Ǯ ��ü�� ���� ��� �θ�+������ġ ���� ���� ����. ���⸦ ���߿� ���� �� �ʿ���.
        {
            ObjectPoolMaker(prefab, 30);
        }

        T go;

        if (pools[prefab].Count > 0)
        {
            go = pools[prefab].Dequeue();

        }
        else //Ǯ�� �ִµ� ������ ��� ����. �̶� ���� ��ġ ���.
        {
            go = CreatePooledObject(prefab);

        }

        activedObjects.Add(go);
        go.gameObject.SetActive(true);

        // OnSpawn/OnDespawn�� IPoolable ����ü + Factory(������ Initialize ��)���� ó��.
        // GetObject�� Ǯ���� ���� Ȱ��ȭ�� �Ѵ�.

        return go;
    }

    public T GetObject(T prefab, Vector3 position, Quaternion rotation) // ������ �ٷ� ��ġ �� ȸ�� �� ���� ������ �����ε�
    {
        T go = GetObject(prefab);

        if (go == null)
            return null;

        go.transform.SetPositionAndRotation(position, rotation);

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

        if (!originPrefabs.TryGetValue(go, out T originPrefab)) // int tryparse�ߴ� �Ŷ� ���� ��.
                                                                // ���⼭�� ���� �������� ã�´�.
        {
            Debug.Log("???");
            activedObjects.Add(go);
            return;
        }

        if (go is IPoolable poolable)
            poolable.OnDespawn();

        go.gameObject.SetActive(false);
        go.transform.SetParent(poolsParents[originPrefab]); //���� ��ġ ã�ư���.
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





