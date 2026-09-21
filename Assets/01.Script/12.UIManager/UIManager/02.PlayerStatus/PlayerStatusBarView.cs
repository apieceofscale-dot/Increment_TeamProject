using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatusBarView : MonoBehaviour, IUIViewInitialize
{
    
    [SerializeField] Image hpBar;
    [SerializeField] Image mpBar;
    [SerializeField] Image expBar;
    [SerializeField] TextMeshProUGUI level;
    [SerializeField] TextMeshProUGUI job;

    public void InitializeView()
    {
        //ºó°Ô ¸ÂÀ½.
    }
    public void SetHpBar(float nowHp, float maxHp)
    {
        hpBar.fillAmount = maxHp > 0 ? Mathf.Clamp01((float)((double)nowHp / maxHp)) : 0f;
    }
    public void SetMpBar(float nowMp, float maxMp)
    {
        mpBar.fillAmount = maxMp > 0 ? Mathf.Clamp01((float)nowMp / maxMp) : 0f;
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
