using UnityEngine;
using System;
using System.Text;

public class CharacterEquipmentTester : MonoBehaviour
{
    [SerializeField] private CharacterControllers characterControllers;
    [SerializeField] private int testArmorId = 10001;
    [SerializeField, Min(0)] private int testBaseValue = 10;
    [SerializeField] private CharacterEquipmentSlot targetSlot = CharacterEquipmentSlot.Hat;
    private Guid lastCreatedId;

    private bool Ready()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("플레이 모드에서 실행해주세요.", this);
            return false;
        }

        if (characterControllers == null)
            characterControllers = GetComponentInParent<CharacterControllers>();

        if (characterControllers == null || characterControllers.Inventory == null || characterControllers.Equipment == null)
        {
            Debug.LogWarning("Player의 Controller, Inventory, Equipment를 확인해주세요.", this);
            return false;
        }

        return true;
    }

    [ContextMenu("Equipment/테스트 방어구 추가")]
    private void AddTestArmor()
    {
        if (!Ready())
            return;

        ItemStatus source = new ItemStatus();
        int value = Mathf.Max(0, testBaseValue);
        source.Reset(testArmorId, ItemType.Equipment, value, 1, 0, 0, value);

        if (characterControllers.AddEquipment(source, out Guid id))
        {
            lastCreatedId = id;
            Debug.Log($"방어구 추가 | 아이템 ID {testArmorId} | 개별 ID {id}", this);
        }

        source.Clear();
    }

    [ContextMenu("Equipment/마지막 추가 방어구 착용")]
    private void EquipLast()
    {
        if (!Ready())
            return;

        bool result = characterControllers.EquipEquipment(lastCreatedId, targetSlot);
        Debug.Log(result ? $"{targetSlot} 착용 성공" : "착용 실패: 부위 규칙/중복 ID 규칙/대상 슬롯/이미 착용 중인지 확인하세요.", this);
    }

    [ContextMenu("Equipment/선택 슬롯 해제")]
    private void Unequip()
    {
        if (!Ready())
            return;

        Debug.Log(characterControllers.UnequipEquipment(targetSlot) ? "해제 성공" : "빈 슬롯 또는 잘못된 슬롯", this);
    }

    [ContextMenu("Equipment/마지막 추가 방어구 제거 시도")]
    private void RemoveLast()
    {
        if (!Ready())
            return;

        // Controller를 거치지 않아도 인벤토리 자체가 장착 중 제거를 거부해야 한다.
        bool result = characterControllers.Inventory.TryRemoveEquipment(lastCreatedId);
        Debug.Log(result ? "제거 성공" : "제거 거부: 장착 중이거나 보유하지 않은 방어구", this);
    }

    [ContextMenu("Equipment/전체 슬롯과 보유 목록 출력")]
    private void PrintState()
    {
        if (!Ready())
            return;

        var text = new StringBuilder("장비 슬롯\n");

        foreach (var pair in characterControllers.GetEquipmentSlots())
        {
            if (characterControllers.TryGetEquippedItem(pair.Key, out CharacterInventoryEquipment item))
                text.AppendLine($"{pair.Key}: 아이템 {item.ItemId}, 개별 ID {item.InstanceId}");
            else
                text.AppendLine($"{pair.Key}: 비어 있음");
        }
        text.AppendLine($"보유 방어구 총 {characterControllers.Inventory.EquipmentCount}개 (장착 포함)");
        foreach (var item in characterControllers.GetInventoryEquipment())
            text.AppendLine($"{item.InstanceId}: 아이템 {item.ItemId}, 착용 {characterControllers.IsEquipmentEquipped(item.InstanceId)}");
        Debug.Log(text.ToString(), this);
    }
}
