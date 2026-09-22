using UnityEngine;

public class CharacterPopupPresenter
{
    CharacterPopupView view;
    CharacterFacade model;

    //전투력 변화 이벤트
    //공격력 변화 이벤트
    //방어력 변화 이벤트
    //스탯 변화 이벤트.


    public CharacterPopupPresenter(CharacterPopupView view, CharacterFacade model)
    {
        this.view = view;
        this.model = model;

        view.OnAttackStatbuttonClicked += HandleAttackIncreasementBtn;
        view.OnDefenceButtonClicked += HandleDefenceStatIncreasementBtn;
        view.OnMaxHpButtonClicked += HandleMaxHpIncreasementBtn;

        //전투력 변화 이벤트 +=HandleCombatRating;
        //공격력 변화 이벤트 +=HandleAttack
        //방어력 변화 이벤트 +=HandleDefence   
        //최대체력 변화 이벤트 += HandleMaxHp


        Refresh();
    }

     


    public void HandleCombatRating(int num)
    {
        view.SetCombatRating(num);
    }
    public void HandleAttack(float num)
    {
        view.SetAttack(num);
    }
    public void HandleDefence(long num)
    { 
        view.SetDefence(num);
    }
    public void HandleMaxHp(long num)
    { 
        view.SetMaxHp(num);
    }
  

    public void HandleAttackIncreasement() 
    {
        //view.SetAttackIncreasement(model.공격력증가량);
    }
    public void HandleDefenceStatIncreasement()
    {
        //view.SetDefenceStatIncreasement(model.방어력증가량);
    }
    public void HandleMaxHpIncreasement()
    {
        //view.SetMaxHpIncreasement(model.체력증가량);
    }



    public void HandleAttackIncreasementBtn()
    {
       // model.Status.IncreaseAttack(model.공격력증가량);
    }

    public void HandleDefenceStatIncreasementBtn()
    {
       // model.Status.IncreaseDefence(model.방어증가량);
    }

    public void HandleMaxHpIncreasementBtn()
    {
       // model.Status.IncreaseMaxHp(model.체력증가량);
    }

    private void Refresh()
    {
       // HandleCombatRating(model.전투력)
        HandleAttack(model.Status.Attack);
        HandleDefence(model.Status.Defence);
        HandleMaxHp(model.Status.MaxHp);

        // view.SetCoombatRating(model.전투력);
        // view.SetAttackIncreasement(model.공격력증가량);
        // view.SetDefenceStatIncreasement(model.방어증가량);
        // view.SetMaxHpIncreasement(model.체력증가량);
    }

}
