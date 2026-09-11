using System.Collections.Generic;
using UnityEngine;

public class CharacterFacade : MonoBehaviour
{
    [SerializeField] private CharacterControllers characterControllers;
    public CharacterStatus Status => characterControllers.Status;
    public CharacterInventory Inventory => characterControllers.Inventory;
    public PlayerData Data { get; private set; }

    private void Awake()
    {
        if (characterControllers == null)
            characterControllers = GetComponent<CharacterControllers>();

        if (characterControllers == null)
            Debug.LogError("캐릭터 컨트롤러 없음", this);
    }

    public void Initialize(PlayerData playerData)
    {
        characterControllers.Initialize(playerData);
    }

    public void GainExp(long amount)
    {
        characterControllers.GainExp(amount);
    }

    public void TakeDamage(long damage)
    {
        characterControllers.Status.TakeDamage(damage);
    }

    public void RecoverHp(long amount)
    {
        characterControllers.Status.RecoverHp(amount);
    }

    public bool UseMp(int amount)
    {
        return characterControllers.Status.UseMp(amount);
    }

    public void SetMoveInput(float input)
    {
        characterControllers.SetMoveInput(input);
    }

    public void Jump()
    {
        characterControllers.Jump();
    }

    public bool Attack()
    {
        return characterControllers.TryAttack();
    }

    public bool ChangeJob(int id) 
    {
        return characterControllers.ChangeJob(id); 
    }

    public bool ChangeNextJob() 
    { 
        return characterControllers.ChangeNextJob(); 
    }

    public void RestoreBasicAttack() 
    { 
        characterControllers.RestoreBasicAttack(); 
    }

    public bool SetSlashBasicAttack() 
    { 
        return characterControllers.SetSlashBasicAttack(); 
    }

    public bool SetProjectileBasicAttack() 
    { 
        return characterControllers.SetProjectileBasicAttack(); 
    }

    public bool SetBasicAttackReplacement(CharacterSkillBase skill)
    {
        return characterControllers.SetBasicAttackReplacement(skill);
    }

    public bool UseSkillSlash()
    {
        return characterControllers.UseSkillSlash();
    }

    public bool UseSkillProjectile()
    {
        return characterControllers.UseSkillProjectile();
    }

    public bool UseSkillAttackBuff()
    {
        return characterControllers.UseSkillAttackBuff();
    }

    public void TestSkillLevelUp()
    {
        characterControllers.TestSkillLevelUp();
    }

    public void RecoverMp(int amount)
    {
        characterControllers.Status.RecoverMp(amount);
    }

    public bool AddEquipment(ItemStatus status, out System.Guid instanceId) // 방어구 1개 추가, 성공 여부와 개별 ID 반환
    {
        return characterControllers.AddEquipment(status, out instanceId);
    }

    public bool RemoveEquipment(System.Guid instanceId) // 개별 방어구 제거
    {
        return characterControllers.RemoveEquipment(instanceId);
    }

    public bool TryGetEquipment(System.Guid instanceId, out CharacterInventoryEquipment equipment) // 개별 방어구 조회
    {
        return characterControllers.TryGetEquipment(instanceId, out equipment);
    }

    public int GetEquipmentCount(int itemId) // 같은 아이템 ID의 보유 갯수
    {
        return characterControllers.GetEquipmentCount(itemId);
    }

    public IReadOnlyList<CharacterInventoryEquipment> GetInventoryEquipment() // 읽기 전용 목록 복사본
    {
        return characterControllers.GetInventoryEquipment();
    }

}
