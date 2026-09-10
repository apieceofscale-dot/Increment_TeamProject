using System;
using System.Diagnostics;

public class CharacterSkill
{
    public string SkillName { get; private set; }
    public int Level { get; private set; }
    public int MpCost { get; private set; }
    public float Cooldown { get; private set; }
    public bool IsUnlocked { get; private set; }

    private readonly Func<float> clock;
    private float readyTime = float.NegativeInfinity;
    public float RemainingCooldown => Math.Max(0f, readyTime - clock());

    public CharacterSkill(string skillName, int level, int mpCost, float cooldown, bool isUnlocked = false, Func<float> clock = null)
    {
        SkillName = skillName;
        Level = Math.Max(1, level);
        MpCost = Math.Max(0, mpCost);
        Cooldown = Math.Max(0f, cooldown);
        IsUnlocked = isUnlocked;

        if (clock != null)
            this.clock = clock;
        else
        {
            Stopwatch timer = Stopwatch.StartNew();
            this.clock = () => (float)timer.Elapsed.TotalSeconds;
        }
    }

    public bool CanUse() => IsUnlocked && clock() >= readyTime;

    public bool TryUse(float effectiveCooldown)
    {
        if (!CanUse())
            return false;

        readyTime = clock() + Math.Max(0.01f, effectiveCooldown);
        return true;
    }

    public void Use()
    {
        TryUse(Cooldown);
    }
    public void Unlock()
    {
        IsUnlocked = true;
    }
    public void IncreaseLevel()
    {
        Level++;
    }
    public void SetMpCost(int value) 
    {
        if (value >= 0)
            MpCost = value; 
    }
    public void SetCooldown(float value) 
    { 
        if (value >= 0f)
            Cooldown = value;
    }
}
