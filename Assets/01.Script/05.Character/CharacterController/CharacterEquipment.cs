using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterInventory))]
public class CharacterEquipment : MonoBehaviour
{
    [SerializeField] private List<CharacterArmorPartRule> armorPartRules = new List<CharacterArmorPartRule>();

    private CharacterInventory inventory;
    private readonly Dictionary<CharacterEquipmentSlot, Guid> slots = CreateSlots();

    public event Action<CharacterEquipmentSlot, Guid, Guid> SlotChanged;

    private CharacterInventory Inventory
    {
        get
        {
            if (inventory == null) inventory = GetComponent<CharacterInventory>();
            return inventory;
        }
    }

    private static Dictionary<CharacterEquipmentSlot, Guid> CreateSlots()
    {
        var result = new Dictionary<CharacterEquipmentSlot, Guid>();
        foreach (CharacterEquipmentSlot slot in Enum.GetValues(typeof(CharacterEquipmentSlot)))
            result.Add(slot, Guid.Empty);
        return result;
    }

    public bool TryEquip(Guid instanceId, CharacterEquipmentSlot slot)
    {
        if (!slots.ContainsKey(slot) || instanceId == Guid.Empty || Inventory == null)
            return false;

        if (!Inventory.TryGetEquipment(instanceId, out CharacterInventoryEquipment item))
            return false;

        if (slots[slot] == instanceId)
            return true;

        if (IsEquipped(instanceId))
            return false;

        if (!TryGetPart(item.ItemId, out CharacterArmorPart part) || !Fits(part, slot))
            return false;

        Guid previous = slots[slot];
        slots[slot] = instanceId;
        NotifySlotChanged(slot, previous, instanceId);

        return true;
    }

    public bool TryUnequip(CharacterEquipmentSlot slot)
    {
        if (!slots.TryGetValue(slot, out Guid previous) || previous == Guid.Empty)
            return false;

        slots[slot] = Guid.Empty;
        NotifySlotChanged(slot, previous, Guid.Empty);

        return true;
    }

    public bool IsEquipped(Guid instanceId)
    {
        return instanceId != Guid.Empty && slots.ContainsValue(instanceId);
    }

    public bool TryGetEquippedItem(CharacterEquipmentSlot slot, out CharacterInventoryEquipment item)
    {
        item = null;
        return slots.TryGetValue(slot, out Guid id) && id != Guid.Empty && Inventory != null && Inventory.TryGetEquipment(id, out item);
    }

    public IReadOnlyDictionary<CharacterEquipmentSlot, Guid> GetSlotsSnapshot()
    {
        return new ReadOnlyDictionary<CharacterEquipmentSlot, Guid>(new Dictionary<CharacterEquipmentSlot, Guid>(slots));
    }

    private bool TryGetPart(int itemId, out CharacterArmorPart part)
    {
        part = default;
        bool found = false;

        if (armorPartRules == null)
            return false;

        foreach (CharacterArmorPartRule rule in armorPartRules)
        {
            if (rule == null || rule.itemId != itemId)
                continue;

            if (found || !Enum.IsDefined(typeof(CharacterArmorPart), rule.part))
                return false;

            part = rule.part;
            found = true;
        }

        return found;
    }

    private static bool Fits(CharacterArmorPart part, CharacterEquipmentSlot slot)
    {
        switch (part)
        {
            case CharacterArmorPart.Hat:
                return slot == CharacterEquipmentSlot.Hat;
            case CharacterArmorPart.Top:
                return slot == CharacterEquipmentSlot.Top;
            case CharacterArmorPart.Bottom:
                return slot == CharacterEquipmentSlot.Bottom;
            case CharacterArmorPart.Gloves:
                return slot == CharacterEquipmentSlot.Gloves;
            case CharacterArmorPart.Cape:
                return slot == CharacterEquipmentSlot.Cape;
            case CharacterArmorPart.Shoulder:
                return slot == CharacterEquipmentSlot.Shoulder;
            case CharacterArmorPart.Belt:
                return slot == CharacterEquipmentSlot.Belt;
            case CharacterArmorPart.Shoes:
                return slot == CharacterEquipmentSlot.Shoes;
            case CharacterArmorPart.Ring:
                return slot == CharacterEquipmentSlot.Ring1 || slot == CharacterEquipmentSlot.Ring2;
            case CharacterArmorPart.Necklace:
                return slot == CharacterEquipmentSlot.Necklace;
            default:
                return false;
        }
    }

    private void NotifySlotChanged(CharacterEquipmentSlot slot, Guid previous, Guid current)
    {
        var handlers = SlotChanged;

        if (handlers == null)
            return;

        foreach (Delegate handler in handlers.GetInvocationList())
        {
            try 
            { 
                ((Action<CharacterEquipmentSlot, Guid, Guid>)handler)(slot, previous, current); 
            }
            catch (Exception exception) 
            {
                Debug.LogException(exception, this);
            }
        }
    }
}

[Serializable]
public sealed class CharacterArmorPartRule
{
    public int itemId;
    public CharacterArmorPart part;
}