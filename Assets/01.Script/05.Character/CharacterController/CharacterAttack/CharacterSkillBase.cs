using UnityEngine;

public abstract class CharacterSkillBase : MonoBehaviour
{
    [SerializeField] protected string skillName = "테스트 스킬";
    [SerializeField, Min(0)] protected int mpCost = 10;
    [SerializeField, Min(0.01f)] protected float cooldown = 3f;
    [SerializeField, Min(1)] private int initialLevel = 1;
    [SerializeField] private bool initiallyUnlocked = true;
    [SerializeField] private bool affectedByAttackSpeed;
    [SerializeField] private Sprite skillIcon;
    public Sprite SkillIcon => skillIcon;

    protected CharacterControllers characterControllers;
    protected CharacterSkill skill;

    private bool executing;
    public CharacterSkill RuntimeSkill => skill;
    public virtual bool CanReplaceBasicAttack => false;
    public bool BelongsTo(CharacterControllers owner)
    {
        return skill != null && owner != null && characterControllers == owner;
    }

    public void Initialize(CharacterControllers owner)
    {
        if (owner == null || GetComponentInParent<CharacterControllers>() != owner)
            throw new System.InvalidOperationException("스킬을 자식에 배치");

        if (skill != null)
        {
            if (characterControllers != owner)
                throw new System.InvalidOperationException("스킬 소유자 변경은 없음");

            return; // 쿨타임·강화 상태를 중복 초기화하지 않는다.
        }

        characterControllers = owner;
        skill = new CharacterSkill(skillName, initialLevel, mpCost, cooldown, initiallyUnlocked, () => Time.time);
    }

    public bool TryUse()
    {
        if (skill == null || characterControllers == null || !characterControllers.CanRun)
            return false;

        if (!isActiveAndEnabled || executing || characterControllers == null || characterControllers.Status == null || characterControllers.Status.CurrentHp <= 0)
            return false;

        if (!skill.CanUse() || !CanUse())
            return false;

        if (characterControllers.Status.CurrentMp < skill.MpCost)
            return false;

        float interval = GetUseInterval();

        if (!skill.TryUse(interval))
            return false;

        characterControllers.Status.UseMp(skill.MpCost);
        executing = true;

        try
        {
            Execute();
        }
        finally
        {
            executing = false;
        }

        return true;
    }

    public float GetUseInterval()
    {
        if (skill == null)
            return float.PositiveInfinity;

        float rate = affectedByAttackSpeed && characterControllers != null && characterControllers.Status != null ? Mathf.Clamp(characterControllers.Status.AttackSpeedRate, 0f, 1.5f) : 0f;
        
        return Mathf.Max(0.01f, skill.Cooldown) / (1f + rate);
    }

    protected virtual bool CanUse() { return true; }
    protected abstract void Execute();
}
