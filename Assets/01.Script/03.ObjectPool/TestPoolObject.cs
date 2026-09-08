using System;
using UnityEngine;

public class TestPoolObject : MonoBehaviour, IPoolable
{
    private Action returnAction;

    public void InitializePoolObj(Action returnAction)
    {
        this.returnAction = returnAction;

        Debug.Log($"{name} : InitializePoolObj");
    }

    public void OnSpawn()
    {
        Debug.Log($"{name} : OnSpawn");
    }

    public void OnDespawn()
    {
        Debug.Log($"{name} : OnDespawn");
    }

    public void ReturnToPool()
    {
        returnAction?.Invoke();
    }
}