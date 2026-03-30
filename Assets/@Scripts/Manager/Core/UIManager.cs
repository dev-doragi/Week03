using UnityEngine;

public class UIManager : MonoBehaviour, IInitializable
{
    public bool IsInitialized { get; private set; }

    [Header("Overlay Panels")]
    [SerializeField] private UI_Pause _uiPausePanel;
    [SerializeField] private UI_GameOver _uiGameOverPanel;
    [SerializeField] private UI_Clear _uiClearPanel;

    [Header("HUD")]
    [SerializeField] private UI_HUD _uiHud;

    private GameStateManager _gameStateManager;

    public void Initialize()
    {
        if (IsInitialized)
            return;

        if (!ManagerRegistry.TryGet(out _gameStateManager))
        {
            Debug.LogError($"{name}: GameStateManager not found.");
            return;
        }

        _gameStateManager.OnStateChanged += HandleStateChanged;

        IsInitialized = true;
    }

    public void RebindSceneUI()
    {
        _uiPausePanel = FindAnyObjectByType<UI_Pause>(FindObjectsInactive.Include);
        _uiGameOverPanel = FindAnyObjectByType<UI_GameOver>(FindObjectsInactive.Include);
        _uiClearPanel = FindAnyObjectByType<UI_Clear>(FindObjectsInactive.Include);
        _uiHud = FindAnyObjectByType<UI_HUD>(FindObjectsInactive.Include);

        HideOverlayPanels();
    }

    public void RebindUI(PlayerHealth playerHealth, PlayerCombat playerCombat)
    {
        if (_uiHud == null)
            return;

        if (playerHealth == null || playerCombat == null)
        {
            _uiHud.Unbind();
            return;
        }

        _uiHud.Bind(playerHealth, playerCombat);
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

            case GameState.Clear:
                ShowClear();
                break;

            default:
                HideOverlayPanels();
                break;
        }
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

        if (_uiClearPanel != null)
            _uiClearPanel.gameObject.SetActive(false);
    }

    public void ShowGameOver()
    {
        if (_uiGameOverPanel != null)
        {
            _uiGameOverPanel.gameObject.SetActive(true);
            _uiGameOverPanel.ApplyFirstSelection();
        }

        if (_uiPausePanel != null)
            _uiPausePanel.gameObject.SetActive(false);

        if (_uiClearPanel != null)
            _uiClearPanel.gameObject.SetActive(false);
    }

    public void ShowClear()
    {
        if (_uiClearPanel != null)
        {
            _uiClearPanel.gameObject.SetActive(true);
            _uiClearPanel.ApplyFirstSelection();
        }

        if (_uiPausePanel != null)
            _uiPausePanel.gameObject.SetActive(false);

        if (_uiGameOverPanel != null)
            _uiGameOverPanel.gameObject.SetActive(false);
    }

    public void HideOverlayPanels()
    {
        if (_uiPausePanel != null)
            _uiPausePanel.gameObject.SetActive(false);

        if (_uiGameOverPanel != null)
            _uiGameOverPanel.gameObject.SetActive(false);

        if (_uiClearPanel != null)
            _uiClearPanel.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (_gameStateManager != null)
            _gameStateManager.OnStateChanged -= HandleStateChanged;

        if (_uiHud != null)
            _uiHud.Unbind();
    }
}