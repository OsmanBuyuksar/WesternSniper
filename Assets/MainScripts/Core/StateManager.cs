using UnityEngine;

public class StateManager : Singleton<StateManager>
{
    [SerializeField]
    private GameState gameState = GameState.None;

    public GameState CurrentState => gameState;

    private void StartGame()
    {
        UpdateState(GameState.OnStart);
    }

    private void WinGame()
    {
        UpdateState(GameState.OnWin);
    }

    private void LoseGame()
    {
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
