using UnityEngine;
using System;
using System.Collections.Generic;


[DisallowMultipleComponent]
public class CharacterInventory : MonoBehaviour
{
    private CharacterEquipment equipment;
    internal void Inject(CharacterEquipment source)
    {
        if (source == null || source.gameObject != gameObject)
            throw new InvalidOperationException("같은 캐릭터의 CharacterEquipment 필요");

        equipment = source;
    }

    private readonly Dictionary<Guid, CharacterInventoryEquipment> equipmentItems = new Dictionary<Guid, CharacterInventoryEquipment>();

    public int EquipmentCount => equipmentItems.Count;
    public event Action<Guid> EquipmentChanged;
    public event Action ListChanged;

    public bool TryAddEquipment(ItemStatus source, out Guid instanceId)
    {
        instanceId = Guid.Empty;
        if (source == null || source.Type != ItemType.Equipment || source.BaseValue < 0)
            return false;

        Guid newId = Guid.NewGuid();
        while (equipmentItems.ContainsKey(newId))
            newId = Guid.NewGuid();

        CharacterInventoryEquipment entry = new CharacterInventoryEquipment(newId, source.Id, source.BaseValue);

        equipmentItems.Add(newId, entry);
        instanceId = newId;
        NotifyEquipmentChanged(newId);
        return true;
    }

    public bool TryGetEquipment(Guid instanceId, out CharacterInventoryEquipment equipment)
    {
        return equipmentItems.TryGetValue(instanceId, out equipment);
    }

    public int GetEquipmentCount(int itemId)
    {
        int count = 0;
        
        foreach (CharacterInventoryEquipment equipment in equipmentItems.Values)
        {
            if (equipment.ItemId == itemId)
                count++;
        }

        return count;
    }

    public bool TryRemoveEquipment(Guid instanceId)
    {
        if (equipment == null)
            return false;

        if (equipment != null && equipment.IsEquipped(instanceId))
            return false;

        if (!equipmentItems.Remove(instanceId))
            return false;

        NotifyEquipmentChanged(instanceId);

        return true;
    }

    public IReadOnlyList<CharacterInventoryEquipment> GetEquipmentSnapshot()
    {
        return new List<CharacterInventoryEquipment>(equipmentItems.Values).AsReadOnly();
    }

    private void NotifyListChanged()
    {
        Action handlers = ListChanged;

        if (handlers == null)
            return;

        foreach (Delegate handler in handlers.GetInvocationList())
        {
            try 
            {
                ((Action)handler)(); 
            }
            catch(Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }
    }

    private void NotifyEquipmentChanged(Guid instanceId)
    {
        NotifyListChanged();
        Action<Guid> handlers = EquipmentChanged;

        if (handlers == null)
            return;

        foreach (Delegate handler in handlers.GetInvocationList())
        {
            try 
            { 
                ((Action<Guid>) handler)(instanceId); 
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }
    }
}
