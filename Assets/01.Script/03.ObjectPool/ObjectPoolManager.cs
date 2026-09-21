using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager<T> : MonoBehaviour where T : Component
{
    public static ObjectPoolManager<T> instance;

    // ?  ?
    Dictionary<T, Queue<T>> pools = new Dictionary<T, Queue<T>>();
    // string->?, Queue ->  . 
    Dictionary<T, Transform> poolsParents = new Dictionary<T, Transform>();
    //    . TryGetValue
    Dictionary<T, T> originPrefabs = new Dictionary<T, T>();
    //? ?  ?.
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

       
    }

    public void MakeFirstPools(T prefab, int count = 30) //Prewarming()
    {
        if (prefab == null)
            return;

        ObjectPoolMaker(prefab, count);
    }


    public void ObjectPoolMaker(T prefab, int n) //creatPool()
    {
        if (pools.ContainsKey(prefab))
            return;

        poolSize = n;

        GameObject parentPool = new GameObject($"{prefab.name}_Pool");   // ?   {obj.name}_Pool ?   ?.
        parentPool.transform.SetParent(transform);                  //?  parentPool ?  ? .
                                                                    //   ? ? {obj.name}_Pool ??.
        poolsParents[prefab] = parentPool.transform;                // ?  ? .
        pools[prefab] = new Queue<T>();                    //?? ? ? ?, ? . 

        for (int i = 0; i < poolSize; i++)
        {
            T go = CreatePooledObject(prefab);
            go.gameObject.SetActive(false);
            pools[prefab].Enqueue(go);
        }
    }

    public T CreatePooledObject(T prefab) //? ?   ?? ?? ?.
    {
        T go = Instantiate(prefab, poolsParents[prefab]);

        originPrefabs[go] = prefab; //  .

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
            return null; //?  ????
        }

        if (!pools.ContainsKey(prefab)) //? ?    ?+?   . ? ?   ?.
        {
            ObjectPoolMaker(prefab, 30);
        }

        T go;

        if (pools[prefab].Count > 0)
        {
            go = pools[prefab].Dequeue();

        }
        else //? ?   . ?  ? .
        {
            go = CreatePooledObject(prefab);

        }

        activedObjects.Add(go);
        go.gameObject.SetActive(true);

        // OnSpawn/OnDespawn IPoolable  + Factory( Initialize ) .
        // GetObject ?  ?? ?.

        return go;
    }

    public T GetObject(T prefab, Vector3 position, Quaternion rotation) //  ? ?  ?    ?
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

        if (!originPrefabs.TryGetValue(go, out T originPrefab)) // int tryparse? ?  .
                                                                // ?   ?.
        {
            Debug.Log("???");
            activedObjects.Add(go);
            return;
        }

        if (go is IPoolable poolable)
            poolable.OnDespawn();

        go.gameObject.SetActive(false);
        go.transform.SetParent(poolsParents[originPrefab]); // ? ?.
        pools[originPrefab].Enqueue(go);
        //activedObjects.Remove(go);
    }

    public void ReturnAllobjects()
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





