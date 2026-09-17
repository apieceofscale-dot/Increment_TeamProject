using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnchatPopupView : MonoBehaviour, IUIViewInitialize
{
    [SerializeField] TextMeshProUGUI itemNameText;
    [SerializeField] TextMeshProUGUI currentLevelText;
    [SerializeField] TextMeshProUGUI nextLevelText;
    [SerializeField] TextMeshProUGUI currentValueText;
    [SerializeField] TextMeshProUGUI nextValueText;
    [SerializeField] TextMeshProUGUI costText;
    [SerializeField] Image itemIcon;

    public void InitializeView()
    {
    }

    public void SetPreview(ItemUpgradePreview preview, string displayName, Sprite icon)
    {
        if (itemNameText != null)
        {
            itemNameText.text = displayName ?? string.Empty;
        }

        if (currentLevelText != null)
        {
            currentLevelText.text = preview.CurrentUpgradeLevel.ToString();
        }

        if (nextLevelText != null)
        {
            nextLevelText.text = preview.NextUpgradeLevel.ToString();
        }

        if (currentValueText != null)
        {
            currentValueText.text = preview.CurrentEffectiveValue.ToString();
        }

        if (nextValueText != null)
        {
            nextValueText.text = preview.NextEffectiveValue.ToString();
        }

        if (costText != null)
        {
            costText.text = preview.UpgradeCost.ToString();
        }

        if (itemIcon != null)
        {
            itemIcon.sprite = icon;
            itemIcon.enabled = icon != null;
        }
    }
}
