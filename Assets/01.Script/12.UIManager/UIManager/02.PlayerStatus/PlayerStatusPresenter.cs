using UnityEngine;

public class PlayerStatusPresenter
{
    private PlayerStatusBarView view;

    private readonly CharacterFacade model;

    private PlayerStatusPresenter(PlayerStatusBarView view, CharacterFacade model)//ui매니저에서 호출.
    {
        this.view = view;
        this.model = model;
    }

}
    

