using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerInputReader : MonoBehaviour
{
    private const string PLAYER_ACTION_MAP = "Player";
    private const string MOVE_ACTION = "Move";
    private const string LOOK_ACTION = "Look";
    private const string ATTACK_ACTION = "Attack";
    private const string DASH_ACTION = "Dash";
    private const string RELOAD_ACTION = "Reload";
    private const string GAMEPAD_SCHEME = "Gamepad";


    private const string SYSTEM_ACTION_MAP = "System";
    private const string PAUSE_ACTION = "Pause";

    private PlayerInput _playerInput;
    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _attackAction;
    private InputAction _dashAction;
    private InputAction _reloadAction;

    private InputAction _pauseAction;

    public bool IsGamepadScheme =>
        string.Equals(_playerInput.currentControlScheme, GAMEPAD_SCHEME, StringComparison.Ordinal);

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();

        InputActionMap playerActionMap = _playerInput.actions.FindActionMap(PLAYER_ACTION_MAP, true);
        _moveAction = playerActionMap.FindAction(MOVE_ACTION, true);
        _lookAction = playerActionMap.FindAction(LOOK_ACTION, true);
        _attackAction = playerActionMap.FindAction(ATTACK_ACTION, true);
        _dashAction = playerActionMap.FindAction(DASH_ACTION, true);
        _reloadAction = playerActionMap.FindAction(RELOAD_ACTION, true);

        InputActionMap systemActionMap = _playerInput.actions.FindActionMap(SYSTEM_ACTION_MAP, true);
        _pauseAction = systemActionMap.FindAction(PAUSE_ACTION, true);
    }

    public Vector2 ReadMove()
    {
        return _moveAction.ReadValue<Vector2>();
    }

    public Vector2 ReadLook()
    {
        return _lookAction.ReadValue<Vector2>();
    }

    public bool IsAttackPressed()
    {
        return _attackAction.IsPressed();
    }

    public bool WasDashPressedThisFrame()
    {
        return _dashAction.WasPressedThisFrame();
    }

    public bool WasReloadPressedThisFrame()
    {
        return _reloadAction.WasPressedThisFrame();
    }

    public bool WasPausePressedThisFrame()
    {
        return _pauseAction.WasPressedThisFrame();
    }
}