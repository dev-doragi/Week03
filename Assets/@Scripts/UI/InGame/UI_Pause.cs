using UnityEngine;
using UnityEngine.UI;

public class UI_Pause : UI_Base
{
    [Header("Pause Buttons")]
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _retryButton;
    [SerializeField] private Button _mainMenuButton;
    [SerializeField] private SceneLoader _sceneLoader;

    private PauseController _pauseController;

    protected override void CacheReferences()
    {
        ManagerRegistry.TryGet(out _pauseController);
    }

    protected override void BindUI()
    {
        if (_resumeButton != null)
            _resumeButton.onClick.AddListener(HandleResumeClicked);

        if (_retryButton != null)
            _retryButton.onClick.AddListener(HandleRetryClicked);

        if (_mainMenuButton != null)
            _mainMenuButton.onClick.AddListener(HandleMainMenuClicked);
    }

    protected override void UnbindUI()
    {
        if (_resumeButton != null)
            _resumeButton.onClick.RemoveListener(HandleResumeClicked);

        if (_retryButton != null)
            _retryButton.onClick.RemoveListener(HandleRetryClicked);

        if (_mainMenuButton != null)
            _mainMenuButton.onClick.RemoveListener(HandleMainMenuClicked);
    }

    private void HandleResumeClicked()
    {
        _pauseController?.ResumeGame();
    }

    private void HandleRetryClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RestartGame();
    }

    private void HandleMainMenuClicked()
    {
        if (_sceneLoader != null)
            _sceneLoader.LoadMainMenu();
    }
}