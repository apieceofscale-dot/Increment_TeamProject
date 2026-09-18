using UnityEngine;

/// <summary>
/// 아이템 강화 팝업. 수치는 ItemFacade, 인벤/재화는 CharacterFacade 연동 예정.
/// </summary>
public class EnchatPopupPresenter
{
    readonly EnchatPopupView view;
    readonly CharacterFacade characterFacade;
    readonly ItemFacade itemFacade;

    public EnchatPopupPresenter(EnchatPopupView view, CharacterFacade characterFacade, ItemFacade itemFacade)
    {
        this.view = view;
        this.characterFacade = characterFacade;
        this.itemFacade = itemFacade;
    }

    /// <summary>인벤에서 선택한 장비의 강화 미리보기.</summary>
    public void RefreshForItem(int itemId, int upgradeLevel, int starForce = 0)
    {
        if (view == null || itemFacade == null)
        {
            return;
        }

        if (!itemFacade.TryGetUpgradePreview(itemId, upgradeLevel, starForce, out ItemUpgradePreview preview))
        {
            return;
        }

        string displayName = itemId.ToString();
        if (itemFacade.TryGetItemData(itemId, out ItemData data))
        {
            displayName = data.displayName;
        }

        itemFacade.TryGetItemIcon(itemId, out Sprite icon);
        view.SetPreview(preview, displayName, icon);
    }
}
