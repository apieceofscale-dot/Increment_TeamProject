using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterPopupView : MonoBehaviour, IUIViewInitialize
{
    [SerializeField] TextMeshProUGUI combatRating;
    [SerializeField] TextMeshProUGUI attack;
    [SerializeField] TextMeshProUGUI defence;
    [SerializeField] TextMeshProUGUI maxHp;    

    [SerializeField] TextMeshProUGUI mainStatIncreasement;
    [SerializeField] TextMeshProUGUI defenceIncreasement;
    [SerializeField] TextMeshProUGUI maxHpIncreasemetent;

    [SerializeField] Button mainStatButton;
    [SerializeField] Button defenceButton;
    [SerializeField] Button maxHpButton;

    public event Action OnAttackStatbuttonClicked;
    public event Action OnDefenceButtonClicked;
    public event Action OnMaxHpButtonClicked;

    private bool isInitialized;

    public void InitializeView()
    {
        if(isInitialized) return;

        isInitialized = true;
        mainStatButton.onClick.AddListener(HandleAttackStatButtonClicked);
        defenceButton.onClick.AddListener(HandleDefenceButtonClicked);
        maxHpButton.onClick.AddListener(HandleMaxHpButtonclikced);

    }

    public void SetCombatRating(int num) { combatRating.text = num.ToString(); }
    public void SetAttack(float num) { attack.text = num.ToString(); }
    public void SetDefence(long num) { defence.text = num.ToString(); }
    public void SetMaxHp(long num) { maxHp.text = num.ToString(); }
   

    public void SetAttackIncreasement(int num) { mainStatIncreasement.text = num.ToString(); }
    public void SetDefenceStatIncreasement(int num) { defenceIncreasement.text = num.ToString(); }
    public void SetMaxHpIncreasement(int num) { maxHpIncreasemetent.text = num.ToString(); }


    public void HandleAttackStatButtonClicked()
    {
        OnAttackStatbuttonClicked?.Invoke();
    }

    public void HandleDefenceButtonClicked()
    {
        OnDefenceButtonClicked?.Invoke();
    }

    public void HandleMaxHpButtonclikced()
    {
        OnMaxHpButtonClicked?.Invoke(); 
    }

}
