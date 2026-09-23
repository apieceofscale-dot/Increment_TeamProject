using System;
using UnityEngine;

public class CharacterPopupPresenter : IDisposable
{
    CharacterPopupView view;
    CharacterFacade model;
    private bool disposed;

    //전투력 변화 이벤트
    //공격력 변화 이벤트
    //방어력 변화 이벤트
    //스탯 변화 이벤트.


    public CharacterPopupPresenter(CharacterPopupView view, CharacterFacade model)
    {
        if (view == null) throw new ArgumentNullException(nameof(view));
        if (model == null) throw new ArgumentNullException(nameof(model));
        this.view = view;
        this.model = model;

        view.OnAttackStatbuttonClicked += HandleAttackIncreasementBtn;
        view.OnDefenceButtonClicked += HandleDefenceStatIncreasementBtn;
        view.OnMaxHpButtonClicked += HandleMaxHpIncreasementBtn;

        // [연결] 강화·장비·레벨·직업 변경 시 현재 수치와 강화 가능 여부를 다시 조회한다.
        model.StatUpgradeChanged += Refresh;
        model.EquipmentChanged += Refresh;
        model.LevelChanged += HandleLevelChanged;
        model.JobNameChanged += HandleJobChanged;
        model.CombatPowerChanged += HandleCombatPowerChanged;
        model.HpChanged += HandleHpChanged;
        // [변경] 이 팝업의 바는 랭크 진행도이므로 경험치 이벤트는 연결하지 않는다.

        // [연결] 팝업 재표시 시 갱신하고, 파괴 시 모델 이벤트 구독을 해제한다.
        // [변경] 실제 팝업 열기 요청은 전용 초기화 함수로 받는다.
        view.OnShown += InitializePopup;
        view.OnDestroyed += Dispose;

        //전투력 변화 이벤트 +=HandleCombatRating;
        //공격력 변화 이벤트 +=HandleAttack
        //방어력 변화 이벤트 +=HandleDefence
        //최대체력 변화 이벤트 += HandleMaxHp


        Refresh();
    }




    public void HandleCombatRating(int num)
    {
        view.SetCombatRating(num);
    }
    public void HandleAttack(float num)
    {
        view.SetAttack(num);
    }
    public void HandleDefence(long num)
    {
        view.SetDefence(num);
    }
    public void HandleMaxHp(long num)
    {
        view.SetMaxHp(num);
    }


    public void HandleAttackIncreasement()
    {
        // [연결] 공격력 직접 증가가 아니라 직업별 주스탯 강화 증가량이다.
        view.SetAttackIncreasement(model.GetStatUpgradeOption(CharacterStatUpgradeType.MainStat).IncreaseAmount);
        //view.SetAttackIncreasement(model.공격력증가량);
    }
    public void HandleDefenceStatIncreasement()
    {
        // [연결] 강화 API가 제공하는 방어력 증가량을 표시한다.
        view.SetDefenceStatIncreasement(model.GetStatUpgradeOption(CharacterStatUpgradeType.Defence).IncreaseAmount);
        //view.SetDefenceStatIncreasement(model.방어력증가량);
    }
    public void HandleMaxHpIncreasement()
    {
        // [연결] 강화 API가 제공하는 최대 체력 증가량을 표시한다.
        view.SetMaxHpIncreasement(model.GetStatUpgradeOption(CharacterStatUpgradeType.MaxHp).IncreaseAmount);
        //view.SetMaxHpIncreasement(model.체력증가량);
    }



    public void HandleAttackIncreasementBtn()
    {
        // [연결] 포인트 소모와 한도 검사는 Facade의 강화 API에 맡긴다.
        Upgrade(CharacterStatUpgradeType.MainStat);
       // model.Status.IncreaseAttack(model.공격력증가량);
    }

    public void HandleDefenceStatIncreasementBtn()
    {
        // [연결] 방어력 강화 버튼.
        Upgrade(CharacterStatUpgradeType.Defence);
       // model.Status.IncreaseDefence(model.방어증가량);
    }

    public void HandleMaxHpIncreasementBtn()
    {
        // [연결] 최대 체력 강화 버튼.
        Upgrade(CharacterStatUpgradeType.MaxHp);
       // model.Status.IncreaseMaxHp(model.체력증가량);
    }

    // [연결] 팝업을 열 때 1회 실행한다. 캐릭터 스탯 자체를 리셋하지 않고 표시만 갱신한다.
    public void InitializePopup()
    {
        Refresh();
    }

