using ElephantSDK;
using UnityEngine;

public class StateManager : Singleton<StateManager>
{
    [SerializeField]
    private GameState gameState = GameState.None;

    public GameState CurrentState => gameState;

    private void StartGame()
    {
        Elephant.LevelStarted(LevelManager.Instance.GetCurrentLevelIndex());
        UpdateState(GameState.OnStart);
    }

    private void WinGame()
    {
        Elephant.LevelCompleted(LevelManager.Instance.GetCurrentLevelIndex());
        UpdateState(GameState.OnWin);
    }

    private void LoseGame()
    {
        Elephant.LevelFailed(LevelManager.Instance.GetCurrentLevelIndex());
        UpdateState(GameState.OnLose);
    }

    private void UpdateState(GameState state)
    {
        gameState = state;
    }

    private void OnEnable()
    {
        GameManager.OnGameStart += StartGame;
        GameManager.OnGameLose += LoseGame;
        GameManager.OnGameWin += WinGame;
    }

    private void OnDisable()
    {
        GameManager.OnGameStart -= StartGame;
        GameManager.OnGameLose -= LoseGame;
        GameManager.OnGameWin -= WinGame;
    }
}
