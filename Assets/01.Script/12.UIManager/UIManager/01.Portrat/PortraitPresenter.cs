using UnityEngine;

public class PortraitPresenter
{
    PortraitView view;
    CharacterFacade model;

    public PortraitPresenter(PortraitView view, CharacterFacade model)
    {
        this.view = view;
        this.model = model;

        model.MoneyChanged += HandleGoldChanged;
        model.CombatPowerChanged += HandleCombatPower;

        HandlePortrait(model.GetCharacterPortrait());
        HandleGoldChanged(model.Money);
        HandleCombatPower(model.CombatPower);
    }

    public void HandlePortrait(Sprite portrait)
    {
        view.SetPortrait(portrait);
    }

    public void HandleCombatPower(int combatPower)
    {
        view.SetAttackRating(combatPower);
    }

    public void HandleGoldChanged(long gold)
    {
        view.SetGold(gold);
    }
}
