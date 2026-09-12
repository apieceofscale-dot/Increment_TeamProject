using System;
using UnityEngine;

public sealed class StageFacade : MonoBehaviour, IBootStrapper
{
    //세이브매니저만들어지면 완성되면 이 값을 false로 두고 SaveManager가 StartStage를 호출
    //[SerializeField] private bool autoStartOnBoot = true;

    private StageController stageController;
    private StageFactory stageFactory;
    private MonsterFacade monsterFacade;

    public int BootOrder => (int)BootLayer.StageManager;


    public void IBootStrapperInject(BootstrapContext context)
    {

    }

    public void IBootStrapperInitialize()
    {

    }


}
