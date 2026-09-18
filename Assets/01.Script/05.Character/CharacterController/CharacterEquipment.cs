using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterInventory))]
public class CharacterEquipment : MonoBehaviour
{
    [SerializeField] private List<CharacterArmorPartRule> armorPartRules = new List<CharacterArmorPartRule>();

    private CharacterInventory inventory;
    private readonly Dictionary<CharacterEquipmentSlot, Guid> slots = CreateSlots();

    public event Action<CharacterEquipmentSlot, Guid, Guid> SlotChanged;
    public event Action ListChanged;

    private CharacterControllers controller;
    private CharacterInventory Inventory => inventory;
    internal void Inject(CharacterControllers owner, CharacterInventory source)
    {
        if (owner == null || source == null || owner.gameObject != gameObject || source.gameObject != gameObject)
            throw new InvalidOperationException("장비와 인벤토리는 같은 캐릭터에 있어야 합니다.");

        controller = owner;
        inventory = source;
    }

    #region 장비 스탯 조회 및 합산하는 부분
    // 테스트용, 추후 아이템쪽이랑 연결될거
    [SerializeField] private List<EquipmentStatEntry> equipmentStatEntries = new List<EquipmentStatEntry>();
    private bool equipmentStatsDirty = true;
    private bool recalculatingStats;

    [Serializable] public sealed class EquipmentStatEntry
    {
        public int itemId;
        public EquipmentStatValues stats = new EquipmentStatValues();
    }

    // 장비쪽 스탯 구현 완료되면 이쪽 조회 부분 교체
    private bool TryGetEquipmentStats(CharacterInventoryEquipment item, CharacterEquipmentSlot slot, out EquipmentStatValues stats)
    {
        stats = null;

        if (item == null || equipmentStatEntries == null)
            return false;

        foreach (EquipmentStatEntry entry in equipmentStatEntries)
        {
            if (entry == null || entry.itemId != item.ItemId)
                continue;

            if (stats != null || entry.stats == null)
                return false;

            stats = entry.stats;
        }

        return stats != null;
    }

    public void RequestEquipmentStatsRefresh() // 여러 변경점 한프레임에 모아서 처리할 때 쓸거
    {
        equipmentStatsDirty = true;
    }

    public void RefreshEquipmentStatsIfNeeded() //컨트롤러가 UI변경 알림 직전에 호출하는거
    {
        if (equipmentStatsDirty)
            RefreshEquipmentStats();
    }

    public bool RefreshEquipmentStats()
    {
        if (recalculatingStats)
            return false;

        equipmentStatsDirty = false;
        recalculatingStats = true;

        try
        {
            if (controller == null || controller.Status == null || inventory == null)
                return StatsFailed("Controller의 참조 주입 및 Status 초기화가 필요합니다.");

            var total = new EquipmentStatValues();
            var seen = new HashSet<Guid>();

            foreach (var pair in slots)
            {
                if (pair.Value == Guid.Empty)
                    continue;

                if (!seen.Add(pair.Value))
                    return StatsFailed("장비 ID 중복 등록");

                if (!TryGetEquippedItem(pair.Key, out CharacterInventoryEquipment item))
                    return StatsFailed($"{pair.Key} 부위 장비 인벤에 없음");

                if (!TryGetEquipmentStats(item, pair.Key, out EquipmentStatValues values) || values == null || !values.IsFinite())
                    return StatsFailed($"{pair.Key} | {item.ItemId} 추가 스탯 확인");

                total.Add(values);
            }

            if (!total.IsFinite())
                return StatsFailed("합계 유효수치 범위 이탈");

            total.ApplyTo(controller);
            return true;
        }

        catch (Exception exception)
        {
            Debug.LogException(exception, this);
            return false;
        }
        finally 
        {
            recalculatingStats = false;
        }
    }

    private bool StatsFailed(string message)
    {
        Debug.LogWarning($"스탯 갱신 실패", this);
        return false;
    }

    [ContextMenu("Equipment/착용 장비 스탯 재계산")]
    private void RefreshStatsFromInspector()
    {
        if (Application.isPlaying)
            RefreshEquipmentStats();
    }
    #endregion

    private static Dictionary<CharacterEquipmentSlot, Guid> CreateSlots()
    {
        var result = new Dictionary<CharacterEquipmentSlot, Guid>();

        foreach (CharacterEquipmentSlot slot in Enum.GetValues(typeof(CharacterEquipmentSlot)))
            result.Add(slot, Guid.Empty);

        return result;
    }

    public bool TryEquipItem(Guid instanceId)
    {
        if (Inventory == null || !Inventory.TryGetEquipment(instanceId, out CharacterInventoryEquipment item) || !TryGetPart(item.ItemId, out CharacterArmorPart part))
            return false;

        CharacterEquipmentSlot slot;

        switch (part)
        {
            case CharacterArmorPart.Hat:
                slot = CharacterEquipmentSlot.Hat;
                break;
            case CharacterArmorPart.Top:
                slot = CharacterEquipmentSlot.Top;
                break;
            case CharacterArmorPart.Gloves:
                slot = CharacterEquipmentSlot.Gloves;
                break;
            case CharacterArmorPart.Shoes:
                slot = CharacterEquipmentSlot.Shoes;
                break;
            case CharacterArmorPart.Necklace:
                slot = CharacterEquipmentSlot.Necklace;
                break;
            case CharacterArmorPart.Ring:
                slot = CharacterEquipmentSlot.Ring1;
                break;
            default:
                return false;
        }

        return TryEquip(instanceId, slot);
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

        if (!RefreshEquipmentStats()) // 바뀐 착용상태 기준으로 스탯 합산
        {
            slots[slot] = previous; // 실패 시 원래장비로 복원
            return false;
        }

        NotifySlotChanged(slot, previous, instanceId);

        return true;
    }

