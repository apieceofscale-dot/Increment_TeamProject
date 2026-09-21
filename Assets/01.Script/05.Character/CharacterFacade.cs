using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterFacade : MonoBehaviour, IBootStrapper
{
    
    [SerializeField] private CharacterControllers characterControllers;
    private CharacterControllers Controller => characterControllers;

    [SerializeField] private int bootOrder = 1100;
    public int BootOrder => bootOrder;
    public bool IsInitialized => characterControllers != null && characterControllers.IsInitialized;
    public bool CanRun => characterControllers != null && characterControllers.CanRun;
    internal void Inject(CharacterControllers owner)
    {
        if (owner == null)
            throw new System.ArgumentNullException(nameof(owner));

        if (characterControllers != null && characterControllers != owner)
            throw new System.InvalidOperationException("ùùùùù ?ùùù?ù ùùùù ùùù ù?ù");

        characterControllers = owner;
    }
    public void IBootStrapperInject(BootstrapContext context)
    {
        Inject(characterControllers != null ? characterControllers : GetComponentInParent<CharacterControllers>());
    }

    public void IBootStrapperInitialize()
    {
        if (!IsInitialized)
            throw new System.InvalidOperationException("Controller must be initialized before CharacterFacade boot.");
    }

    #region ???? ?? ??? ???? ??
    public CharacterStatus Status => Controller.Status;
    public CharacterInventory Inventory => Controller.Inventory;
    public CharacterEquipment Equipment => Controller.Equipment;
    #endregion

    #region ùù???? ???? ?? ????
    public PlayerData Data => Controller.Data;
    public void Initialize(PlayerData playerData)
    {
        Controller.Initialize(playerData);
    }
    #endregion

    #region UI?? ???ùù ???
    public long CurrentHp => Controller.CurrentHp; // ???? ???
    public long MaxHp => Controller.MaxHp; // ??? ???
    public int CurrentMp => Controller.CurrentMp; // ???? ????
    public int MaxMp => Controller.MaxMp; // ??? ????
    public int Level => Controller.Level; // ???? ????
    public long Money => Controller.Money; // ???? ???
    public int CombatPower => Controller.CombatPower; // ???? ???? ?????
    public float AttackRating => Controller.AttackRating;
    public long CurrentExp => Controller.CurrentExp;
    public long RequiredExpForCurrentLevel => Controller.RequiredExpForCurrentLevel;
    public string JobName => Controller.JobName; // ???? ???

    public Sprite GetCharacterPortrait()
    {
        return Controller.GetCharacterPortrait(); // Controller?? ???? ??? ??? ????
    }
    #endregion

    #region UI?? ???? ???? ????
    public event Action<long, long> HpChanged // ???? ???, ??? ??? ???? ????
    {
        add { Controller.HpChanged += value; }
        remove
        {
            if (Controller != null)
                Controller.HpChanged -= value;
        }
    }

    public event Action<int, int> MpChanged // ??????, ??? ???? ???? ????
    {
        add { Controller.MpChanged += value; }
        remove
        {
            if (Controller != null)
                Controller.MpChanged -= value;
        }
    }

    public event Action<int> LevelChanged // ???? ????
    {
        add { Controller.LevelChanged += value; }
        remove
        {
            if (Controller != null)
                Controller.LevelChanged -= value;
        }
    }

    public event Action<long> MoneyChanged // ??? ????
    {
        add { Controller.MoneyChanged += value; }
        remove
        {
            if (Controller != null)
                Controller.MoneyChanged -= value;
        }
    }

    public event Action<int> CombatPowerChanged // ???? ????? ????
    {
        add { Controller.CombatPowerChanged += value; }
        remove
        {
            if (Controller != null)
                Controller.CombatPowerChanged -= value;
        }
    }

    public event Action<float> AttackRatingChanged
    {
        add { Controller.AttackRatingChanged += value; }
        remove
        {
            if (Controller != null)
                Controller.AttackRatingChanged -= value;
        }
    }

    public event Action<long, long> ExpChanged
    {
        add { Controller.ExpChanged += value; }
        remove
        {
            if (Controller != null)
                Controller.ExpChanged -= value;
        }
    }

    public event Action<string> JobNameChanged // ???? ??? ????
    {
        add { Controller.JobNameChanged += value; }
        remove
        {
            if (Controller != null)
                Controller.JobNameChanged -= value;
        }
    }
    public void RefreshUiEvents()
    {
        Controller.RefreshUiEvents(); // ??? ???? ?? ??????? ????
    }
    #endregion

    #region ??? ?? ??? UI??
    public bool EquipEquipment(Guid instanceId, CharacterEquipmentSlot slot) // ??? ????
    {
        return Controller.EquipEquipment(instanceId, slot);
        // Hat(???)=0, Top(????)=1, Bottom(????)=2, Gloves(??)=3, Cape(????)=4, Shoulder(???)=5, Belt(??)=6, Shoes(???)=7, Ring1(????1)=8, Ring2(????2)=9, Necklace(?????)=10
    }

    public bool UnequipEquipment(CharacterEquipmentSlot slot) // ??? ????
    {
        return Controller.UnequipEquipment(slot);
    }

    public bool IsEquipmentEquipped(Guid instanceId) // ??? ?????????
    {
        return Controller.IsEquipmentEquipped(instanceId);
    }

    public bool TryGetEquippedItem(CharacterEquipmentSlot slot, out CharacterInventoryEquipment item) // ?????? ??? ???
    {
        return Controller.TryGetEquippedItem(slot, out item);
    }

    public IReadOnlyDictionary<CharacterEquipmentSlot, Guid> GetEquipmentSlots()
    {
        return Controller.GetEquipmentSlots();
    }

    public event Action InventoryChanged // ?ùù??? ?? ?????? ??? ????? ??????? ???? ???
    {
        add
        {
            Controller.InventoryChanged += value;
        }
        remove
        {
            if (Controller != null)
                Controller.InventoryChanged -= value;
        }
    }

    public event Action EquipmentChanged // ???? ? ?????
    {
        add
        {
            Controller.EquipmentChanged += value;
        }
        remove
        {
            if (Controller != null)
                Controller.EquipmentChanged -= value;
        }
    }

    public IReadOnlyList<CharacterInventoryEquipment> GetEquipmentInventory() // ???? ??? ?????? ?ùù? ???? ????
    {
        return Controller.GetEquipmentInventory();
    }

    public bool TryGetItemData(int itemId, out ItemData data)
    {
        return Controller.TryGetItemData(itemId, out data);
    }

    public void EquipItem(Guid instanceId) //  ???? ??? ????/???
    {
        Controller.EquipItem(instanceId);
    }

    public bool TryEquipItem(Guid instanceId) // ????/???? ??ùù? ????? ?? ???
    {
        return Controller.TryEquipItem(instanceId);
    }

    public bool UnequipItem(CharacterEquipmentSlot slot) // ????? ???, ??????? ???????? ?????? ???? ??? ???
    {
        return Controller.UnequipItem(slot);
    }
    #endregion

    #region ??? UI ?? ??? ??????
    public int SkillSlotCount => CharacterControllers.SkillSlotCount; // 6??, ?ùù????? 0~5

    // ?????? ???? ???????? ??? ???? ????? ????
    public event Action<SkillSlotInfo> SkillSlotChanged
    {
        add { Controller.SkillSlotChanged += value; }
        remove
        {
            if (Controller != null)
                Controller.SkillSlotChanged -= value;
        }
    }

    // ????? ???? 6??
    public IReadOnlyList<CharacterControllers.SkillCooldownChannel> SkillCooldownEvents => Controller.SkillCooldownEvents;

    // ???????? ???? ?? 0~5?? ?????? ??????/????? ???? ??????? ?????
    public SkillSlotInfo GetEquippedSkill(int slotIndex) => Controller.GetEquippedSkill(slotIndex);
    public SkillCooldownInfo GetSkillCooldown(int slotIndex) => Controller.GetSkillCooldown(slotIndex);

    // ???? ùù?????? ??? ????????? ????
    public bool EquipSkill(int slotIndex, CharacterSkillBase skill) => Controller.EquipSkill(slotIndex, skill);
    public bool UnequipSkill(int slotIndex) => Controller.UnequipSkill(slotIndex);

    // UI ????? ????? ????
    public bool UseSkill(int slotIndex) => Controller.UseSkill(slotIndex);
    #endregion

    #region ???? ?? ??????
    public void TakeDamage(long damage)
    {
        Controller.Status.TakeDamage(damage);
    }

    public void RecoverHp(long amount)
    {
        Controller.Status.RecoverHp(amount);
    }

    public bool UseMp(int amount)
    {
        return Controller.Status.UseMp(amount);
    }

    public void RecoverMp(int amount)
    {
        Controller.Status.RecoverMp(amount);
    }
    #endregion

    #region ???? ?? ??? ??
    public void GainExp(long amount)
    {
        Controller.GainExp(amount);
    }

    public void SetMoney(long amount)
    {
        Controller.SetMoney(amount); // ??? ??? ????
    }
    #endregion

    #region ?????? ?? ?ùù??? ??
    public bool AddEquipment(ItemStatus status, out Guid instanceId) // ??? 1?? ???, ???? ???ùù? ???? ID ???
    {
        return Controller.AddEquipment(status, out instanceId);
    }

    public bool RemoveEquipment(Guid instanceId) // ???? ??? ????
    {
        return Controller.RemoveEquipment(instanceId);
    }

    public bool TryGetEquipment(Guid instanceId, out CharacterInventoryEquipment equipment) // ???? ??? ???
    {
        return Controller.TryGetEquipment(instanceId, out equipment);
    }

    public int GetEquipmentCount(int itemId) // ???? ?????? ID?? ???? ????
    {
        return Controller.GetEquipmentCount(itemId);
    }

    public IReadOnlyList<CharacterInventoryEquipment> GetInventoryEquipment() // ?ùù? ???? ??? ????
    {
        return Controller.GetInventoryEquipment();
    }
    #endregion

    #region ?????? ???? ???? ??
    // ???? ???? ??? ????? ??? ???, ?????? ????? 0
    public void SetEquipmentStats(
        long maxHp = 0,
        int maxMp = 0,
        int recoverMpPerSec = 0,
        float moveSpeed = 0f,
        int strength = 0,
        int dexterity = 0,
        int intelligence = 0,
        int luck = 0,
        int attack = 0,
        float attackSpeedRate = 0f,
        int hitRate = 0,
        float criticalRate = 0f,
        float criticalDamage = 0f,
        float damageByMainStat = 0f,
        float damageOnBoss = 0f,
        float damageOnNormal = 0f,
        float armorPenetration = 0f,
        float finalDamage = 0f,
        long defence = 0,
        int dodgeRate = 0)
    {
        Controller.SetEquipmentStats(
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

    public bool RefreshEquipmentStats()
    {
        return Controller.RefreshEquipmentStats(); // ???????????? ????? ????
    }

    public void ClearEquipmentStats()
    {
        Controller.ClearEquipmentStats();
    }
    #endregion

    #region ??????
    public void SetAutoFarming(bool enabled) // ?????? ON/OFF
    {
        Controller.SetAutoFarming(enabled); 
    }

    public bool IsAutoFarming => Controller.IsAutoFarming;
    public string AutoFarmingState => Controller.AutoFarmingState;

    public void SetAutoFarmingTargetFilter(Func<Collider2D, bool> filter)
    {
        Controller.SetAutoFarmingTargetFilter(filter);
    }
    #endregion

    #region ùù???? ???? ?? ?????????
    public void SetMoveInput(float input)
    {
        Controller.SetMoveInput(input);
    }

    public void Jump()
    {
        Controller.Jump();
    }

    public bool Attack()
    {
        return Controller.TryAttack();
    }

    public bool SetBasicAttackReplacement(CharacterSkillBase skill)
    {
        return Controller.SetBasicAttackReplacement(skill);
    }

    public void RestoreBasicAttack()
    {
        Controller.RestoreBasicAttack();
    }
    #endregion

    #region ???? ?? ???? ??
    public bool ChangeJob(int id)
    {
        return Controller.ChangeJob(id);
    }
    #endregion

    #region ???? ?? ???? ??
    public bool ChangeNextJob()
    {
        return Controller.ChangeNextJob();
    }

    public bool SetSlashBasicAttack()
    {
        return Controller.SetSlashBasicAttack();
    }

    public bool SetProjectileBasicAttack()
    {
        return Controller.SetProjectileBasicAttack();
    }

    public bool UseSkillSlash()
    {
        return Controller.UseSkillSlash();
    }

    public bool UseSkillProjectile()
    {
        return Controller.UseSkillProjectile();
    }

    public bool UseSkillAttackBuff()
    {
        return Controller.UseSkillAttackBuff();
    }

    public void TestSkillLevelUp()
    {
        Controller.TestSkillLevelUp();
    }
    #endregion
}