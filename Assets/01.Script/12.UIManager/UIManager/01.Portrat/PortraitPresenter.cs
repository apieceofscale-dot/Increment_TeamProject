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
        model.AttackRatingChanged += HandleAttackRating;

        HandlePortrait(model.GetCharacterPortrait());
        HandleGoldChanged(model.Money);
        HandleAttackRating(model.AttackRating);

        model.RefreshUiEvents();
    }

    public void HandlePortrait(Sprite portrait)
    {
        view.SetPortrait(portrait);
    }

    public void HandleAttackRating(float attackRating)
    {
        view.SetAttackRating(attackRating);
    }

    public void HandleGoldChanged(long gold)
    {
        view.SetGold(gold);
    }
}
