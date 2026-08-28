using UnityEngine;

public sealed class ItemEnchantProvider
{
    public const float GrowthRate = 1.1f;
    public static readonly ItemEnchantProvider Default = new ItemEnchantProvider();

    public int Apply(int baseDamage, int starForce)
    {
        if (baseDamage <= 0)
        {
            return 0;
        }

        var star = Mathf.Max(0, starForce);
        if (star == 0)
        {
            return baseDamage;
        }

        return Mathf.Max(1, Mathf.RoundToInt(baseDamage * Mathf.Pow(GrowthRate, star)));
    }
}
