
using System;

[Serializable]
public abstract class BaseData
{
    public int id;
    public string name;
    public string description;

    public string displayName;

    public abstract BaseData Clone();
    
}
   