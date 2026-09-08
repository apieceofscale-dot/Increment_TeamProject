
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public abstract class BaseData
{
    public int id;
    public string codeName;
    public string description;
    public string displayName;


    public abstract BaseData Clone();
    
}
   

public abstract class BaseList<T> : ScriptableObject where T : BaseData
{
    public List<T> baseList;
}