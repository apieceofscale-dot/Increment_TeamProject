using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour, IBootStrapper
{
    [SerializeField] private CharacterControllers characterControllers;
    [SerializeField] private int bootOrder = 1100;
    public int BootOrder => bootOrder;
    public bool IsInitialized => characterControllers != null && characterControllers.IsInitialized;
    public bool CanRun => characterControllers != null && characterControllers.CanRun;
    internal void Inject(CharacterControllers owner)
    {
        if (owner == null)
            throw new System.ArgumentNullException(nameof(owner));

        if (characterControllers != null && characterControllers != owner)
            throw new System.InvalidOperationException("연결된 캐릭터와 주입 대상이 다름");

        characterControllers = owner;
    }
    public void IBootStrapperInject(BootstrapContext context)
    {
        Inject(characterControllers != null ? characterControllers : GetComponentInParent<CharacterControllers>());
    }
    public void IBootStrapperInitialize()
    {
        if (!IsInitialized)
            throw new System.InvalidOperationException("컨트롤러 초기화 먼저 완료해야함");
    }

    private void Update()
    {
        if (!CanRun || Keyboard.current == null || !characterControllers.isActiveAndEnabled)
            return;

        HandleMovement();
        HandleJump();
        HandleSkills();
        HandleAttack();
    }

    private void HandleMovement()
    {
        float moveInput = 0f;

        if (Keyboard.current.aKey.isPressed)
            moveInput -= 1f;

        if (Keyboard.current.dKey.isPressed)
            moveInput += 1f;

        characterControllers.SetMoveInput(moveInput);
    }

    private void HandleJump()
    {
        if (!Keyboard.current.spaceKey.wasPressedThisFrame)
            return;

        characterControllers.Jump();
    }

    private void HandleAttack()
    {
        if (!Keyboard.current.jKey.wasPressedThisFrame)
            return;

        characterControllers.TryAttack();
    }

    private void HandleSkills()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
            characterControllers.UseSkill(0);

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
            characterControllers.UseSkill(1);

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
            characterControllers.UseSkill(2);

        if (Keyboard.current.digit4Key.wasPressedThisFrame)
            characterControllers.UseSkill(3);

        if (Keyboard.current.digit5Key.wasPressedThisFrame)
            characterControllers.UseSkill(4);

        if (Keyboard.current.digit6Key.wasPressedThisFrame)
            characterControllers.UseSkill(5);
    }

    private void OnDisable()
    {
        if (characterControllers != null)
            characterControllers.SetMoveInput(0f);
    }
}
