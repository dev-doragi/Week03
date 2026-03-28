using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UI_Pause : UI_Base
{
    [Header("Pause Buttons")]
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _retryButton;
    [SerializeField] private Button _mainMenuButton;

    private PauseController _pauseController;
    private InputManager _inputManager;
    private GameStateManager _gameStateManager;

    public event System.Action OnRetryRequested;
    public event System.Action OnMainMenuRequested;

    protected override void Awake()
    {
        ManagerRegistry.TryGet(out _pauseController);
        ManagerRegistry.TryGet(out _inputManager);
        ManagerRegistry.TryGet(out _gameStateManager);

        base.Awake();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (_inputManager != null)
            _inputManager.OnCancel += HandleCancel;
    }

    protected override void BindEvents()
    {
        if (_resumeButton != null)
            _resumeButton.onClick.AddListener(() => _pauseController?.ResumeGame());

        if (_retryButton != null)
            _retryButton.onClick.AddListener(() => OnRetryRequested?.Invoke());

        if (_mainMenuButton != null)
            _mainMenuButton.onClick.AddListener(() => OnMainMenuRequested?.Invoke());
    }

    private void HandleCancel(InputAction.CallbackContext ctx)
    {
        if (!ctx.started)
            return;

        if (_gameStateManager != null && _gameStateManager.CurrentState != GameState.Paused)
            return;
        EventSystem.current?.SetSelectedGameObject(null);
        _pauseController?.ResumeGame();
    }

    private void OnDisable()
    {
        if (_inputManager != null)
            _inputManager.OnCancel -= HandleCancel;
    }
}