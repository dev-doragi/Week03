using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private string _mainMenuSceneName = "MainMenu";
    [SerializeField] private string _gameplaySceneName = "StageScene";
    [SerializeField] private string _tutorialSceneName = "TutorialScene";

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadSceneAsync(_mainMenuSceneName);
    }

    public void LoadGameplay()
    {
        Time.timeScale = 1f;
        SceneManager.LoadSceneAsync(_gameplaySceneName);
    }

    public void LoadTutorial()
    {
        Time.timeScale = 1f;
        SceneManager.LoadSceneAsync(_tutorialSceneName);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}