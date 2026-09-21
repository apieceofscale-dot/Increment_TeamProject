using UnityEngine;

public static class CharacterDamageUtility
{
    public static CharacterDamageAttackData Capture(CharacterStatus status, CharacterDamageMainStat mainStat)
    {
        if (status == null)
            return default;

        double main;

        switch (mainStat)
        {
            case CharacterDamageMainStat.Strength:
                main = status.Strength; break;
            case CharacterDamageMainStat.Dexterity:
                main = status.Dexterity; break;
            case CharacterDamageMainStat.Intelligence:
                main = status.Intelligence; break;
            case CharacterDamageMainStat.Luck:
                main = status.Luck; break;
            default:
                throw new System.ArgumentOutOfRangeException(nameof(mainStat));
        }
        return new CharacterDamageAttackData(status.Attack, main, status.DamageByMainStat, status.CriticalRate, status.CriticalDamage, status.FinalDamage, status.HitRate, status.ArmorPenetration, status.DamageOnNormal, status.DamageOnBoss);
    }

    public static CharacterDamageResult Apply(CharacterDamageAttackData attack, float multiplier, IDamageable target)
    {
        if (target == null || (target is Object unityObject && unityObject == null))
            return default;

        CharacterDamageTargetData data = default;
        ICharacterDamageTarget provider = target as ICharacterDamageTarget;

        if (provider == null && target is Component component)
            provider = component.GetComponent<ICharacterDamageTarget>();

        if (provider != null)
            data = provider.GetDamageTargetData();

        double hitRoll = Random.Range(0, 16777216) / 16777216.0;
        double criticalRoll = Random.Range(0, 16777216) / 16777216.0;
        CharacterDamageResult result = CharacterDamageCalculator.Calculate(attack, multiplier, data, hitRoll, criticalRoll);

        if (!result.IsMiss && result.Damage > 0)
            target.TakeDamage(result.Damage);

        return result;
    }
}
