
public class AutoFarmingPresenter
{
    private AutoFarmingView view;
    private CharacterFacade model;

    public AutoFarmingPresenter(AutoFarmingView view, CharacterFacade model)
    {
        this.view = view;
        this.model = model;

        view.OnAutoFarmingChanged += HandleAutoFarmingChanged;
    }

    private void HandleAutoFarmingChanged(bool isOn)
    {
        model.SetAutoFarming(isOn);
    }
}
