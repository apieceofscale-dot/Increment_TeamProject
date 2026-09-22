using UnityEngine;

public sealed class CharacterSkillShockwave : CharacterSkillAreaVariation // 길고 낮은 전방 범위 즉시 1회 타격
{
    protected override Vector2 AreaSize => new Vector2(6.0f, 1.6f);
    protected override float ForwardDistance => 3.0f;
    protected override float DamageMultiplier => 1.6f;
    protected override int HitCount => 1;
}
