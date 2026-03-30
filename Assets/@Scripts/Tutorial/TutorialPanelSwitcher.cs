using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialPanelSwithcer : MonoBehaviour
{
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private GameObject _keyboardMousePanel;
    [SerializeField] private GameObject _gamepadPanel;

    private void OnEnable()
    {
        if (_playerInput != null)
            _playerInput.onControlsChanged += HandleControlsChanged;

        RefreshPanel();
    }

    private void OnDisable()
    {
        if (_playerInput != null)
            _playerInput.onControlsChanged -= HandleControlsChanged;
    }

    private void HandleControlsChanged(PlayerInput playerInput)
    {
        RefreshPanel();
    }

    private void RefreshPanel()
    {
        if (_playerInput == null)
            return;

        bool isGamepad = _playerInput.currentControlScheme == "Gamepad";

        if (_keyboardMousePanel != null)
            _keyboardMousePanel.SetActive(!isGamepad);

        if (_gamepadPanel != null)
            _gamepadPanel.SetActive(isGamepad);
    }
}