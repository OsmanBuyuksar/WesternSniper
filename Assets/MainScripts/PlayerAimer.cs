using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAimer : MonoBehaviour
{
    [SerializeField] private Transform aimOrigin;
    [SerializeField] private Transform aimReticle;
    [SerializeField] private TouchInput input;
    [SerializeField] private float rotateClampValue = 30f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        MoveReticle();
    }

    void MoveReticle()
    {
        aimOrigin.Rotate((new Vector3(-(input.vertical), input.horizontal,0)) * 0.2f);
        //aimOrigin.rotation = Quaternion.Euler(new Vector3(Mathf.Clamp(aimOrigin.rotation.x, -rotateClampValue, rotateClampValue), Mathf.Clamp(aimOrigin.rotation.y, -rotateClampValue, rotateClampValue), aimOrigin.rotation.z));
    }
    public void Fire()
    {
        Ray ray = new Ray(aimOrigin.position, aimOrigin.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 200))
        {
            if (hit.collider.TryGetComponent<Enemy>(out Enemy enemy))
            {
                enemy.Dead();
            }
        }
    }
}
