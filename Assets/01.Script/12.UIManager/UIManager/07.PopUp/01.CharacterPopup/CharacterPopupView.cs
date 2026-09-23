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

    // [변경] 기존 expBar의 Inspector 참조를 유지하면서 실제 용도인 랭크 진행도로 이름을 바꾼다.
    [UnityEngine.Serialization.FormerlySerializedAs("expBar")]
    [SerializeField] Image rankProgressBar;
    // [변경] 기존 pointLeft의 Inspector 연결을 유지하고 요청한 필드명으로 변경한다.
    [UnityEngine.Serialization.FormerlySerializedAs("pointLeft")]
    [SerializeField] TextMeshProUGUI lefttext;


    public event Action OnAttackStatbuttonClicked;
    public event Action OnDefenceButtonClicked;
    public event Action OnMaxHpButtonClicked;

    // [연결] Presenter가 팝업 재표시와 파괴 시점을 받을 수 있게 한다.
    public event Action OnShown;
    public event Action OnDestroyed;
    private void OnEnable() => InitializePopup();

    // [연결] 팝업을 열 때마다 Presenter에 현재 값으로 표시를 초기화하도록 요청한다.
    // 버튼 이벤트를 등록하는 InitializeView와 구분하여 중복 등록을 방지한다.
    public void InitializePopup()
    {
        OnShown?.Invoke();
    }
    private void OnDestroy() => OnDestroyed?.Invoke();

    // [연결] 포인트/강화 한도에 따른 버튼 활성 여부를 적용한다.
    public void SetUpgradeAvailability(bool mainStat, bool defence, bool maxHp)
    {
        mainStatButton.interactable = mainStat;
        defenceButton.interactable = defence;
        maxHpButton.interactable = maxHp;
    }

    // [변경] 강화 1회마다 진행하며, 승급 시 시스템이 횟수를 0으로 초기화한다.
    public void SetRankProgress(int allocatedCount, int requiredCount)
    {
        if (rankProgressBar == null) return;
        rankProgressBar.type = Image.Type.Filled;
        rankProgressBar.fillMethod = Image.FillMethod.Horizontal;
        rankProgressBar.fillOrigin = (int)Image.OriginHorizontal.Left;
        rankProgressBar.fillAmount = requiredCount > 0
            ? Mathf.Clamp01((float)allocatedCount / requiredCount)
            : 0f;
    }

    private bool isInitialized;

    public void InitializeView()
    {
        if(isInitialized) return;

        isInitialized = true;
        mainStatButton.onClick.AddListener(HandleAttackStatButtonClicked);
        defenceButton.onClick.AddListener(HandleDefenceButtonClicked);
        maxHpButton.onClick.AddListener(HandleMaxHpButtonclikced);

    }

    // [연결] 레벨업으로 얻고 스탯 강화에 사용하는 남은 일반 포인트를 표시한다.
    public void SetRemainingStatPoints(int points)
    {
        if (lefttext != null)
            lefttext.text = points.ToString();
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
