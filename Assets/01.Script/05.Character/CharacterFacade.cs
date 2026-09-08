using UnityEngine;

public class CharacterFacade : MonoBehaviour
{
    [SerializeField] private CharacterControllers characterControllers;
    public CharacterStatus Status => characterControllers.Status;
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
        if (playerData == null)
        {
            Debug.LogError("플레이어 데이터 없음");
            return;
        }

        Data = playerData;

        // Status.Initialize(playerData);
        // 왠지 내가 다른 파일 잘못 건드린 것 같아서 비활성화
        // 당장은 Status에 있는 생성자에 있는 수치를 적용
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
}
