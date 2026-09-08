using UnityEngine;

public class CharacterFacade : MonoBehaviour
{
    [SerializeField] private CharacterControllers characterControllers;
    public CharacterStatus Status => characterControllers.Status;

    private void Awake()
    {
        if(characterControllers == null)
            characterControllers = GetComponent<CharacterControllers>();
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

    public bool UseTestSkill() // 테스트용 임시 메서드
    {
        return characterControllers.UseTestSkill();
    }

    public void RecoverMp(int amount)
    {
        characterControllers.Status.RecoverMp(amount);
    }
}
