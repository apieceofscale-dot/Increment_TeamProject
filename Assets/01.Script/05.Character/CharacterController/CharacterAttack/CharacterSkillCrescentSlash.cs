using UnityEngine;

public sealed class CharacterSkillCrescentSlash : CharacterSkillAreaVariation // 넓은 전방 범위 1회 타격
{
    protected override Vector2 AreaSize => new Vector2(4.0f, 2.0f);
    protected override float ForwardDistance => 1.8f;
    protected override float DamageMultiplier => 1.4f;
    protected override int HitCount => 1;
}
