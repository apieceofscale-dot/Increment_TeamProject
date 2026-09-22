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
        model.CombatPowerChanged += HandleAttackRating;


        HandlePortrait(model.GetCharacterPortrait());
        HandleGoldChanged(model.Money);
        HandleAttackRating(model.CombatPower);

    }


    //얘는 캐릭터 초상화라 이벤트 필요 없습니다.
    public void HandlePortrait(Sprite portrait)
    {
        view.SetPortrait(portrait);
    }


    //아래는 이벤트 필요합니다. 
    public void HandleAttackRating(int attackRating)
    {
        view.SetAttackRating(attackRating);
    } 

    public void HandleGoldChanged(long gold)
    {
        view.SetGold(gold);
    }




}

