using UnityEngine;

public class SkillPresenter
{
    CharacterFacade model;
    SkillView view;

    // 파사드의 스킬 슬롯에 장착이벤트 참조. so나.
    // 파사드의 쿨타임 관련 6개 짜리 이벤트 배열 참조. so나.

    public SkillPresenter(SkillView view, CharacterFacade model)
    {
        this.model = model;
        this.view = view;

        view.OnSkillClicked += HandleSkillClicked;

        //파사드의 슬롯 이벤트 += HandleSkillChanged;
        //파사드의 쿨타임 이벤트 배열 +=HandleCoolTime

        // SyncSkillSlots(); 이건 세이브를 만들었을 때 사용할 것. 없으면 영원히 주석처리.
    }


    private void HandleSkillClicked(int slotIndex)
    {
        //model.useSkill(slotIndex);
        // 슬롯 인덱스 만으로 스킬에 접근할 수 있어야 합니다.
    }

    //아래 두 핸들러의 인자는 제가 일단 완성하기 위해 쓴 것입니다.
    //제네릭으로 선언해서, Data구조를 만들어서 선언하시면 제가 알아서 바꾸면 되니 편한대로 하세요.
    //제네릭이 더 확장성 있는 방법입니다. 나중에 구조체에 멤버만 추가하고, 파사드에서 초기화 하면 끝이니까요.
    //특히 호출자가 수정본을 몰라도 됩니다.    
    /*
     ex)
      private void HandleSkillChanged(SkillData data)
      {
           view.SetSkill(data.slotIndex, data.sprite, data.skillName); 
      { 
    */
    private void HandleSkillChanged(int slotIndex, Sprite sprite, string skillName)
    {
        view.SetSkill(slotIndex, sprite, skillName);        
    }     
    private void HandleCoolTime(int slotIndex,float timeLeft, float totalTime)
    {
        view.SetCooltime(slotIndex, timeLeft, totalTime);
    }



    private void SyncSkillSlots()
    {
        for (int i = 0; i < 6; i++)
        {
            
            // SkillData data = model.GetEquippedSkill(i);
            // view.SetSkill(i, data.Sprite, data.SkillName);
        }
    }

}
