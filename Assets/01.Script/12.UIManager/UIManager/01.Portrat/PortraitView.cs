using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PortraitView : MonoBehaviour
{

    [SerializeField] Image portrait;
    [SerializeField] TextMeshProUGUI attackRating;
    [SerializeField] TextMeshProUGUI gold;
    
    public void PortraitChanged(Image portrait)
    {
        this.portrait = portrait;
    }
    public void AttackRatingChanged(int attackRating)
    {
        this.attackRating.text = attackRating.ToString();
    }
    public void GoldChanged(int gold)
    {
        this.gold.text = gold.ToString();  
    }
 
}
