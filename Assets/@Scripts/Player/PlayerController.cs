using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInputReader))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerAim))]
[RequireComponent(typeof(PlayerCombat))]
public class PlayerController : MonoBehaviour
{
    private Rigidbody2D _rb;
    private PlayerInputReader _inputReader;
    private PlayerMovement _movement;
    private PlayerAim _aim;
    private PlayerCombat _combat;

    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public Vector2 AimDirection { get; private set; } = Vector2.right;

    public bool IsAttackPressed { get; private set; }
    public bool IsDashPressedThisFrame { get; private set; }
    public bool IsReloadPressedThisFrame { get; private set; }
    public bool IsGamepadInput { get; private set; }

    public bool IsDashing { get; private set; }
    public bool IsInvincible { get; private set; }
    public bool IsFacingLeft { get; private set; }
    public bool IsDead { get; private set; }

    public bool IsMoving => MoveInput.sqrMagnitude > 0.0001f;
    public Rigidbody2D Rigidbody => _rb;

    public event Action OnAttackPerformed;
    public event Action OnDied;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _inputReader = GetComponent<PlayerInputReader>();
        _movement = GetComponent<PlayerMovement>();
        _aim = GetComponent<PlayerAim>();
        _combat = GetComponent<PlayerCombat>();
    }

    private void Update()
    {
        if (IsDead)
            return;

        MoveInput = _inputReader.ReadMove();
        LookInput = _inputReader.ReadLook();
        IsAttackPressed = _inputReader.IsAttackPressed();
        IsDashPressedThisFrame = _inputReader.WasDashPressedThisFrame();
        IsReloadPressedThisFrame = _inputReader.WasReloadPressedThisFrame();
        IsGamepadInput = _inputReader.IsGamepadScheme;

        _aim.UpdateAim();
        _movement.HandleDash();
        _combat.HandleAttack();
        _combat.HandleReload();
    }

    private void FixedUpdate()
    {
        if (IsDead)
        {
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        _movement.HandleMove();
    }

    public void SetAimDirection(Vector2 aimDirection)
    {
        if (aimDirection.sqrMagnitude <= 0.0001f)
            return;

        AimDirection = aimDirection.normalized;
    }

    public void SetFacingLeft(bool isFacingLeft)
    {
        IsFacingLeft = isFacingLeft;
    }

    public void SetDashing(bool isDashing)
    {
        IsDashing = isDashing;
    }

    public void SetInvincible(bool isInvincible)
    {
        IsInvincible = isInvincible;
    }

    public void RaiseAttackPerformed()
    {
        OnAttackPerformed?.Invoke();
    }

    public void Die()
    {
        if (IsDead)
            return;

        IsDead = true;
        IsDashing = false;
        IsAttackPressed = false;
        IsDashPressedThisFrame = false;
        IsReloadPressedThisFrame = false;
        MoveInput = Vector2.zero;
        _rb.linearVelocity = Vector2.zero;

        OnDied?.Invoke();
    }
}