using UnityEngine;
using UnityEngine.UI;

public class UI_GameOver : UI_Base
{
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _mainMenuButton;
    [SerializeField] private SceneLoader _sceneLoader;

    protected override void BindUI()
    {
        if (_restartButton != null)
            _restartButton.onClick.AddListener(HandleRestartClicked);

        if (_mainMenuButton != null)
            _mainMenuButton.onClick.AddListener(HandleMainMenuClicked);
    }

    protected override void UnbindUI()
    {
        if (_restartButton != null)
            _restartButton.onClick.RemoveListener(HandleRestartClicked);

        if (_mainMenuButton != null)
            _mainMenuButton.onClick.RemoveListener(HandleMainMenuClicked);
    }

    private void HandleRestartClicked()
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