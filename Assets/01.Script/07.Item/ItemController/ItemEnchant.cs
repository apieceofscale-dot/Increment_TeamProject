public sealed class ItemEnchant
{
    public int StarForce { get; private set; }

    public void Set(int starForce)
    {
        StarForce = UnityEngine.Mathf.Max(0, starForce);
    }

    public void Clear()
    {
        StarForce = 0;
    }
}
