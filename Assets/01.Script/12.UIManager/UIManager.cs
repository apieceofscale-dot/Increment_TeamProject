using UnityEngine;

public class UIManager : MonoBehaviour, IBootStrapper
{
    


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
