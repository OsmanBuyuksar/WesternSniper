using FluffyUnderware.Curvy.Controllers;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] SplineController mainSpline = null;
    [SerializeField] float moveSpeed = 10;

    private void Start()
    {
        mainSpline = GetComponentInParent<SplineController>();
    }

    private void OnTriggerEnter(Collider other)
    {
       if(other.TryGetComponent(out FinishController finish))
        {
            GameManager.OnGameWin?.Invoke();
        }
    }

    private void OnStart()
    {
        mainSpline.Speed = moveSpeed;
    }


    private void OnWin()
    {
        mainSpline.Speed = 0;
    }


    private void OnLose()
    {
        mainSpline.Speed = 0;
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