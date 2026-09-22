using UnityEngine;

[DisallowMultipleComponent]
public class CharacterPlayModeTests : MonoBehaviour
{
#if UNITY_EDITOR
    private void RunTest(System.Action<CharacterControllers> action)
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("플레이 모드에서만 실행 가능", this);
            return;
        }

        CharacterControllers controller = GetComponentInParent<CharacterControllers>();

        if (controller == null || !controller.CanRun || !controller.isActiveAndEnabled)
        {
            Debug.LogWarning("초기화가 완료된 활성 캐릭터에서 실행", this);
            return;
        }

        action(controller);
        Debug.Log($"테스트 후 상태 | Lv.{controller.Status.Level} / Exp: {controller.Status.Exp} / HP: {controller.Status.CurrentHp}/{controller.Status.MaxHp} / MP: {controller.Status.CurrentMp}/{controller.Status.MaxMp}", this);
    }

    [ContextMenu("캐릭터 테스트/경험치 100 획득")]
    private void GainExp()
    {
        RunTest(c => { c.GainExp(100); });
    }

    [ContextMenu("캐릭터 테스트/피해 10 받기")]
    private void TakeDamage()
    {
        RunTest(c => { c.Status.TakeDamage(10); });
    }

    [ContextMenu("캐릭터 테스트/HP 10 회복")]
    private void RecoverHp()
    {
        RunTest(c => { c.Status.RecoverHp(10); });
    }

    [ContextMenu("캐릭터 테스트/MP 10 사용")]
    private void UseMp()
    {
        RunTest(c => { Debug.Log(c.Status.UseMp(10) ? "MP 사용 성공" : "MP 부족", this); });
    }

    [ContextMenu("캐릭터 테스트/MP 20 회복")]
    private void RecoverMp()
    {
        RunTest(c => { c.Status.RecoverMp(20); });
    }

    [ContextMenu("캐릭터 테스트/베기 스킬 사용")]
    private void UseSlash()
    {
        RunTest(c => { c.UseSkillSlash(); });
    }

    [ContextMenu("캐릭터 테스트/기존 투사체 스킬 사용")]
    private void UseProjectile()
    {
        RunTest(c => { c.UseSkillProjectile(); });
    }

    [ContextMenu("캐릭터 테스트/공격 버프 사용")]
    private void UseAttackBuff()
    {
        RunTest(c => { c.UseSkillAttackBuff(); });
    }

    [ContextMenu("캐릭터 테스트/다음 전직")]
    private void ChangeJob()
    {
        RunTest(c => { c.ChangeNextJob(); });
    }

    [ContextMenu("캐릭터 테스트/기본 공격 복원")]
    private void RestoreAttack()
    {
        RunTest(c => { c.RestoreBasicAttack(); });
    }

    [ContextMenu("캐릭터 테스트/베기로 기본 공격 교체")]
    private void SlashAttack()
    {
        RunTest(c => { Debug.Log(c.SetSlashBasicAttack() ? "베기 교체 성공" : "베기 연결 확인 필요", this); });
    }

    [ContextMenu("캐릭터 테스트/투사체로 기본 공격 교체")]
    private void ProjectileAttack()
    {
        RunTest(c => { Debug.Log(c.SetProjectileBasicAttack() ? "투사체 교체 성공" : "투사체 연결 확인 필요", this); });
    }

    [ContextMenu("캐릭터 테스트/기존 테스트 스킬 강화")]
    private void LevelUpSkill()
    {
        RunTest(c => { c.TestSkillLevelUp(); });
    }

#endif
}
