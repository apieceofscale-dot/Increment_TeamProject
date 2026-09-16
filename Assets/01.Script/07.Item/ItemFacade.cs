using System;
using UnityEngine;

/// <summary>
/// 아이템 시스템 외부 API.
/// Factory로 생성하고, Character 쪽에 장착 스탯 정보를 제공한다.
/// </summary>
public class ItemFacade : MonoBehaviour
{
    public static event Action<ItemPickedUpInfo> ItemPickedUp;

    void OnEnable()
    {
        ItemPickedUp -= HandlePickedUp;
        ItemPickedUp += HandlePickedUp;
    }

    void OnDisable()
    {
        ItemPickedUp -= HandlePickedUp;
    }

    // -------------------------------------------------------------------------
    // UI / Character 연동 API
    // 팀에서 요청하는 메서드는 이 region 아래에 추가한다.
    // -------------------------------------------------------------------------
    #region UI · Character 연동

    /// <summary>장착 시 캐릭터에 더할 스탯·부위 조회. CharacterEquipment에서 호출.</summary>
    public bool TryGetEquipStat(int itemId, out ItemEquipStat stat)
    {
        if (DataManager.instance != null
            && DataManager.instance.TryGetItemData(itemId, out ItemData data))
        {
            stat = ItemEquipStat.FromData(data);
            return stat.Type == ItemType.Equipment && ItemArmorPartTable.IsEquipment(itemId);
        }

        stat = default;
        return false;
    }

    /// <summary>장비 id가 어느 슬롯 부위인지 조회.</summary>
    public bool TryGetArmorPart(int itemId, out CharacterArmorPart part)
    {
        return ItemArmorPartTable.TryGetPart(itemId, out part);
    }

    /// <summary>강화/스타포스 옵션 포함 스폰.</summary>
    public ItemController Spawn(int itemId, Vector3 position, Quaternion rotation, int upgradeLevel = 0, int starForce = 0)
    {
        if (ItemFactory.Instance == null)
        {
            Debug.LogWarning("[ItemFacade] ItemFactory is missing.");
            return null;
        }

        return ItemFactory.Instance.Create(itemId, position, rotation, 1, upgradeLevel, starForce);
    }

    /// <summary>스택 수량 포함 스폰. ItemDropManager에서 호출.</summary>
    public ItemController Spawn(int itemId, int amount, Vector3 position)
    {
        if (ItemFactory.Instance == null)
        {
            Debug.LogWarning("[ItemFacade] ItemFactory is missing.");
            return null;
        }

        return ItemFactory.Instance.Create(itemId, position, Quaternion.identity, amount);
    }

    /// <summary>ItemData(SO)로 스폰.</summary>
    public ItemController Spawn(ItemData data, Vector3 position, int stackAmount = 1)
    {
        if (ItemFactory.Instance == null)
        {
            Debug.LogWarning("[ItemFacade] ItemFactory is missing.");
            return null;
        }

        if (data == null)
        {
            Debug.LogWarning("[ItemFacade] item data is missing.");
            return null;
        }

        return ItemFactory.Instance.Create(data.id, position, Quaternion.identity, stackAmount);
    }

    /// <summary>아이템을 풀에 반환.</summary>
    public void Despawn(ItemController item)
    {
        if (item == null)
        {
            return;
        }

        item.ReturnToPool();
    }

    // TODO(UI): 장착 버튼 → CharacterFacade + TryGetEquipStat 연결
    // TODO(UI): 인벤토리 슬롯 클릭 → Spawn / Despawn 연결

    #endregion

    // -------------------------------------------------------------------------
    // 내부 이벤트
    // -------------------------------------------------------------------------

    public static void NotifyPickedUp(in ItemPickedUpInfo info)
    {
        ItemPickedUp?.Invoke(info);
    }

    void HandlePickedUp(ItemPickedUpInfo info)
    {
        if (info.Source != null)
        {
            Despawn(info.Source);
        }
    }
}
