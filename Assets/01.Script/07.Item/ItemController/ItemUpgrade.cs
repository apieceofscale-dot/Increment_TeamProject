public sealed class ItemUpgrade
{
    public int Level { get; private set; }
    public int BonusValue { get; private set; }

    public void Set(int level, int bonusValue)
    {
        Level = UnityEngine.Mathf.Max(0, level);
        BonusValue = UnityEngine.Mathf.Max(0, bonusValue);
    }

    public void Clear()
    {
        Level = 0;
        BonusValue = 0;
    }
}
