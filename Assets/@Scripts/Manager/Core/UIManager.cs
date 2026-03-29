// 5. UIManager.cs
using UnityEngine;

public class UIManager : MonoBehaviour, IInitializable
{
    public bool IsInitialized { get; private set; }

    [Header("Overlay Panels")]
    [SerializeField] private UI_Pause _uiPausePanel;
    [SerializeField] private UI_GameOver _uiGameOverPanel;

    [Header("HUD")]
    [SerializeField] private UI_HUD _uiHud;

    private GameStateManager _gameStateManager;
    private PauseController _pauseController;
    private PoolManager _poolManager;

    public void Initialize()
    {
        if (IsInitialized)
            return;

        if (!ManagerRegistry.TryGet(out _gameStateManager))
        {
            Debug.LogError($"{name}: GameStateManager not found.");
            return;
        }

        ManagerRegistry.TryGet(out _pauseController);
        ManagerRegistry.TryGet(out _poolManager);

        _gameStateManager.OnStateChanged += HandleStateChanged;
        BindPanelEvents();
        RebindUI();

        IsInitialized = true;
    }

    public void RebindUI()
    {
        if (_uiHud != null)
        {
            _uiHud.Unbind();
        }

        PlayerController playerController = FindAnyObjectByType<PlayerController>();
        if (playerController == null)
        {
            Debug.LogWarning("UIManager.RebindUI: PlayerController not found.");
            return;
        }

        PlayerHealth playerHealth = playerController.GetComponent<PlayerHealth>();
        PlayerCombat playerCombat = playerController.GetComponent<PlayerCombat>();

        if (_uiHud != null)
        {
            _uiHud.Bind(playerHealth, playerCombat);
        }
    }

    private void BindPanelEvents()
    {
        if (_uiPausePanel != null)
        {
            _uiPausePanel.OnRetryRequested += HandleRetryRequested;
            _uiPausePanel.OnMainMenuRequested += HandleMainMenuRequested;
        }

        if (_uiGameOverPanel != null)
        {
            _uiGameOverPanel.OnRetryRequested += HandleRetryRequested;
            _uiGameOverPanel.OnMainMenuRequested += HandleMainMenuRequested;
        }
    }

    private void UnbindPanelEvents()
    {
        if (_uiPausePanel != null)
        {
            _uiPausePanel.OnRetryRequested -= HandleRetryRequested;
            _uiPausePanel.OnMainMenuRequested -= HandleMainMenuRequested;
        }

        if (_uiGameOverPanel != null)
        {
            _uiGameOverPanel.OnRetryRequested -= HandleRetryRequested;
            _uiGameOverPanel.OnMainMenuRequested -= HandleMainMenuRequested;
        }
    }

    private void HandleStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.Paused:
                ShowPause();
                break;

            case GameState.GameOver:
                ShowGameOver();
                break;

            default:
                HideOverlayPanels();
                break;
        }
    }

    private void HandleRetryRequested()
    {
        _gameStateManager.ChangeState(GameState.Respawning);
        _pauseController?.ResumeGame();
        _poolManager?.ClearRuntimeObjects();
    }

    private void HandleMainMenuRequested()
    {
        _poolManager?.ClearRuntimeObjects();
    }

    public void ShowPause()
    {
        if (_uiPausePanel != null)
        {
            _uiPausePanel.gameObject.SetActive(true);
            _uiPausePanel.ApplyFirstSelection();
        }

        if (_uiGameOverPanel != null)
            _uiGameOverPanel.gameObject.SetActive(false);
    }

    public void ShowGameOver()
    {
        if (_uiPausePanel != null)
            _uiPausePanel.gameObject.SetActive(false);

        if (_uiGameOverPanel != null)
            _uiGameOverPanel.gameObject.SetActive(true);
    }

    public void HideOverlayPanels()
    {
        if (_uiPausePanel != null)
            _uiPausePanel.gameObject.SetActive(false);

        if (_uiGameOverPanel != null)
            _uiGameOverPanel.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (_gameStateManager != null)
            _gameStateManager.OnStateChanged -= HandleStateChanged;

        UnbindPanelEvents();

        if (_uiHud != null)
            _uiHud.Unbind();
    }
}