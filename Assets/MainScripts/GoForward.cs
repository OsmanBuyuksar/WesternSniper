using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoForward : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 100f;
    [SerializeField] private float disappearTime = 2f;
    private void Start()
    {
        Destroy(gameObject, disappearTime);
    }
    private void Update()
    {
        transform.position += transform.forward * moveSpeed * Time.deltaTime;
    }
}
