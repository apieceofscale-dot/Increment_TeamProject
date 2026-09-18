using System.Collections.Generic;
using UnityEngine;

public class SkillPresenter
{
    CharacterFacade model;
    SkillView view;

    public SkillPresenter(SkillView view, CharacterFacade model)
    {
        this.model = model;
        this.view = view;

        view.OnSkillClicked += HandleSkillClicked;

        model.SkillSlotChanged += HandleSkillSlotChanged;

        IReadOnlyList<CharacterControllers.SkillCooldownChannel> cooldownChannels = model.SkillCooldownEvents;
        for (int i = 0; i < cooldownChannels.Count; i++)
            cooldownChannels[i].Changed += HandleSkillCooldown;

        SyncSkillSlots();
    }

    private void HandleSkillClicked(int slotIndex)
    {
        model.UseSkill(slotIndex);
    }

    private void HandleSkillSlotChanged(SkillSlotInfo data)
    {
        view.SetSkill(data.SlotIndex, data.Sprite, data.SkillName);
    }

    private void HandleSkillCooldown(SkillCooldownInfo data)
    {
        view.SetCooltime(data.SlotIndex, data.TimeLeft, data.TotalTime);
    }

    private void SyncSkillSlots()
    {
        for (int i = 0; i < CharacterControllers.SkillSlotCount; i++)
        {
            SkillSlotInfo slot = model.GetEquippedSkill(i);
            HandleSkillSlotChanged(slot);

            SkillCooldownInfo cooldown = model.GetSkillCooldown(i);
            if (cooldown.TimeLeft > 0f)
                HandleSkillCooldown(cooldown);
        }
    }
}
