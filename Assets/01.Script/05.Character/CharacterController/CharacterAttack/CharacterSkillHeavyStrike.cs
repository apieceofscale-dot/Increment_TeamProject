using UnityEngine;

public sealed class CharacterSkillHeavyStrike : CharacterSkillAreaVariation // 짧은 범위 높은 배율로 1회 타격
{
    protected override Vector2 AreaSize => new Vector2(1.8f, 2.0f);
    protected override float ForwardDistance => 1.0f;
    protected override float DamageMultiplier => 3.0f;
    protected override int HitCount => 1;
}
