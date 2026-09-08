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

    public void ApplyStage(MonsterStatus status, int stageIndex)
    {
        if (status == null)
        {
            return;
        }

        var stage = Mathf.Max(1, stageIndex);
        status.ApplyStageMultiplier(1f + (stage - 1) * 0.25f);
    }

    public Color GetPalette(int stageIndex)
    {
        var index = Mathf.Max(0, stageIndex - 1) % Palette.Length;
        return Palette[index];
    }
}
