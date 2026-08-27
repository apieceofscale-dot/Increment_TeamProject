using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DataRepositary<T> where T : BaseData //각종 data클래스
{
    private readonly Dictionary<int, T> dataByID = new Dictionary<int, T>();

    public void Load(List<T> source)
    {
        dataByID.Clear();

        if (source == null)
        {
            Debug.Log($"{typeof(T).Name} null");
            return;
        }

        for (int i = 0; i < source.Count; i++)
        {
            T original = source[i];

            if (original == null)
            {
                Debug.Log($"{typeof(T).Name} {i} 없음.");
                continue;
            }

            if (original.Clone() is not T clonedData)
            {
                Debug.LogError($"{typeof(T).Name}  {i}복제 실패");
                continue;
            }

            if (!dataByID.TryAdd(clonedData.id, clonedData)) //여기서 넣는거임.
            {
                Debug.LogError($"데이터 ID 중복: ID={clonedData.id}, {i}, {typeof(T).Name}");
            }

        }

    }
    public bool TryGet(int id, out T data) //아래 함수의 구조 자체가 이거임을 생각하면, 항상 이렇게 하는 게 덜 헷갈림. 
    {
        return dataByID.TryGetValue(id, out data);
    }

    public void Clear()
    {
        dataByID.Clear();
    }
}