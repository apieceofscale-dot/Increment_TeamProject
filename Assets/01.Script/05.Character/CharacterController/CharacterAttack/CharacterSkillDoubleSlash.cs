using UnityEngine;

public sealed class CharacterSkillDoubleSlash : CharacterSkillAreaVariation // 전방 범위 2회 타격
{
    protected override Vector2 AreaSize => new Vector2(2.6f, 2.0f);
    protected override float ForwardDistance => 1.3f;
    protected override float DamageMultiplier => 0.9f;
    protected override int HitCount => 2;
}
