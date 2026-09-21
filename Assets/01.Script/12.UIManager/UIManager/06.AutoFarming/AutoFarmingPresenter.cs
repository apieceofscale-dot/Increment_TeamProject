
public class AutoFarmingPresenter
{
    private AutoFarmingView view;
    private CharacterFacade model;

    public AutoFarmingPresenter(AutoFarmingView view, CharacterFacade model)
    {
        this.view = view;
        this.model = model;

        view.OnAutoFarmingChanged += HandleAutoFarmingChanged;
        view.SetAutoFarming(model.IsAutoFarming);
    }

    private void HandleAutoFarmingChanged(bool isOn)
    {
        model.SetAutoFarming(isOn);
        view.SetAutoFarming(model.IsAutoFarming);
    }
}
