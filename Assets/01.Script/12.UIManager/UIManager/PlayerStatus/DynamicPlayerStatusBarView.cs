using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DynamicPlayerStatusBarView : MonoBehaviour
{
    
    [SerializeField] Image hpBar;
    [SerializeField] Image mpBar;
    
    [SerializeField] TextMeshProUGUI attackRating;
    [SerializeField] TextMeshProUGUI money;


    //fill º¯°æ.
    public void HpChanged(float nowHp,float maxHp)
    {
        hpBar.fillAmount = nowHp / maxHp;
    }
    public void MpChanged(float nowMp, float maxMp)
    {
        mpBar.fillAmount = nowMp / maxMp;
    }
    
    public void AttackRatingChanged(string attackRating)
    {
        this.attackRating.text = attackRating;
    }
    public void MoneyChanged(int money)
    {
        this.money.text = money.ToString();
    }

}
