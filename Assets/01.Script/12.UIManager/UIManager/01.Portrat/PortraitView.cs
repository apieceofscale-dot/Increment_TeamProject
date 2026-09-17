using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PortraitView : MonoBehaviour, IUIViewInitialize
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

    public void InitializeView()
    {
        //빈 게 맞음 통일성을 위해 넣어둠.
    }
 }
