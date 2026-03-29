using UnityEngine;

public class PauseController : MonoBehaviour, IInitializable
{
    public bool IsInitialized { get; private set; }

    private GameStateManager _gameStateManager;
    private GameManager _gameManager;

    public void Initialize()
    {
        if (IsInitialized)
            return;

        ManagerRegistry.TryGet(out _gameStateManager);
        ManagerRegistry.TryGet(out _gameManager);

        if (_gameStateManager == null)
        {
            Debug.LogError($"{name}: PauseController Initialize failed.");
            return;
        }

        IsInitialized = true;
    }

    public void PauseGame()
    {
        if (_gameManager != null)
            _gameManager.SetPlayerControlEnabled(false);

        Time.timeScale = 0f;
        _gameStateManager.ChangeState(GameState.Paused);

        Debug.Log("Game Paused");
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;

        if (_gameManager != null)
            _gameManager.SetPlayerControlEnabled(true);

        _gameStateManager.ChangeState(GameState.Playing);

        Debug.Log("Game Resumed");
    }

    private void OnDestroy()
    {
        if (Time.timeScale == 0f)
            Time.timeScale = 1f;
    }
}