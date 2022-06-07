using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAimer : MonoBehaviour
{
    public bool shooted = false;
    [SerializeField] private Transform aimOrigin;
    [SerializeField] private Transform aimReticle;
    [SerializeField] private TouchInput input;
    [SerializeField] private float rotateClampValue = 30f;
    // Update is called once per frame
    void Update()
    {
        MoveReticle();
    }

    void MoveReticle()
    {
        aimOrigin.Rotate((new Vector3(-(input.vertical), input.horizontal,0)) * 0.2f);
        Ray ray = new Ray(aimOrigin.position, aimOrigin.forward);
        Debug.DrawLine(ray.origin, ray.origin + ray.direction * 100);
        //aimOrigin.rotation = Quaternion.Euler(new Vector3(Mathf.Clamp(aimOrigin.rotation.x, -rotateClampValue, rotateClampValue), Mathf.Clamp(aimOrigin.rotation.y, -rotateClampValue, rotateClampValue), aimOrigin.rotation.z));
    }
    public void Fire()
    {
        
        Ray ray = new Ray(aimOrigin.position, aimOrigin.forward);
        Debug.DrawRay(ray.origin, ray.direction);
        if (Physics.Raycast(ray, out RaycastHit hit, 200))
        {
            if (hit.collider.TryGetComponent<Enemy>(out Enemy enemy))
            {
                enemy.Dead();
            }
        }
        shooted = true;
    }
}
