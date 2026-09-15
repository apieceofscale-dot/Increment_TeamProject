using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatusBarView : MonoBehaviour
{
    
    [SerializeField] Image hpBar;
    [SerializeField] Image mpBar;
    [SerializeField] Image expBar;
    [SerializeField] TextMeshProUGUI level;
    [SerializeField] TextMeshProUGUI job;
  
    public void SetHpBar(float nowHp,float maxHp)
    {
        hpBar.fillAmount = nowHp / maxHp;
    }
    public void SetMpBar(float nowMp, float maxMp)
    {
        mpBar.fillAmount = nowMp / maxMp;
    }    
    public void SetExpBar(float nowExp, float maxExp)
    {
        expBar.fillAmount = nowExp / maxExp;
    }
    public void SetLevel(int level)
    {
        this.level.text = level.ToString();
    }
    public void SetJobName(string jobName)
    {
        this.job.text = jobName;
    }
}
