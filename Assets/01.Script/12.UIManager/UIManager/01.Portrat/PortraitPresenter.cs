using UnityEngine;

public class PortraitPresenter : MonoBehaviour
{
    PortraitView view;
    CharacterFacade model;

    //참조 : 파사드의 골드 변경 이벤트
    //참조 : 전투력 변경 이벤트.

    public PortraitPresenter(PortraitView view, CharacterFacade model)
    {
        this.view = view;
        this.model = model;

        //HandlePortrait(model.portrait); 초기화.

        //골드 변경 이벤트 += HandleGoldChanged;
        //전투력 변경 이벤트 += HandleAttackRating;
    }


    //얘는 캐릭터 초상화라 이벤트 필요 없습니다.
    public void HandlePortrait(Sprite portrait)
    {
        view.SetPortrait(portrait);
    }


    //아래는 이벤트 필요합니다. 
    public void HandleAttackRating(float attackRating)
    {
        view.SetAttackRating(attackRating);
    } 

    public void HandleGoldChanged(int gold)
    {
        view.SetGold(gold);
    }




}

