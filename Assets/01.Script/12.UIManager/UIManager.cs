using UnityEngine;

public class UIManager : MonoBehaviour, IBootStrapper
{

    
    //각 presenter를 생성자로 초기화.
    //그리드 제거 
    //123번 interative제거
    public int BootOrder => (int)BootLayer.UIManager;

    // Update is called once per frame
    void Update()
    {
        
    }

  
    
    public void IBootStrapperInject(BootstrapContext context)
    {

    }
    public void IBootStrapperInitialize()
    {

    }
}
