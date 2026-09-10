using UnityEngine;

public abstract class CharacterSkillBase : MonoBehaviour
{
    [SerializeField] protected string skillName = "테스트 스킬";
    [SerializeField, Min(0)] protected int mpCost = 10;
    [SerializeField, Min(0.01f)] protected float cooldown = 3f;
    [SerializeField, Min(1)] private int initialLevel = 1;
    [SerializeField] private bool initiallyUnlocked = true;
    [SerializeField] private bool affectedByAttackSpeed;

    protected CharacterFacade characterFacade;
    protected CharacterSkill skill;

    private bool executing;
    public CharacterSkill RuntimeSkill { get { EnsureInitialized(); return skill; } }
    public virtual bool CanReplaceBasicAttack => false;
    public bool BelongsTo(CharacterFacade owner)
    {
        EnsureInitialized();
        return characterFacade == owner;
    }

    protected virtual void Awake()
    { 
        EnsureInitialized(); 
    }

    private void EnsureInitialized()
    {
        if (characterFacade == null)
            characterFacade = GetComponentInParent<CharacterFacade>();

        if (skill == null)
            skill = new CharacterSkill(skillName, initialLevel, mpCost, cooldown, initiallyUnlocked, () => Time.time);
    }

    public bool TryUse()
    {
        EnsureInitialized();

        if (!isActiveAndEnabled || executing || characterFacade == null || characterFacade.Status == null || characterFacade.Status.CurrentHp <= 0)
            return false;

        if (!skill.CanUse() || !CanUse())
            return false;

        if (characterFacade.Status.CurrentMp < skill.MpCost)
            return false;

        float interval = GetUseInterval();

        if (!skill.TryUse(interval))
            return false;

        characterFacade.UseMp(skill.MpCost);
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
        float rate = affectedByAttackSpeed && characterFacade != null && characterFacade.Status != null ? Mathf.Clamp(characterFacade.Status.AttackSpeedRate, 0f, 1.5f) : 0f;
        return Mathf.Max(0.01f, skill.Cooldown) / (1f + rate);
    }

    protected virtual bool CanUse() { return true; }
    protected abstract void Execute();
}
