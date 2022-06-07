using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float timeInterval = 0.5f;
    [SerializeField] private Transform barrel;
    [SerializeField] private GameObject bullet;
    [SerializeField] private PlayerAimer player;
    [SerializeField] private List<Transform> walkPoints = new List<Transform>();
    [SerializeField] private float speed = 3f;
    private int pointIndex = 0;
    private float distance = 0;
    float timer = 0;
    //private Rigidbody rb;
    private void Start()
    {
        //rb = GetComponent<Rigidbody>();
    }
    private void Update()
    {
        if (!player.shooted)
            WalkAround();
        else
            ShootPlayer();
    }
    private void WalkAround()
    {
        distance = Vector3.Distance(transform.position, walkPoints[pointIndex].position);
        if (distance < 1f) pointIndex++;

        if (pointIndex >= walkPoints.Capacity) pointIndex = 0;

        transform.LookAt(walkPoints[pointIndex].position);
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

    }
    public void Dead()
    {
        Destroy(gameObject);
    }

    public void ShootPlayer()
    {   
        if(Time.time - timer > timeInterval)
        {
            transform.LookAt(player.transform.position + new Vector3(Random.Range(-4, 4), Random.Range(-4, 4), Random.Range(-2, 2)));
            Instantiate(bullet, barrel.position, barrel.rotation);
            timer = Time.time;
        }
    }

}
