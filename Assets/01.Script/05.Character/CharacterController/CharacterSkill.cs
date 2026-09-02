using System;

public class CharacterSkill
{
    public string SkillName { get; private set; }
    public int Level { get; private set; }
    public int MpCost { get; private set; }
    public float Cooldown { get; private set; }
    public bool IsUnlocked { get; private set; }
    private DateTime lastUsedTime;

    public CharacterSkill(string skillName, int level, int mpCost, float cooldown, bool isUnlocked = false)
    {
        SkillName = skillName;
        Level = 1;
        MpCost = mpCost;
        Cooldown = cooldown;
        IsUnlocked = isUnlocked;
        lastUsedTime = DateTime.MinValue;
    }

    public bool CanUse()
    {
        if (!IsUnlocked)
            return false;

        double elapsedTime = (DateTime.UtcNow - lastUsedTime).TotalSeconds;

        return elapsedTime >= Cooldown;
    }

    public void Use()
    {
        if (!CanUse())
            return;

        lastUsedTime = DateTime.UtcNow;
    }

    public void Unlock()
    {
        IsUnlocked = true;
    }

    public void IncreaseLevel()
    {
        Level++;
    }

    public void SetMpCost(int mpCost)
    {
        if (mpCost < 0)
            return;

        MpCost = mpCost;
    }

    public void SetCooldown(float cooldown)
    {
        if (cooldown < 0f)
            return;

        Cooldown = cooldown;
    }
}
