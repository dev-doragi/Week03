using System;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    None,

    Loading,
    Playing,
    Paused,
    Respawning,
    Death,
    GameOver,
    Clear
}

public class GameStateManager : MonoBehaviour, IInitializable
{
    public GameState CurrentState { get; private set; }

    public bool IsInitialized { get; private set; }

    public event Action<GameState> OnStateChanged;

    public void Initialize()
    {
        if (IsInitialized) return;
        IsInitialized = true;

        ChangeState(GameState.None);
    }

    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState)
            return;

        CurrentState = newState;
        OnStateChanged?.Invoke(CurrentState);
    }
}