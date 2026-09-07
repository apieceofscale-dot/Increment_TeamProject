using UnityEngine;

public abstract class CharacterSkillBase : MonoBehaviour
{
    [SerializeField] protected string skillName;
    [SerializeField] protected int mpCost = 10;
    [SerializeField] protected float cooldown = 3f;

    protected CharacterFacade characterFacade;
    private float lastUsedTime = -999f;

    protected virtual void Awake()
    {
        characterFacade = GetComponent<CharacterFacade>();
    }

    public bool TryUse()
    {
        if (characterFacade == null)
            return false;
        if (!CanUse())
            return false;
        if (!characterFacade.UseMp(mpCost))
            return false;

        lastUsedTime = Time.time;
        Execute();

        return true;
    }

    protected virtual bool CanUse()
    {
        return Time.time >= lastUsedTime + cooldown;
    }

    protected abstract void Execute();
}
