using UnityEngine;

public abstract class CharacterSkillBase : MonoBehaviour
{
    [SerializeField] protected string skillName = "테스트 스킬";
    [SerializeField, Min(0)] protected int mpCost = 10;
    [SerializeField, Min(0.01f)] protected float cooldown = 3f;
    [SerializeField, Min(1)] private int initialLevel = 1;
    [SerializeField] private bool initiallyUnlocked = true;
    [SerializeField] private bool affectedByAttackSpeed;

    protected CharacterControllers characterControllers;
    protected CharacterSkill skill;

    private bool executing;
    public CharacterSkill RuntimeSkill { get { EnsureInitialized(); return skill; } }
    public virtual bool CanReplaceBasicAttack => false;
    public bool BelongsTo(CharacterControllers owner)
    {
        EnsureInitialized();
        return characterControllers == owner;
    }

    protected virtual void Awake()
    { 
        EnsureInitialized(); 
    }

    private void EnsureInitialized()
    {
        if (characterControllers == null)
            characterControllers = GetComponentInParent<CharacterControllers>();

        if (skill == null)
            skill = new CharacterSkill(skillName, initialLevel, mpCost, cooldown, initiallyUnlocked, () => Time.time);
    }

    public bool TryUse()
    {
        EnsureInitialized();

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
        EnsureInitialized();
        float rate = affectedByAttackSpeed && characterControllers != null && characterControllers.Status != null ? Mathf.Clamp(characterControllers.Status.AttackSpeedRate, 0f, 1.5f) : 0f;
        return Mathf.Max(0.01f, skill.Cooldown) / (1f + rate);
    }

    protected virtual bool CanUse() { return true; }
    protected abstract void Execute();
}
