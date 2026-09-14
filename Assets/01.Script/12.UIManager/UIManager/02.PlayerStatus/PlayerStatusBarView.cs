using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatusBarView : MonoBehaviour
{
    
    [SerializeField] Image hpBar;
    [SerializeField] Image mpBar;
    [SerializeField] TextMeshProUGUI level;
    [SerializeField] TextMeshProUGUI job;
    


  
    public void HpChanged(float nowHp,float maxHp)
    {
        hpBar.fillAmount = nowHp / maxHp;
    }
    public void MpChanged(float nowMp, float maxMp)
    {
        mpBar.fillAmount = nowMp / maxMp;
    }    
    public void LevelChagned(int level)
    {
        this.level.text = level.ToString();
    }
    public void JobChanged(string jobName)
    {
        this.job.text = jobName;
    }
}
