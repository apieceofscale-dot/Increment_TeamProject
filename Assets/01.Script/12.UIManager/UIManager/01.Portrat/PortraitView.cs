using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PortraitView : MonoBehaviour
{
    [SerializeField] Image portrait;
    [SerializeField] TextMeshProUGUI attackRating;
    [SerializeField] TextMeshProUGUI gold;
    
    public void SetPortrait(Sprite portrait)
    {
        this.portrait.sprite = portrait;
    }
    public void SetAttackRating(float attackRating)
    {
        this.attackRating.text = attackRating.ToString("0");
    }
    public void SetGold(int gold)
    {
        this.gold.text = gold.ToString();  
    }
 }
