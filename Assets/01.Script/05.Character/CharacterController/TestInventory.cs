using System;
using System.Text;
using UnityEngine;

// 테스트 전용. Player 또는 자식 오브젝트에 붙인다.
public class TestInventory : MonoBehaviour
{
    [SerializeField] private CharacterControllers characterControllers;
    [SerializeField] private int testArmorId = 10001;
    [SerializeField, Min(0)] private int testBaseValue = 10;
    private Guid lastTestEquipmentId = Guid.Empty;

    private bool Ready()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("플레이 모드에서 실행해주세요.", this);
            return false;
        }

        if (characterControllers == null)
            characterControllers = GetComponentInParent<CharacterControllers>();

        if (characterControllers == null || characterControllers.Inventory == null)
        {
            Debug.LogWarning("CharacterControllers와 CharacterInventory를 연결해주세요.", this);
            return false;
        }
        return true;
    }

    [ContextMenu("Inventory/테스트 방어구 추가")]
    private void AddTestArmor()
    {
        if (!Ready()) return;

        // 실제 아이템 데이터 검색을 하지 않는 임시 상태. 공유 아이템 코드는 수정하지 않는다.
        ItemStatus source = new ItemStatus();
        source.Reset(testArmorId, ItemType.Equipment, testBaseValue, 1, 0, 0, testBaseValue);
        bool success = characterControllers.AddEquipment(source, out Guid instanceId);

        if (success)
            lastTestEquipmentId = instanceId;

        source.Clear();
        Debug.Log($"방어구 추가 {(success ? "성공" : "실패")} | 개별 ID: {instanceId}", this);
    }

    [ContextMenu("Inventory/마지막 테스트 방어구 제거")]
    private void RemoveTestArmor()
    {
        if (!Ready())
            return;

        bool success = characterControllers.RemoveEquipment(lastTestEquipmentId);

        if (success)
            lastTestEquipmentId = Guid.Empty;

        Debug.Log(success ? "방어구 제거 완료 (분해 보상 없음)" : "해당 방어구가 없습니다.", this);
    }

    [ContextMenu("Inventory/방어구 목록 출력")]
    private void PrintArmor()
    {
        if (!Ready())
            return;

        var equipment = characterControllers.GetInventoryEquipment();
        StringBuilder text = new StringBuilder($"보유 방어구: {equipment.Count}개\n");

        foreach (CharacterInventoryEquipment entry in equipment)
            text.AppendLine($"ID {entry.ItemId} | 개별 ID {entry.InstanceId} | 기본 수치 {entry.BaseValue}");

        Debug.Log(text.ToString(), this);
    }

    [ContextMenu("Inventory/방어구 데이터 점검")]
    private void CheckArmorRules()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("플레이 모드에서 실행해주세요.", this);
            return;
        }

        GameObject firstObject = new GameObject("ArmorInventoryTest");
        GameObject secondObject = new GameObject("OtherArmorInventoryTest");
        try
        {
            CharacterInventory first = firstObject.AddComponent<CharacterInventory>();
            CharacterInventory second = secondObject.AddComponent<CharacterInventory>();
            int changes = 0;
            first.EquipmentChanged += id => changes++;
            Check(!first.TryAddEquipment(null, out Guid rejected) && rejected == Guid.Empty, "null 입력 거부");

            ItemStatus source = new ItemStatus();
            source.Reset(100, ItemType.Weapon, 10, 1, 0, 0, 10);
            Check(!first.TryAddEquipment(source, out rejected) && rejected == Guid.Empty, "무기 거부");
            source.Reset(100, ItemType.Equipment, -1, 1, 0, 0, -1);
            Check(!first.TryAddEquipment(source, out rejected), "음수 기본 수치 거부");
            source.Reset(100, ItemType.Equipment, 10, 1, 2, 3, 99);
            Check(first.TryAddEquipment(source, out Guid firstId), "방어구 추가");
            Check(source.BaseValue == 10 && source.EffectiveValue == 99 && !source.PickedUp, "공유 상태를 변경하지 않음");
            Check(first.TryAddEquipment(source, out Guid secondId) && firstId != secondId, "같은 종류도 개별 보관");
            source.Clear();
            Check(first.TryGetEquipment(firstId, out CharacterInventoryEquipment saved) && saved.BaseValue == 10 && saved.ItemId == 100, "원본 초기화 후 기본값 보존");
            Check(first.GetEquipmentCount(100) == 2 && second.EquipmentCount == 0, "종류별 개수와 캐릭터별 독립");
            firstObject.SetActive(false);
            firstObject.SetActive(true);
            Check(first.EquipmentCount == 2, "재활성화 후 보유량 유지");
            var snapshot = first.GetEquipmentSnapshot();
            Check(first.TryRemoveEquipment(firstId) && first.TryGetEquipment(secondId, out _), "선택한 개별 ID만 제거");
            Check(!first.TryRemoveEquipment(firstId), "중복 제거 거부");
            Check(first.EquipmentCount == 1 && snapshot.Count == 2, "목록 복사본 유지");
            Check(changes == 3, "성공한 변경만 이벤트 발생");
            Debug.Log("방어구 데이터 점검 통과", this);
        }
        catch (Exception exception) { Debug.LogException(exception, this); }
        finally
        {
            Destroy(firstObject);
            Destroy(secondObject);
        }
    }

    private static void Check(bool condition, string description)
    {
        if (!condition) throw new InvalidOperationException($"방어구 점검 실패: {description}");
    }
}
