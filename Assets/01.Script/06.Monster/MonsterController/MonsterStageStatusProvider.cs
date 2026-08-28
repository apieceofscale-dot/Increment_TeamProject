using UnityEngine;

public sealed class MonsterStageStatusProvider
{
    public static readonly MonsterStageStatusProvider Default = new MonsterStageStatusProvider();

    static readonly Color[] Palette =
    {
        new Color(0.85f, 0.35f, 0.35f),
        new Color(0.35f, 0.75f, 0.45f),
        new Color(0.35f, 0.45f, 0.90f),
        new Color(0.90f, 0.75f, 0.25f)
    };

    public MonsterData ApplyStage(MonsterData baseData, int stageIndex)
    {
        if (baseData == null)
        {
            return null;
        }

        var stage = Mathf.Max(1, stageIndex);
        var multiplier = 1f + (stage - 1) * 0.25f;
        var scaled = (MonsterData)baseData.Clone();
        scaled.maxHp = Mathf.Max(1, Mathf.RoundToInt(baseData.maxHp * multiplier));
        scaled.attackDamage = Mathf.Max(1, Mathf.RoundToInt(Mathf.Max(1, baseData.attackDamage) * multiplier));
        scaled.moveSpeed = baseData.moveSpeed * multiplier;
        return scaled;
    }

    public Color GetPalette(int stageIndex)
    {
        var index = Mathf.Max(0, stageIndex - 1) % Palette.Length;
        return Palette[index];
    }
}
