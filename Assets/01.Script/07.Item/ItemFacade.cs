using System;
using UnityEngine;

/// <summary>
/// 아이템 시스템 외부 API.
/// Factory로 생성하고, Character 쪽에 장착 스탯 정보를 제공한다.
/// </summary>
/// <remarks>
/// [팀 전달]
/// ■ id는 Generated ItemId / DataManager ItemData.id(int) 사용. 별도 enum·매핑 테이블 없음.
/// ■ 장착 부위는 ItemData.armorPart (CharacterArmorPart enum).
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

    #region UI · Character 연동

    /// <summary>장착 가능 장비 SO 조회. 스탯은 itemType, value, upgradeStep, armorPart.</summary>
    public bool TryGetEquipmentData(int itemId, out ItemData data)
    {
        if (DataManager.instance != null
            && DataManager.instance.TryGetItemData(itemId, out data)
            && data.itemType == ItemType.Equipment)
        {
            return true;
        }

        data = null;
        return false;
    }

    /// <summary>ItemId enum으로 SO 조회.</summary>
    public bool TryGetItemData(ItemId itemId, out ItemData data)
    {
        return TryGetItemData((int)itemId, out data);
    }

    public bool TryGetItemData(int itemId, out ItemData data)
    {
        if (DataManager.instance != null && DataManager.instance.TryGetItemData(itemId, out data))
        {
            return true;
        }

        data = null;
        return false;
    }

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

    public bool TryGetArmorPart(int itemId, out CharacterArmorPart part)
    {
        if (TryGetEquipmentData(itemId, out ItemData data))
        {
            part = data.armorPart;
            return true;
        }

        part = default;
        return false;
    }

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

    public bool TryGetUpgradePreview(int itemId, int currentUpgradeLevel, int starForce, out ItemUpgradePreview preview)
    {
        preview = default;
        if (!TryGetItemData(itemId, out ItemData data))
        {
            return false;
        }

        int currentLevel = Mathf.Max(0, currentUpgradeLevel);
        int nextLevel = currentLevel + 1;

        preview = new ItemUpgradePreview
        {
            ItemId = (ItemId)itemId,
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

    public ItemController Spawn(int itemId, Vector3 position, Quaternion rotation, int upgradeLevel = 0, int starForce = 0)
    {
        if (ItemFactory.Instance == null)
        {
            Debug.LogWarning("[ItemFacade] ItemFactory is missing.");
            return null;
        }

        return ItemFactory.Instance.Create(itemId, position, rotation, 1, upgradeLevel, starForce);
    }

    public ItemController Spawn(int itemId, int amount, Vector3 position)
    {
        if (ItemFactory.Instance == null)
        {
            Debug.LogWarning("[ItemFacade] ItemFactory is missing.");
            return null;
        }

        return ItemFactory.Instance.Create(itemId, position, Quaternion.identity, amount);
    }

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

    public void Despawn(ItemController item)
    {
        if (item == null)
        {
            return;
        }

        item.ReturnToPool();
    }

    #endregion

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
