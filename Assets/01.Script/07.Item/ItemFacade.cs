using System;
using UnityEngine;

/// <summary>
/// 아이템 시스템 외부 API.
/// Factory로 생성하고, Character 쪽에 장착 스탯 정보를 제공한다.
/// </summary>
/// <remarks>
/// [팀 전달 — 2025-09-16, 신현수 부재 시 참고]
/// ■ Facade/API: Spawn·Despawn·ItemPickedUp·TryGetEquipStat·TryGetArmorPart 껍데기 구현됨.
/// ■ 동작함: ItemFactory+풀링, Trigger 픽업→ItemPickedUp→Despawn (Player 태그·Trigger 콜라이더 필요).
/// ■ 미동작·타팀 연결 필요:
///   - ItemDropManager.ProcessPendingRequests: itemFactory.Create 주석 → 몬ster 드랍 시 필드 아이템 안 생김.
///   - ItemPickedUp 구독자 없음 → 인벤토리·HP회복·재화 반영 안 됨 (Character 쪽 구독 필요).
///   - CharacterEquipment.armorPartRules vs ItemFacade.TryGetArmorPart — 장착 시 둘 중 하나 연결 필요.
///   - 씬 Inspector: ItemFactory defaultPrefab·poolManager 수동 연결.
///   - CSV→SO 임포트(Tools/ExcelTest) 안 하면 장비 10010~10020 데이터 없음.
/// </remarks>
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
            && DataManager.instance.TryGetItemData(itemId, out ItemData data)
            && data.itemType == ItemType.Equipment)
        {
            stat = ItemEquipStat.FromData(data);
            return true;
        }

        stat = default;
        return false;
    }

    /// <summary>장비 id가 어느 슬롯 부위인지 조회.</summary>
    public bool TryGetArmorPart(int itemId, out CharacterArmorPart part)
    {
        return ItemArmorPartTable.TryGetPart(itemId, out part);
    }

    /// <summary>UI·장비 팝업용 SO 조회.</summary>
    public bool TryGetItemData(int itemId, out ItemData data)
    {
        if (DataManager.instance != null && DataManager.instance.TryGetItemData(itemId, out data))
        {
            return true;
        }

        data = null;
        return false;
    }

    /// <summary>아이템 초상화(아이콘). SO icon 미할당 시 null.</summary>
    public bool TryGetItemIcon(int itemId, out Sprite icon)
    {
        if (TryGetItemData(itemId, out ItemData data) && data.icon != null)
        {
            icon = data.icon;
            return true;
        }

        icon = null;
        return false;
    }

    /// <summary>강화/스타포스 반영 EffectiveValue.</summary>
    public bool TryGetEffectiveValue(int itemId, int upgradeLevel, int starForce, out int effectiveValue)
    {
        effectiveValue = 0;
        if (!TryGetItemData(itemId, out ItemData data))
        {
            return false;
        }

        effectiveValue = ItemValueEvaluator.Evaluate(
            data.itemType,
            data.value,
            data.upgradeStep,
            upgradeLevel,
            starForce);
        return true;
    }

    /// <summary>강화 UI 미리보기 (비용은 임시 공식).</summary>
    public bool TryGetUpgradePreview(int itemId, int currentUpgradeLevel, int starForce, out ItemUpgradePreview preview)
    {
        preview = default;
        if (!TryGetItemData(itemId, out ItemData data))
        {
            return false;
        }

        int currentLevel = UnityEngine.Mathf.Max(0, currentUpgradeLevel);
        int nextLevel = currentLevel + 1;

        preview = new ItemUpgradePreview
        {
            ItemId = itemId,
            CurrentUpgradeLevel = currentLevel,
            NextUpgradeLevel = nextLevel,
            CurrentEffectiveValue = ItemValueEvaluator.Evaluate(
                data.itemType, data.value, data.upgradeStep, currentLevel, starForce),
            NextEffectiveValue = ItemValueEvaluator.Evaluate(
                data.itemType, data.value, data.upgradeStep, nextLevel, starForce),
            UpgradeCost = ItemUpgradeCostProvider.Default.GetCost(currentLevel),
        };
        return true;
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

    // TODO(ItemDropManager): ProcessPendingRequests → itemFactory.Create 주석 해제 (손효림)
    // TODO(Character): ItemPickedUp 구독 → 인벤/회복/재화 (이동준)
    // TODO(Character): TryGetEquipStat / TryGetArmorPart → CharacterEquipment 장착 연결
    // TODO(Character): TryUpgrade(instanceId) — 재화 차감·인벤 upgradeLevel 저장 (이동준)
    // TODO(UI): EnchatPopupPresenter.RefreshForItem → TryGetUpgradePreview 연결
    // TODO(UI): 장착·인벤 슬롯 → TryGetItemIcon / EquipItem (EquipmentPopup)

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
