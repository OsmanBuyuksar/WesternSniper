using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private Animator playerAnimator = null;

    private void Start()
    {
        // TODO
    }

    
    private void OnStart()
    {
        // TODO  
    }

    private void OnWin()
    {
        // TODO
    }

    
    private void OnLose()
    {
        // TODO
    }

    private void OnEnable()
    {
        GameManager.OnGameStart += OnStart;
        GameManager.OnGameWin += OnWin;
        GameManager.OnGameLose += OnLose;
    }

    private void OnDisable()
    {
        GameManager.OnGameStart -= OnStart;
        GameManager.OnGameWin -= OnWin;
        GameManager.OnGameLose -= OnLose;
    }
}