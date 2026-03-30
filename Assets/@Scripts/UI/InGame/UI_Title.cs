using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_Title : UI_Base
{
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _guideButton;
    [SerializeField] private Button _quitButton;
    [SerializeField] private SceneLoader _sceneLoader;

    protected override void BindUI()
    {
        if (_startButton != null)
            _startButton.onClick.AddListener(HandleStartClicked);

        if (_guideButton != null)
            _guideButton.onClick.AddListener(HandleGuideClicked);

        if (_quitButton != null)
            _quitButton.onClick.AddListener(HandleQuitClicked);
    }

    protected override void UnbindUI()
    {
        if (_startButton != null)
            _startButton.onClick.RemoveListener(HandleStartClicked);

        if (_guideButton != null)
            _guideButton.onClick.RemoveListener(HandleGuideClicked);

        if (_quitButton != null)
            _quitButton.onClick.RemoveListener(HandleQuitClicked);
    }

    private void HandleStartClicked()
    {
        if (_sceneLoader != null)
            _sceneLoader.LoadGameplay();
    }

    private void HandleGuideClicked()
    {
        if (_sceneLoader != null)
            _sceneLoader.LoadTutorial();
    }

    private void HandleQuitClicked()
    {
        if (_sceneLoader != null)
            _sceneLoader.QuitGame();
    }
}