using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float forwardSpeed = 0f;
    [SerializeField] float horizontalSpeed = 2f;
    [SerializeField] Transform playerModel; // AÇI DÖNME VE GİBİ İŞLEMLER İÇİN...
    [SerializeField] float workingArea = 0;

    private bool hasFinishGame = false;
    private TouchInput input = null;

    [Header("@ROTATION OPTIONS")]
    [SerializeField] bool playerRotation = false;
    [SerializeField] bool roadRotation = false;
    [SerializeField] float rotationLimit = 0;

    private void Start()
    {
        input = GetComponent<TouchInput>();
    }

    private void Update()
    {
        if (!hasFinishGame)
        {
            Move();
        }
    }

    private void Move()
    {
        transform.localPosition += Vector3.right * input.horizontal * horizontalSpeed * Time.deltaTime;

        if (playerRotation)
        {
            float angle = Mathf.Atan2(input.horizontal, 0) * Mathf.Rad2Deg;
            angle = Mathf.Clamp(angle, -rotationLimit, rotationLimit);
            var rot = Quaternion.Euler(new Vector3(0, angle, 0));
            transform.localRotation = Quaternion.Lerp(transform.localRotation, rot, 0.2f);
        }

        if (roadRotation)
        {
            transform.localRotation = transform.parent.localRotation;
        }

        var pos = transform.localPosition;
        pos.x = Mathf.Clamp(transform.localPosition.x, -workingArea, workingArea);
        transform.localPosition = pos;
    }

    private void OnStart()
    {

    }

    private void OnWin()
    {
        hasFinishGame = true;
    }

    private void OnLose()
    {
        hasFinishGame = true;
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