    public bool TryUnequip(CharacterEquipmentSlot slot)
    {
        if (!slots.TryGetValue(slot, out Guid previous) || previous == Guid.Empty)
            return false;

        slots[slot] = Guid.Empty;

        if (!RefreshEquipmentStats())
        {
            slots[slot] = previous;
            return false;
        }

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

    public IReadOnlyList<CharacterInventoryEquipment> GetEquippedItemsSnapshot()
    {
        var result = new List<CharacterInventoryEquipment>();
        
        foreach (CharacterEquipmentSlot slot in Enum.GetValues(typeof(CharacterEquipmentSlot)))
        {
            if (TryGetEquippedItem(slot, out CharacterInventoryEquipment item) && item != null)
                result.Add(item);
        }

        return result.AsReadOnly();
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
            case CharacterArmorPart.Gloves:
                return slot == CharacterEquipmentSlot.Gloves;
            case CharacterArmorPart.Shoes:
                return slot == CharacterEquipmentSlot.Shoes;
            case CharacterArmorPart.Ring:
                return slot == CharacterEquipmentSlot.Ring1;
            case CharacterArmorPart.Necklace:
                return slot == CharacterEquipmentSlot.Necklace;
            default:
                return false;
        }
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
            catch (Exception exception) 
            { 
                Debug.LogException(exception, this); 
            }
        }
    }

    private void NotifySlotChanged(CharacterEquipmentSlot slot, Guid previous, Guid current)
    {
        NotifyListChanged();

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

// 별도 스크립트 파일 없이 이 컴포넌트 안에서 사용하는 추가 스탯 묶음.
[Serializable]
public sealed class EquipmentStatValues
{
    public long maxHp;
    public int maxMp;
    public int recoverMpPerSec;
    public float moveSpeed;
    public int strength;
    public int dexterity;
    public int intelligence;
    public int luck;
    public int attack;
    public float attackSpeedRate;
    public int hitRate;
    public float criticalRate;
    public float criticalDamage;
    public float damageByMainStat;
    public float damageOnBoss;
    public float damageOnNormal;
    public float armorPenetration;
    public float finalDamage;
    public long defence;
    public int dodgeRate;

    // 원본을 수정하지 않고 합산 대상에 더한다. 정수 범위 초과는 예외로 알린다.
    public void Add(EquipmentStatValues other)
    {
        if (other == null) throw new ArgumentNullException(nameof(other));
        checked
        {
            maxHp += other.maxHp;
            maxMp += other.maxMp;
            recoverMpPerSec += other.recoverMpPerSec;
            moveSpeed += other.moveSpeed;
            strength += other.strength;
            dexterity += other.dexterity;
            intelligence += other.intelligence;
            luck += other.luck;
            attack += other.attack;
            attackSpeedRate += other.attackSpeedRate;
            hitRate += other.hitRate;
            criticalRate += other.criticalRate;
            criticalDamage += other.criticalDamage;
            damageByMainStat += other.damageByMainStat;
            damageOnBoss += other.damageOnBoss;
            damageOnNormal += other.damageOnNormal;
            armorPenetration += other.armorPenetration;
            finalDamage += other.finalDamage;
            defence += other.defence;
            dodgeRate += other.dodgeRate;
        }
    }

    public bool IsFinite()
    {
        return !float.IsNaN(moveSpeed) && !float.IsInfinity(moveSpeed)
            && !float.IsNaN(attackSpeedRate) && !float.IsInfinity(attackSpeedRate)
            && !float.IsNaN(criticalRate) && !float.IsInfinity(criticalRate)
            && !float.IsNaN(criticalDamage) && !float.IsInfinity(criticalDamage)
            && !float.IsNaN(damageByMainStat) && !float.IsInfinity(damageByMainStat)
            && !float.IsNaN(damageOnBoss) && !float.IsInfinity(damageOnBoss)
            && !float.IsNaN(damageOnNormal) && !float.IsInfinity(damageOnNormal)
            && !float.IsNaN(armorPenetration) && !float.IsInfinity(armorPenetration)
            && !float.IsNaN(finalDamage) && !float.IsInfinity(finalDamage);
    }

    // 누적 증가가 아니라 전체 장비 합계를 교체한다.
    public void ApplyTo(CharacterControllers controller)
    {
        controller.SetEquipmentStats(
            maxHp: maxHp,
            maxMp: maxMp,
            recoverMpPerSec: recoverMpPerSec,
            moveSpeed: moveSpeed,
            strength: strength,
            dexterity: dexterity,
            intelligence: intelligence,
            luck: luck,
            attack: attack,
            attackSpeedRate: attackSpeedRate,
            hitRate: hitRate,
            criticalRate: criticalRate,
            criticalDamage: criticalDamage,
            damageByMainStat: damageByMainStat,
            damageOnBoss: damageOnBoss,
            damageOnNormal: damageOnNormal,
            armorPenetration: armorPenetration,
            finalDamage: finalDamage,
            defence: defence,
            dodgeRate: dodgeRate);
    }
}

[Serializable]
public sealed class CharacterArmorPartRule
{
    public int itemId;
    public CharacterArmorPart part;
}