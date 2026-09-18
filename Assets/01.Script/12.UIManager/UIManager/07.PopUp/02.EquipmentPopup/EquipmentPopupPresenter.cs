using System;
using System.Collections.Generic;
using UnityEngine;


public class EquipmentPopupPresenter
{
    EquipmentPopupView view;
    CharacterFacade model;

    private IReadOnlyList<CharacterInventoryEquipment> currentItems;

    public readonly CharacterEquipmentSlot[] displayedEquipmentSlots =
    {
        CharacterEquipmentSlot.Hat,
        CharacterEquipmentSlot.Top,
        CharacterEquipmentSlot.Shoes,
        CharacterEquipmentSlot.Necklace,
        CharacterEquipmentSlot.Gloves,
        CharacterEquipmentSlot.Ring1
    };

    public EquipmentPopupPresenter(EquipmentPopupView view, CharacterFacade model)
    {
        this.view = view;
        this.model = model;
        view.OnItemClicked += HandleItemClicked;

        model.InventoryChanged += HandleInventoryChanged;
        model.EquipmentChanged += HandleEquipmentChanged;

        Refresh();
    }

    public void Refresh()
    {
        RefreshInventory();
        RefreshEquipment();
    }

    public void RefreshInventory()
    {
        currentItems = model.GetEquipmentInventory();

        List<Sprite> icons = new List<Sprite>(currentItems.Count);

        for (int i = 0; i < currentItems.Count; i++)
        {
            CharacterInventoryEquipment item = currentItems[i];
            if (model.TryGetItemData(item.ItemId, out ItemData itemData))
                icons.Add(itemData.icon);
            else
                icons.Add(null);
        }

        view.SetInventory(icons);
    }

    private void HandleItemClicked(int itemIndex)
    {
        if (currentItems == null) return;

        if (itemIndex < 0 || itemIndex >= currentItems.Count) return;

        CharacterInventoryEquipment selectedItem = currentItems[itemIndex];

        model.EquipItem(selectedItem.InstanceId);

        RefreshEquipment();
    }

    private void RefreshEquipment()
    {
        for (int i = 0; i < displayedEquipmentSlots.Length; i++)
        {
            CharacterEquipmentSlot slot = displayedEquipmentSlots[i];
            RefreshEquipmentSlot(slot);
        }
    }

    private void RefreshEquipmentSlot(CharacterEquipmentSlot slot)
    {
        if (!model.TryGetEquippedItem(slot, out CharacterInventoryEquipment equipment))
        {
            view.ClearEquippedItem(slot);
            return;
        }

        if (!model.TryGetItemData(equipment.ItemId, out ItemData itemData))
        {
            view.ClearEquippedItem(slot);
            return;
        }

        view.SetEquippedItem(slot, itemData.icon);
    }

    private void HandleEquipmentChanged()
    {
        RefreshEquipment();
    }

    private void HandleInventoryChanged()
    {
        RefreshInventory();
    }
}
