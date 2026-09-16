using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentPopupView : MonoBehaviour, IUIViewInitialize
{

    private const int MaxSlots = 24;

    public event Action<int> OnItemClicked;

    [SerializeField] private GameObject[] slots;   
    private Button[] buttons;    

    [SerializeField] private Button leftBtn;
    [SerializeField] private Button rightBtn;

    [SerializeField] private Image hatImage;
    [SerializeField] private Image armorImage;
    [SerializeField] private Image shoesImage;
    [SerializeField] private Image necklaceImage;
    [SerializeField] private Image glovesImage;
    [SerializeField] private Image ringImage;

    private IReadOnlyList<Sprite> itemIcons;


    private int currentPage = 0;

    private bool isInitialized;
    public void InitializeView()
    {
        if (isInitialized) return;

        buttons = new Button[slots.Length];

        for (int i = 0; i < slots.Length; i++)
        {
            int index = i;

            buttons[i] = slots[i].GetComponentInChildren<Button>(true);
            buttons[i].onClick.AddListener(() => HandleItemButtonClicked(index));
        }

        leftBtn.onClick.AddListener(HanldeLeftBtn);
        rightBtn.onClick.AddListener(HanldeRightBtn);

        isInitialized = true;
    }

    private void HandleItemButtonClicked(int slotIndex)
    {
        int itemIndex = currentPage * MaxSlots + slotIndex;

        if (itemIcons == null) return;

        if (itemIndex >= itemIcons.Count) return;

        OnItemClicked?.Invoke(itemIndex);
    }

    public void SetInventory(IReadOnlyList<Sprite> icons)
    {
        itemIcons = icons;

        // 데이터가 바뀌어서 현재 페이지가 사라졌을 수도 있으므로 보정
        int maxPage = GetMaxPage();

        if (currentPage > maxPage)
        {
            currentPage = maxPage;
        }

        RefreshPage();
    }
    private void RefreshPage()
    {
        int startIndex = currentPage * MaxSlots;

        for (int slotIndex = 0; slotIndex < MaxSlots; slotIndex++)
        {
            int itemIndex = startIndex + slotIndex;

            if (itemIcons != null && itemIndex < itemIcons.Count)
            {
                buttons[slotIndex].image.sprite = itemIcons[itemIndex];
                buttons[slotIndex].gameObject.SetActive(true);
            }
            else
            {
                // 슬롯 테두리는 부모에 있으니 버튼만 비워도 됨
                buttons[slotIndex].image.sprite = null;
                buttons[slotIndex].gameObject.SetActive(false);
            }
        }
    }

    public void HanldeRightBtn()
    {
        if (currentPage <= 0) return;
        currentPage--;
        RefreshPage();
    }
    public void HanldeLeftBtn()
    {
        if (currentPage >= GetMaxPage()) return;

        currentPage++;
        RefreshPage();
    }
    private int GetMaxPage()
    {
        if (itemIcons == null || itemIcons.Count == 0)
            return 0;

        return (itemIcons.Count - 1) / MaxSlots;
    }

    //여기부터 장비 장착.

    public void SetEquippedItem(CharacterEquipmentSlot slot, Sprite sprite)
    {
        Image target = GetEquipmentImage(slot);

        if (target == null)
            return;

        target.sprite = sprite;
        target.gameObject.SetActive(true);
    }


    public void ClearEquippedItem(CharacterEquipmentSlot slot)
    {
        Image target = GetEquipmentImage(slot);

        if (target == null)
            return;

        target.sprite = null;
        target.gameObject.SetActive(false);
    }


    private Image GetEquipmentImage(CharacterEquipmentSlot slot)
    {
        switch (slot)
        {
            case CharacterEquipmentSlot.Hat:
                return hatImage;
            
            case CharacterEquipmentSlot.Top:
                return armorImage;

            case CharacterEquipmentSlot.Shoes:
                return shoesImage;

            case CharacterEquipmentSlot.Necklace:
                return necklaceImage;

            case CharacterEquipmentSlot.Gloves:
                return glovesImage;

            case CharacterEquipmentSlot.Ring1:
                return ringImage;

            default:
                return null;
        }
    }

}
