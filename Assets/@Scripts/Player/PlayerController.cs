using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerInputReader))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerAim))]
[RequireComponent(typeof(PlayerCombat))]
public class PlayerController : MonoBehaviour
{
    private Rigidbody2D _rb;
    private PlayerInput _playerInput;
    private PlayerInputReader _inputReader;
    private PlayerMovement _movement;
    private PlayerAim _aim;
    private PlayerCombat _combat;
    private PauseController _pauseController;

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
    public bool IsControlEnabled { get; private set; } = true;

    public bool IsMoving => MoveInput.sqrMagnitude > 0.0001f;
    public Rigidbody2D Rigidbody => _rb;

    public event Action OnAttackPerformed;

    private PlayerHealth _playerHealth;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _playerInput = GetComponent<PlayerInput>();
        _inputReader = GetComponent<PlayerInputReader>();
        _movement = GetComponent<PlayerMovement>();
        _aim = GetComponent<PlayerAim>();
        _combat = GetComponent<PlayerCombat>();
        _playerHealth = GetComponent<PlayerHealth>();
        ManagerRegistry.TryGet(out _pauseController);
    }

    private void Update()
    {
        if (_inputReader.WasPausePressedThisFrame())
        {
            HandlePauseInput();
            return;
        }

        if ((_playerHealth != null && _playerHealth.IsDead) || !IsControlEnabled)
        {
            ClearFrameInput();
            return;
        }

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
        if ((_playerHealth != null && _playerHealth.IsDead) || !IsControlEnabled)
        {
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        _movement.HandleMove();
    }

    public void SetControlEnabled(bool enabled)
    {
        if (_playerInput == null)
            _playerInput = GetComponent<PlayerInput>();

        if (_rb == null)
            _rb = GetComponent<Rigidbody2D>();

        IsControlEnabled = enabled;

        if (_playerInput != null)
        {
            if (enabled)
            {
                _playerInput.enabled = true;
                _playerInput.SwitchCurrentActionMap("Player");
            }
            else
            {
                _playerInput.currentActionMap?.Disable();
            }
        }

        if (!enabled)
        {
            MoveInput = Vector2.zero;
            LookInput = Vector2.zero;
            IsAttackPressed = false;
            IsDashPressedThisFrame = false;
            IsReloadPressedThisFrame = false;

            _movement?.StopMovementImmediate();

            IsDashing = false;
            IsInvincible = false;

            if (_rb != null)
            {
                _rb.linearVelocity = Vector2.zero;
                _rb.angularVelocity = 0f;
            }
        }
    }

    public void ResetRuntimeState()
    {
        IsDashing = false;
        IsInvincible = false;
        IsAttackPressed = false;
        IsDashPressedThisFrame = false;
        IsReloadPressedThisFrame = false;
        MoveInput = Vector2.zero;
        LookInput = Vector2.zero;
        AimDirection = Vector2.right;
        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;
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

    private void ClearFrameInput()
    {
        MoveInput = Vector2.zero;
        LookInput = Vector2.zero;
        IsAttackPressed = false;
        IsDashPressedThisFrame = false;
        IsReloadPressedThisFrame = false;
    }

    private void HandlePauseInput()
    {
        if (_pauseController == null)
            return;

        GameStateManager gameStateManager;
        ManagerRegistry.TryGet(out gameStateManager);

        if (gameStateManager == null)
            return;

        GameState currentState = gameStateManager.CurrentState;

        if (currentState == GameState.GameOver || currentState == GameState.Clear)
            return;

        if (currentState == GameState.Playing)
        {
            _pauseController.PauseGame();
            return;
        }

        if (currentState == GameState.Paused)
        {
            _pauseController.ResumeGame();
        }
    }
}