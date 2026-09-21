using System;

public class MainPresenter
{
   
    MainView view;

    private readonly int[] playerIds = { 1000, 1001, 1002, 1003 };

    public event Action<int> OnCharacterSelected;


    public MainPresenter(MainView view)
    {
        this.view = view;        

        view.OnChoiceButtonClicked += HandleChoiceButtonClicked;

        Initilize();
    }

    private void Initilize()
    {
        for (int i = 0; i < playerIds.Length; i++)
        {
            int playerId = playerIds[i];

            if (!DataManager.instance.TryGetPlayerData(playerId, out PlayerData playerData))
            {
                continue;
            }

           view.SetButton(i, playerData.portrait, playerData.displayName);  
        }
    }

    public void HandleChoiceButtonClicked(int index)
    {
        if (index < 0 || index >= playerIds.Length) return ;

        int playerId = playerIds[index];

        OnCharacterSelected?.Invoke(playerId);
    }



}