    private void Refresh()
    {
        if (disposed || view == null || model == null || !model.IsInitialized)
            return;

        // [연결] 초기 표시와 이벤트 갱신에 같은 API를 사용한다.
        // [변경] 왼쪽 스탯창을 먼저 채운 뒤 강화 옵션과 경험치 UI를 갱신한다.
        HandleCombatRating(model.CombatPower);
        // [설명] 주스탯 강화는 STR/DEX/INT/LUK를 올린다. 이 항목은 별도 기본 공격력이다.
        HandleAttack(model.Status.Attack);
        HandleDefence(model.Status.Defence);
        HandleMaxHp(model.Status.MaxHp);
        HandleAttackIncreasement();
        HandleDefenceStatIncreasement();
        HandleMaxHpIncreasement();
        view.SetUpgradeAvailability(
            model.GetStatUpgradeOption(CharacterStatUpgradeType.MainStat).CanUpgrade,
            model.GetStatUpgradeOption(CharacterStatUpgradeType.Defence).CanUpgrade,
            model.GetStatUpgradeOption(CharacterStatUpgradeType.MaxHp).CanUpgrade);
        // [변경] 일반 레벨 경험치 대신 현재 랭크에서 소비한 강화 횟수를 표시한다.
        // HandleExpChanged(model.CurrentExp, model.RequiredExp);
        CharacterStatUpgradeInfo rankInfo = model.GetStatUpgradeInfo();
        // [연결] 초기 표시/팝업 열기와 StatUpgradeChanged(레벨업·강화) 시 즉시 갱신한다.
        view.SetRemainingStatPoints(rankInfo.StatPoints);
        int allocatedCount = rankInfo.MainStatCount + rankInfo.DefenceCount + rankInfo.MaxHpCount;
        int requiredCount = model.GetStatUpgradeOption(CharacterStatUpgradeType.MainStat).MaxCount
            + model.GetStatUpgradeOption(CharacterStatUpgradeType.Defence).MaxCount
            + model.GetStatUpgradeOption(CharacterStatUpgradeType.MaxHp).MaxCount;
        view.SetRankProgress(allocatedCount, requiredCount);
       // HandleCombatRating(model.전투력)
        // [변경] 기존 왼쪽 스탯 표시 호출은 위로 이동하여 먼저 초기화한다.
        // HandleAttack(model.Status.Attack);
        // HandleDefence(model.Status.Defence);
        // HandleMaxHp(model.Status.MaxHp);

        // view.SetCoombatRating(model.전투력);
        // view.SetAttackIncreasement(model.공격력증가량);
        // view.SetDefenceStatIncreasement(model.방어증가량);
        // view.SetMaxHpIncreasement(model.체력증가량);
    }

    // [연결] 실패한 클릭도 최신 포인트/한도로 버튼 상태를 다시 맞춘다.
    private void Upgrade(CharacterStatUpgradeType type)
    {
        if (disposed || model == null || !model.IsInitialized) return;
        model.TryUpgradeStat(type);
        Refresh();
    }

    private void HandleLevelChanged(int level) => Refresh();
    private void HandleJobChanged(string jobName) => Refresh();
    private void HandleCombatPowerChanged(int combatPower) => Refresh();
    private void HandleHpChanged(long currentHp, long maxHp) => Refresh();

    // [변경] 기존 경험치 바 핸들러는 랭크 진행도와 무관하므로 사용하지 않는다.
    // private void HandleExpChanged(long currentExp, long requiredExp)
    // {
    //     if (!disposed && view != null)
    //         view.SetExpProgress(currentExp, requiredExp);
    // }

    // [연결] 재바인딩/파괴 후 중복 강화와 파괴된 UI 접근을 막는다.
    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        if (view != null)
        {
            view.OnAttackStatbuttonClicked -= HandleAttackIncreasementBtn;
            view.OnDefenceButtonClicked -= HandleDefenceStatIncreasementBtn;
            view.OnMaxHpButtonClicked -= HandleMaxHpIncreasementBtn;
            view.OnShown -= InitializePopup;
            view.OnDestroyed -= Dispose;
        }
        if (model != null)
        {
            model.StatUpgradeChanged -= Refresh;
            model.EquipmentChanged -= Refresh;
            model.LevelChanged -= HandleLevelChanged;
            model.JobNameChanged -= HandleJobChanged;
            model.CombatPowerChanged -= HandleCombatPowerChanged;
            model.HpChanged -= HandleHpChanged;
            // [변경] 경험치 이벤트는 구독하지 않으므로 해제도 필요하지 않다.
        }
    }
}
