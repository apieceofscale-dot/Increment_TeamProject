using UnityEngine;

public class PlayerStatusBarPresenter
{
    private PlayerStatusBarView view;
    private readonly CharacterFacade model;

    public PlayerStatusBarPresenter(PlayerStatusBarView view, CharacterFacade model)
    {
        this.view = view;
        this.model = model;

        model.HpChanged += HandleHpChanged;
        model.MpChanged += HandleMpChanged;
        model.ExpChanged += HandleExpChanged;
        model.JobNameChanged += HandleJobChanged;
        model.LevelChanged += HandleLevelChanged;

        model.RefreshUiEvents();
    }

    private void HandleHpChanged(long nowHp, long totalHp)
    {
        view.SetHpBar(nowHp, totalHp);
    }

    private void HandleMpChanged(int nowMp, int totalMp)
    {
        view.SetMpBar(nowMp, totalMp);
    }

    private void HandleExpChanged(long nowExp, long totalExp)
    {
        float required = totalExp > 0 ? totalExp : 1f;
        view.SetExpBar(nowExp, required);
    }

    private void HandleLevelChanged(int level)
    {
        view.SetLevel(level);
    }

    private void HandleJobChanged(string job)
    {
        view.SetJobName(job);
    }
}
